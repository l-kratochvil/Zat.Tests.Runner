namespace Zat.Tests.Runner.Common.Net.Tests;

using DevKit.Core.Extensions.Types;

using NUnit.Framework;

using System.Reflection;

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
        Assert.That(result, Is.Not.Empty);
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
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithZatTestsAssembly()
    {
        var zatTestsAssemblyPath = Path.Combine(@"C:\Automized tests\Test libs\", "Zat.Z2xxTests.dll");

        if (!File.Exists(zatTestsAssemblyPath))
        {
            throw new FileNotFoundException(zatTestsAssemblyPath);
        }

        // Given
        var unit = this.connector.Proxy;

        // When
        var result = await unit.LoadTestAssemblyAsync(zatTestsAssemblyPath);

        // Then
        Assert.That(result, Is.Not.Empty);
    }
}