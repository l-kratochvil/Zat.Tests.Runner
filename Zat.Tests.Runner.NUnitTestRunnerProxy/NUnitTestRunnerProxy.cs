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

using TestFilter = NUnit.Framework.Internal.TestFilter;
using TestStatus = Common.Model.TestStatus;

/// <summary>
/// Out-of-process NUnit test runner. Hosts the .NET Framework <see cref="ITestAssemblyRunner"/>
/// and is exposed to the application over StreamJsonRpc.
/// </summary>
public sealed class NUnitTestRunnerProxy : INUnitTestRunnerProxy
{
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

                return testAssemblyElement.Tests.Any() ? [..CollectTestSuiteEntities(testAssemblyElement.Tests[0])] : [];
            },
            cancellationToken);

    /// <inheritdoc/>
    public Task<Common.ProxyTestResult> RunTestAsync(
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

                    var buckets = new ResultBuckets();
                    CollectResults(result, buckets);

                    var summary = new Common.TestRunSummary(
                        Total: result.PassCount + result.FailCount + result.WarningCount +
                               result.InconclusiveCount + result.SkipCount,
                        Passed: result.PassCount,
                        Failed: result.FailCount,
                        Warnings: result.WarningCount,
                        Inconclusive: result.InconclusiveCount,
                        Skipped: result.SkipCount,
                        Failures: buckets.Failures.Count,
                        Errors: buckets.Errors.Count,
                        Invalid: buckets.Invalid.Count,
                        Ignored: buckets.Ignored.Count,
                        Explicit: buckets.Explicit.Count,
                        Other: buckets.Other.Count);

                    return new Common.ProxyTestResult(
                        result.ResultState.Status switch
                        {
                            NUnit.Framework.Interfaces.TestStatus.Passed => TestStatus.Passed,
                            NUnit.Framework.Interfaces.TestStatus.Failed => TestStatus.Failed,
                            NUnit.Framework.Interfaces.TestStatus.Skipped => TestStatus.Skipped,
                            NUnit.Framework.Interfaces.TestStatus.Inconclusive => TestStatus.Inconclusive,
                            NUnit.Framework.Interfaces.TestStatus.Warning => TestStatus.Warning,
                            _ => TestStatus.Unknown,
                        },
                        summary,
                        [..buckets.Ignored],
                        [..buckets.Explicit],
                        [..buckets.Other],
                        [..buckets.Errors],
                        [..buckets.Invalid],
                        [..buckets.Failures],
                        [..buckets.Warnings]);
                }
                finally
                {
                    cancellationRegistration.Dispose();
                }
            },
            cancellationToken);
    }

    /// <summary>
    /// Recursively walks the NUnit result tree and buckets outcomes into the not-run
    /// (ignored/explicit/other), error, invalid, failure and warning collections. Results whose
    /// outcome is merely propagated from a parent's setup or aggregated from children
    /// (<see cref="FailureSite.Parent"/>/<see cref="FailureSite.Child"/>) are skipped, so each
    /// intrinsic outcome is reported exactly once (e.g. a suite's own OneTimeSetUp error is reported
    /// at the suite node, not duplicated onto its children). Skipped and warning outcomes are only
    /// taken from leaf test cases to avoid duplicating a fixture-level outcome across its children.
    /// </summary>
    private static void CollectResults(ITestResult result, ResultBuckets buckets)
    {
        var resultState = result.ResultState;
        var isPropagated = resultState.Site is FailureSite.Parent or FailureSite.Child;

        if (!isPropagated)
        {
            switch (resultState.Status)
            {
                case NUnit.Framework.Interfaces.TestStatus.Failed when resultState.Label == "Error":
                    buckets.Errors.Add(new Common.ErrorResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Failed when resultState.Label == "Invalid":
                    buckets.Invalid.Add(new Common.InvalidResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Failed:
                    buckets.Failures.Add(new Common.FailureResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Warning when !result.HasChildren:
                    buckets.Warnings.Add(new Common.WarningResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Skipped when !result.HasChildren
                                                                        && resultState.Label == "Ignored":
                    buckets.Ignored.Add(new Common.IgnoredResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Skipped when !result.HasChildren
                                                                        && resultState.Label == "Explicit":
                    buckets.Explicit.Add(new Common.ExplicitResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Skipped when !result.HasChildren:
                    buckets.Other.Add(new Common.OtherSkippedResult(
                        result.FullName, result.Message ?? string.Empty, result.StackTrace ?? string.Empty));
                    break;

                case NUnit.Framework.Interfaces.TestStatus.Warning:
                case NUnit.Framework.Interfaces.TestStatus.Skipped:
                case NUnit.Framework.Interfaces.TestStatus.Inconclusive:
                case NUnit.Framework.Interfaces.TestStatus.Passed:
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        if (!result.HasChildren)
        {
            return;
        }

        foreach (var child in result.Children)
        {
            CollectResults(child, buckets);
        }
    }

    /// <summary>Mutable accumulator for the categorized outcomes gathered during the result walk.</summary>
    private sealed class ResultBuckets
    {
        public List<Common.IgnoredResult> Ignored { get; } = [];

        public List<Common.ExplicitResult> Explicit { get; } = [];

        public List<Common.OtherSkippedResult> Other { get; } = [];

        public List<Common.ErrorResult> Errors { get; } = [];

        public List<Common.InvalidResult> Invalid { get; } = [];

        public List<Common.FailureResult> Failures { get; } = [];

        public List<Common.WarningResult> Warnings { get; } = [];
    }

    private static IEnumerable<TestSuiteEntity> CollectTestSuiteEntities(ITest root)
        => root.Tests.OfType<TestSuite>().Select(static x =>
        {
            // TODO: Určit "je to runtime test" podle atributu (umístěného do Zat.Z2xxTests.Common)
            var testType = x.FullName.ToLower().Contains("runtimetests")
                ? TestType.Runtime
                : TestType.Application;
            return new TestSuiteEntity(
                [..CollectTestFixtureEntities(x, testType)],
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
                [..CollectTestCaseEntities(testFixture, testType)],
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
            var testCaseId = testCase
                .Method?
                .MethodInfo
                .GetAttribute<TestCaseAttribute>()?
                .TestName ?? testCase.Name;
            yield return new TestCaseEntity(
                testType,
                id: testCaseId,
                name: testCase.Name,
                executionPath: testCase.FullName);
        }
    }
}