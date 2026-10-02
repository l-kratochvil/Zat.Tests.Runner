namespace Zat.Tests.Runner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Linq;

using DevKit.Core.Extensions.Types;

using NUnit;
using NUnit.Framework;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using Zat.Tests.Runner.Common.Model;

using NUnitTestStatus = NUnit.Framework.Interfaces.TestStatus;
using TestCaseResult = Zat.Tests.Runner.Common.Model.TestCaseResult;
using TestFilter = NUnit.Framework.Internal.TestFilter;
using TestStatus = Zat.Tests.Runner.Common.Model.TestStatus;
using TestSuiteResult = Zat.Tests.Runner.Common.Model.TestSuiteResult;

/// <summary>
/// Discovers and runs the tests of one test assembly inside the AppDomain of that test assembly, see
/// <see cref="TestAssemblyDomain"/>. Everything it accepts and returns crosses the AppDomain boundary, so it is
/// either a primitive or <see cref="SerializableAttribute"/>.
/// </summary>
public sealed class TestAssemblyRunner : MarshalByRefObject
{
    private const string NotRunMessage = "Not run";

    private readonly NUnitTestAssemblyRunner runner = new(new DefaultTestAssemblyBuilder());

    /// <summary>Gets a value indicating whether the test assembly is loaded.</summary>
    public bool IsTestLoaded
        => this.runner.IsTestLoaded;

    /// <summary>Gets a value indicating whether a test run is in progress.</summary>
    public bool IsTestRunning
        => this.runner.IsTestRunning;

    /// <summary>Lives as long as its AppDomain instead of expiring after the default remoting lease.</summary>
    /// <returns><see langword="null"/>, meaning an infinite lease.</returns>
    public override object? InitializeLifetimeService()
        => null;

    /// <summary>Loads the test assembly and returns its discovered test tree.</summary>
    /// <param name="assemblyDllPath">The path of the test assembly.</param>
    /// <returns>The test suites of the test assembly.</returns>
    public TestSuiteEntity[] Load(string assemblyDllPath)
    {
        // NOTE: this runner calls Assembly.Load and therefore requires the
        // test assembly's bitness to match this host. Runtime test libraries
        // such as Zat.Z2xxTests.dll are x86, so this host must run as a 32-bit
        // process (see <PlatformTarget>x86</PlatformTarget> in the proxy/test
        // project); otherwise the assembly is reported as NotRunnable with a
        // BadImageFormatException and no tests are discovered.
        var testAssemblyElement = this.runner.Load(
            assemblyDllPath,
            new Dictionary<string, object>
            {
                { FrameworkPackageSettings.WorkDirectory, Path.GetDirectoryName(assemblyDllPath) },
            });

        return [.. CollectTestSuiteEntities(testAssemblyElement)];
    }

    /// <summary>Runs the test entities identified by <paramref name="executionPaths"/>.</summary>
    /// <param name="executionPaths">The execution paths of the test entities to run.</param>
    /// <returns>The result tree of the run.</returns>
    public ProxyTestResult Run(string[] executionPaths)
    {
        if (!this.runner.IsTestLoaded)
        {
            throw new InvalidOperationException("Test assembly wasn't loaded yet");
        }

        // Explicit OR semantics: a test runs if it matches ANY of the requested names.
        // (Multiple <test> elements directly under <filter> would be combined with AND.)
        var testFilterNode = new TNode("filter");
        var orNode = testFilterNode.AddElement("or");
        foreach (var executionPath in executionPaths)
        {
            orNode.AddElement("test", executionPath);
        }

        var testFilter = TestFilter.FromXml(testFilterNode);

        var result = this.runner.Run(TestListener.NULL, testFilter);

        return new ProxyTestResult(
            [.. CollectTestSuiteResults(this.runner.LoadedTest, testFilter, IndexByFullName(result))]);
    }

    /// <summary>Forcibly aborts the in-progress run.</summary>
    public void StopRun()
        => this.runner.StopRun(force: true);

    /// <summary>
    /// Maps an NUnit <paramref name="resultState"/> to a <see cref="TestStatus"/>; an outcome not known here maps to
    /// <see cref="TestStatus.Unknown"/>.
    /// </summary>
    private static TestStatus MapStatus(ResultState resultState)
        => resultState.Status switch
        {
            NUnitTestStatus.Passed
                => TestStatus.Passed,
            NUnitTestStatus.Inconclusive
                => TestStatus.Inconclusive,
            NUnitTestStatus.Warning
                => TestStatus.Warning,
            NUnitTestStatus.Failed when resultState.Label == ResultState.Error.Label
                => TestStatus.Error,
            NUnitTestStatus.Failed when resultState.Label == ResultState.NotRunnable.Label
                => TestStatus.Invalid,
            NUnitTestStatus.Failed when resultState.Label == ResultState.Cancelled.Label
                => TestStatus.Skipped,
            NUnitTestStatus.Failed
                => TestStatus.Failure,
            NUnitTestStatus.Skipped when resultState.Label == ResultState.Ignored.Label
                => TestStatus.Ignored,
            NUnitTestStatus.Skipped when resultState.Label == ResultState.Explicit.Label
                => TestStatus.Explicit,
            NUnitTestStatus.Skipped
                => TestStatus.Skipped,
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
        foreach (var testSuite in FindTestSuites(root).Where(x => testFilter.Pass(x)))
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

    /// <summary>
    /// Finds the test suites under <paramref name="root"/>: every namespace level that directly holds a test fixture,
    /// however deep it is nested.
    /// </summary>
    private static IEnumerable<TestSuite> FindTestSuites(ITest root)
    {
        foreach (var testSuite in root.Tests
            .OfType<TestSuite>()
            .Where(static x => x is not TestFixture and not ParameterizedFixtureSuite))
        {
            if (testSuite.Tests.OfType<TestFixture>().Any())
            {
                yield return testSuite;
            }

            foreach (var nestedTestSuite in FindTestSuites(testSuite))
            {
                yield return nestedTestSuite;
            }
        }
    }

    private static IEnumerable<TestSuiteEntity> CollectTestSuiteEntities(ITest root)
        => FindTestSuites(root).Select(static x =>
        {
            // TODO: Určit "je to runtime test" podle atributu (umístěného do Zat.Z2xxTests.Common)
            var testType = x.FullName.ToLower().Contains("runtimetests")
                ? TestType.Runtime
                : TestType.Application;
            return new TestSuiteEntity(
                [.. CollectTestFixtureEntities(x, testType)],
                testType,
                name: x.Name,
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