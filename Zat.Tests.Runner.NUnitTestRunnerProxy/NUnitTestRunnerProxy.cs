namespace Zat.Tests.Runner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevKit.Core.Extensions.Types;

using NUnit;
using NUnit.Framework;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Services;

using NUnitTestStatus = NUnit.Framework.Interfaces.TestStatus;
using TestCaseResult = Zat.Tests.Runner.Common.Model.TestCaseResult;
using TestFilter = NUnit.Framework.Internal.TestFilter;
using TestStatus = Zat.Tests.Runner.Common.Model.TestStatus;
using TestSuiteResult = Zat.Tests.Runner.Common.Model.TestSuiteResult;

/// <summary>
/// Out-of-process NUnit test runner. Hosts the .NET Framework <see cref="ITestAssemblyRunner"/>
/// and is exposed to the application over StreamJsonRpc.
/// </summary>
public sealed class NUnitTestRunnerProxy : INUnitTestRunnerProxy
{
    private const string NotRunMessage = "Not run";

    // TODO: NEMAPOVAT! Klient si názvy testovacích sad mapuje sám.
    private static readonly ImmutableDictionary<string, string> TestSuiteNamesMap =
        new Dictionary<string, string>
        {
            { "PdpClientTests", "Testy PDP klient" },
            { "Pertinax6Tests", "Testy Pertinax6" },
            { "RuntimeTests", "Testy Runtime" },
        }.ToImmutableDictionary();

    private readonly NUnitTestAssemblyRunner runner = new(new DefaultTestAssemblyBuilder());

    /// <inheritdoc/>
    public Task<bool> GetIsAssemblyLoadedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(this.runner.IsTestLoaded);

    /// <inheritdoc/>
    public Task<bool> GetIsTestRunningAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(this.runner.IsTestRunning);

    /// <inheritdoc/>
    public Task<TestSuiteEntity[]> LoadTestAssemblyAsync(
        string assemblyDllPath, CancellationToken cancellationToken = default)
        => Task.Run<TestSuiteEntity[]>(
            () =>
            {
                // Discover tests through the in-process NUnitTestAssemblyRunner.
                // NOTE: this runner calls Assembly.Load and therefore requires the
                // test assembly's bitness to match this host. Runtime test libraries
                // such as Zat.Z2xxTests.dll are x86, so this host must run as a 32-bit
                // process (see <PlatformTarget>x86</PlatformTarget> in the proxy/test
                // project); otherwise the assembly is reported as NotRunnable with a
                // BadImageFormatException and no tests are discovered.
                var testAssemblyElement = this.runner.Load(assemblyDllPath, new Dictionary<string, object>
                {
                    { FrameworkPackageSettings.WorkDirectory, Path.GetDirectoryName(assemblyDllPath) },
                });

                return testAssemblyElement.Tests.Any() ? [.. CollectTestSuiteEntities(testAssemblyElement.Tests[0])] : [];
            },
            cancellationToken);

    /// <inheritdoc/>
    public Task<ProxyTestResult> RunTestAsync(
        IEnumerable<TestEntity> testRunEntities, CancellationToken cancellationToken = default)
    {
        if (!this.runner.IsTestLoaded)
        {
            throw new InvalidOperationException("Test assembly wasn't loaded yet");
        }

        // Explicit OR semantics: a test runs if it matches ANY of the requested names.
        // (Multiple <test> elements directly under <filter> would be combined with AND.)
        var testFilterNode = new TNode("filter");
        var orNode = testFilterNode.AddElement("or");
        foreach (var testEntity in testRunEntities)
        {
            orNode.AddElement("test", testEntity.ExecutionPath);
        }

        var testFilter = TestFilter.FromXml(testFilterNode);

        // Cancellation forcibly aborts the in-progress run.
        var cancellationRegistration = cancellationToken.Register(() => this.runner.StopRun(force: true));

        return Task.Run(
            () =>
            {
                try
                {
                    var result = this.runner.Run(TestListener.NULL, testFilter);
                    var loadedTest = this.runner.LoadedTest;

                    return new ProxyTestResult(
                        loadedTest.Tests.Any()
                            ? [.. CollectTestSuiteResults(loadedTest.Tests[0], testFilter, IndexByFullName(result))]
                            : []);
                }
                finally
                {
                    cancellationRegistration.Dispose();
                }
            },
            cancellationToken);
    }

    /// <summary>
    /// Maps an NUnit <paramref name="resultState"/> to a <see cref="TestStatus"/>; an outcome not known here maps to
    /// <see cref="TestStatus.Unknown"/>.
    /// </summary>
    private static TestStatus MapStatus(ResultState resultState)
        => resultState.Status switch
        {
            NUnitTestStatus.Passed => TestStatus.Passed,
            NUnitTestStatus.Inconclusive => TestStatus.Inconclusive,
            NUnitTestStatus.Warning => TestStatus.Warning,
            NUnitTestStatus.Failed when resultState.Label == ResultState.Error.Label => TestStatus.Error,
            NUnitTestStatus.Failed when resultState.Label == ResultState.NotRunnable.Label => TestStatus.Invalid,
            NUnitTestStatus.Failed when resultState.Label == ResultState.Cancelled.Label => TestStatus.Error,
            NUnitTestStatus.Failed => TestStatus.Failure,
            NUnitTestStatus.Skipped when resultState.Label == ResultState.Ignored.Label => TestStatus.Ignored,
            NUnitTestStatus.Skipped when resultState.Label == ResultState.Explicit.Label => TestStatus.Explicit,
            NUnitTestStatus.Skipped => TestStatus.Skipped,
            _ => TestStatus.Unknown,
        };

