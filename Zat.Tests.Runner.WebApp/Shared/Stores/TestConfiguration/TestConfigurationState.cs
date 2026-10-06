namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

using System.Text.Json.Serialization;

using Fluxor;

using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The test run a tester is putting together, apart from the test selection.
/// </summary>
/// <remarks>
/// Belongs to one browser and is remembered there.
/// </remarks>
/// <param name="IsTestLinkReportEnabled">Whether the test result is written to TestLink.</param>
/// <param name="IsBetaVersion">Whether the tested build is a beta, filed so in TestLink.</param>
/// <param name="IsDebugModeEnabled">Whether the test run is debugged.</param>
/// <param name="IdeVersion">
/// IDE version the test result is filed under in TestLink. Only validated versions are kept here.
/// </param>
/// <param name="TestedHwAssembly">Test station the test run uses.</param>
/// <param name="RuntimeVersion">
/// Runtime version the test run uses. This is the installation folder name, because that is the
/// runtime version identifier elsewhere in the application.
/// </param>
/// <param name="RuntimeReleaseDate">Release date of the runtime, noted in TestLink.</param>
/// <param name="IdeReleaseDate">Release date of the IDE, noted in TestLink.</param>
/// <param name="BetaVersion">Number of the beta, noted in TestLink.</param>
[FeatureState]
public record TestConfigurationState(
    bool IsTestLinkReportEnabled,
    bool IsBetaVersion,
    bool IsDebugModeEnabled,
    Version? IdeVersion,
    HwAssemblyType? TestedHwAssembly,
    string? RuntimeVersion,
    string? RuntimeReleaseDate,
    string? IdeReleaseDate,
    string? BetaVersion)
{
    public TestConfigurationState()
        : this(
            IsTestLinkReportEnabled: false,
            IsBetaVersion: false,
            IsDebugModeEnabled: false,
            IdeVersion: null,
            TestedHwAssembly: null,
            RuntimeVersion: null,
            RuntimeReleaseDate: null,
            IdeReleaseDate: null,
            BetaVersion: null)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the configuration has errors.
    /// </summary>
    [JsonIgnore]
    public bool HasErrors { get; init; }
}