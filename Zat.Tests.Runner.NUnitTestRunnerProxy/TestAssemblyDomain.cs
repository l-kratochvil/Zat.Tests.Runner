namespace Zat.Tests.Runner.NUnitTestRunnerProxy;

/// <summary>
/// The AppDomain one test assembly is loaded and run in, set up as if the test assembly were the application: its
/// directory is the application base and its <c>.dll.config</c> the configuration file, so the binding redirects and
/// settings of the test assembly apply. Disposing unloads the AppDomain.
/// </summary>
/// <param name="domain">The AppDomain of the test assembly.</param>
/// <param name="runner">The runner living in <paramref name="domain"/>.</param>
internal sealed class TestAssemblyDomain(AppDomain domain, TestAssemblyRunner runner) : IDisposable
{
    /// <summary>Gets the runner living in the AppDomain of the test assembly.</summary>
    public TestAssemblyRunner Runner { get; } = runner;

    /// <summary>Creates the AppDomain of the test assembly at <paramref name="assemblyDllPath"/>.</summary>
    /// <param name="assemblyDllPath">The path of the test assembly.</param>
    /// <returns>The created AppDomain with a runner in it.</returns>
    public static TestAssemblyDomain Create(string assemblyDllPath)
    {
        var fullAssemblyDllPath = Path.GetFullPath(assemblyDllPath);
        var configurationFilePath = fullAssemblyDllPath + ".config";

        var setup = new AppDomainSetup
        {
            ApplicationName = Path.GetFileNameWithoutExtension(fullAssemblyDllPath),
            ApplicationBase = Path.GetDirectoryName(fullAssemblyDllPath),
            ConfigurationFile = File.Exists(configurationFilePath) ? configurationFilePath : null,
        };

        var domain = AppDomain.CreateDomain(setup.ApplicationName, AppDomain.CurrentDomain.Evidence, setup);
        try
        {
            // Loaded from the proxy's own path: assemblies the proxy needs and the test assembly lacks are probed
            // next to the proxy, while those the test assembly ships (e.g. nunit.framework) take precedence.
            var runner = (TestAssemblyRunner)domain.CreateInstanceFromAndUnwrap(
                typeof(TestAssemblyRunner).Assembly.Location,
                typeof(TestAssemblyRunner).FullName);

            return new TestAssemblyDomain(domain, runner);
        }
        catch
        {
            AppDomain.Unload(domain);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => AppDomain.Unload(domain);
}