    private static Detail? CreateDetail(ITestResult result)
        => string.IsNullOrEmpty(result.Message) && string.IsNullOrEmpty(result.StackTrace)
            ? null
            : new Detail(result.Message ?? string.Empty, result.StackTrace);

    /// <summary>
    /// Creates the <see cref="TestStatus"/> and <see cref="Detail"/> of a test suite or test fixture from its intrinsic
    /// outcome only: an outcome aggregated from its children or a missing <paramref name="result"/> is
    /// <see cref="TestStatus.Passed"/>.
    /// </summary>
    private static (TestStatus Status, Detail? Detail) CreateGroupOutcome(ITestResult? result)
    {
        if (result is null || result.ResultState.Site is FailureSite.Child)
        {
            return (TestStatus.Passed, null);
        }

        var status = MapStatus(result.ResultState);
        return status is TestStatus.Passed ? (status, null) : (status, CreateDetail(result));
    }

    private static ITestResult? FindResult(IReadOnlyDictionary<string, ITestResult> results, ITest test)
        => results.TryGetValue(test.FullName, out var result) ? result : null;

    private static Dictionary<string, ITestResult> IndexByFullName(ITestResult root)
    {
        var index = new Dictionary<string, ITestResult>();
        var pending = new Stack<ITestResult>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var result = pending.Pop();

            if (!index.ContainsKey(result.FullName))
            {
                index[result.FullName] = result;
            }

            foreach (var child in result.Children)
            {
                pending.Push(child);
            }
        }

        return index;
    }

    /// <summary>
    /// Builds the result tree from the loaded test tree filtered by <paramref name="testFilter"/>, so that a requested
    /// test suite or test fixture expands to its test cases and a test case missing from <paramref name="results"/>
    /// (e.g. after a cancelled run) is still present.
    /// </summary>
    private static IEnumerable<TestSuiteResult> CollectTestSuiteResults(
        ITest root, TestFilter testFilter, IReadOnlyDictionary<string, ITestResult> results)
    {
        foreach (var testSuite in root.Tests.OfType<TestSuite>().Where(x => testFilter.Pass(x)))
        {
            var (status, detail) = CreateGroupOutcome(FindResult(results, testSuite));
            yield return new TestSuiteResult(
                [.. CollectTestFixtureResults(testSuite, testFilter, results)],
                testSuite.FullName,
                status,
                detail);
        }
    }

    private static IEnumerable<TestFixtureResult> CollectTestFixtureResults(
        TestSuite testSuite, TestFilter testFilter, IReadOnlyDictionary<string, ITestResult> results)
    {
        foreach (var testFixture in testSuite.Tests.OfType<TestFixture>().Where(x => testFilter.Pass(x)))
        {
            var (status, detail) = CreateGroupOutcome(FindResult(results, testFixture));
            yield return new TestFixtureResult(
                [.. CollectTestCaseResults(testFixture, testFilter, results)],
                testFixture.FullName,
                status,
                detail);
        }
    }

    private static IEnumerable<TestCaseResult> CollectTestCaseResults(
        TestFixture testFixture, TestFilter testFilter, IReadOnlyDictionary<string, ITestResult> results)
    {
        foreach (var testCase in testFixture.Tests.OfType<Test>().Where(x => testFilter.Pass(x)))
        {
            var id = GetTestCaseId(testCase);
            yield return results.TryGetValue(testCase.FullName, out var result)
                ? new TestCaseResult(
                    id,
                    EntityName: testCase.FullName,
                    MapStatus(result.ResultState),
                    CreateDetail(result))
                : new TestCaseResult(
                    id,
                    EntityName: testCase.FullName,
                    TestStatus.Unknown,
                    new Detail(NotRunMessage, StackTrace: null));
        }
    }

    /// <summary>
    /// Gets the <see cref="TestCaseEntity.Id"/> of <paramref name="testCase"/>, shared by test discovery and result
    /// collection so that both always agree.
    /// </summary>
    private static string GetTestCaseId(Test testCase)
        => testCase
            .Method?
            .MethodInfo
            .GetAttribute<TestCaseAttribute>()?
            .TestName ?? testCase.Name;

    private static IEnumerable<TestSuiteEntity> CollectTestSuiteEntities(ITest root)
        => root.Tests.OfType<TestSuite>().Select(static x =>
        {
            // TODO: Určit "je to runtime test" podle atributu (umístěného do Zat.Z2xxTests.Common)
            var testType = x.FullName.ToLower().Contains("runtimetests")
                ? TestType.Runtime
                : TestType.Application;
            return new TestSuiteEntity(
                [.. CollectTestFixtureEntities(x, testType)],
                testType,
                name: TestSuiteNamesMap.TryGetValue(x.Name, out var testSuiteName) ? testSuiteName : x.Name,
                executionPath: x.FullName);
        });

    private static IEnumerable<TestFixtureEntity> CollectTestFixtureEntities(
        TestSuite testSuite, TestType testType)
    {
        foreach (var testFixture in testSuite.Tests.OfType<TestFixture>())
        {
            var testFixtureName = testFixture
                .TypeInfo
                .Type
                .GetAttribute<TestFixtureAttribute>()?
                .Description ?? testFixture.Name;
            yield return new TestFixtureEntity(
                [.. CollectTestCaseEntities(testFixture, testType)],
                testType,
                name: testFixtureName,
                executionPath: testFixture.FullName);
        }
    }

    private static IEnumerable<TestCaseEntity> CollectTestCaseEntities(
        TestFixture testFixture, TestType testType)
    {
        foreach (var testCase in testFixture.Tests.OfType<Test>())
        {
            yield return new TestCaseEntity(
                testType,
                id: GetTestCaseId(testCase),
                name: testCase.Name,
                executionPath: testCase.FullName);
        }
    }
}