namespace NUnitTestAssembly.Net481;

using System.Configuration;

using NUnit.Framework;

/// <summary>
/// A test fixture that passes only when the configuration file of this test assembly (App.config) applies to it.
/// </summary>
[TestFixture]
public class ConfigurationFileFixture
{
    /// <summary>
    /// The value the configuration file of this test assembly holds under <c>ConfigurationFileFixture.Value</c>.
    /// </summary>
    public const string ExpectedValue = "FROM TEST ASSEMBLY CONFIGURATION FILE";

    /// <summary>
    /// Reads a value from the configuration file of this test assembly.
    /// </summary>
    [Test]
    public void ReadsTestAssemblyConfigurationFile()
    {
        Assert.That(ConfigurationManager.AppSettings["ConfigurationFileFixture.Value"], Is.EqualTo(ExpectedValue));
    }
}