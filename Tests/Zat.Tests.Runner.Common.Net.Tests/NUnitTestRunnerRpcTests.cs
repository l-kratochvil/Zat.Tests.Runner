namespace Zat.Tests.Runner.Common.Net.Tests;

using System.Reflection;

using DevKit.Core.Extensions.Types;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Application.Paths;
using Zat.Tests.Runner.Common.Net.Services;

[TestFixture]
public class NUnitTestRunnerRpcTests
{
    private const string TestAssemblyNet461Name = "NUnitTestAssembly.Net461";
    private const string TestAssemblyNet481Name = "NUnitTestAssembly.Net481";

    private static readonly string NUnitTestAssembliesDirPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetAssemblyDirectoryPath(),
        "NUnitTestAssemblies");

    private static readonly string TestAssemblyNet461DllPath = Path.Combine(
        NUnitTestAssembliesDirPath,
        TestAssemblyNet461Name,
        $"{TestAssemblyNet461Name}.dll");

    private static readonly string TestAssemblyNet481DllPath = Path.Combine(
        NUnitTestAssembliesDirPath,
        TestAssemblyNet481Name,
        $"{TestAssemblyNet481Name}.dll");

    private readonly AppPathsProvider appPathsProvider = new(TestContext.CurrentContext.TestDirectory);

    private NUnitTestRunnerProxyConnector connector = null!;

    [SetUp]
    public async Task SetUp()
    {
        this.connector = await NUnitTestRunnerProxyConnector.ConnectAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await this.connector.DisposeAsync();
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet461Assembly()
    {
        var testAssemblyDllPath = TestAssemblyNet461DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = this.connector.Proxy;

        // When
        var result = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);

        // Then
        AssertTestTreeLoaded(result);
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet481Assembly()
    {
        var testAssemblyDllPath = TestAssemblyNet481DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = this.connector.Proxy;

        // When
        var result = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);

        // Then
        AssertTestTreeLoaded(result);
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithZatTestsAssembly()
    {
        // TODO: "Test libs" folder content should be copied to the output folder of this test project, so that the test can run on any machine without manual setup.
        var zatTestsAssemblyPath = Path.Combine(this.appPathsProvider.Files.MainAssemblyDll);

        if (!File.Exists(zatTestsAssemblyPath))
        {
            throw new FileNotFoundException(zatTestsAssemblyPath);
        }

        // Given
        var unit = this.connector.Proxy;

        // When
        var result = await unit.LoadTestAssemblyAsync(zatTestsAssemblyPath);

        // Then
        AssertTestTreeLoaded(result);
    }

    private static void AssertTestTreeLoaded(TestSuiteEntity[] testSuites)
    {
        var testFixtures = testSuites.SelectMany(x => x.TestFixtures).ToArray();
        var testCases = testFixtures.SelectMany(x => x.TestCases).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(testSuites, Is.Not.Empty);
            Assert.That(testFixtures, Is.Not.Empty);
            Assert.That(testCases, Is.Not.Empty);
        }
    }
}