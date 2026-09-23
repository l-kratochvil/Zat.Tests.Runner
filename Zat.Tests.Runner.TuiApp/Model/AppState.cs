namespace Zat.Tests.Runner.TuiApp.Model;

using System.Text.Json.Serialization;

using Zat.Z2xxTests.Common.Model;

internal record AppState(
    [property: JsonPropertyName("runtimeVersion")]
    string? RuntimeVersion,
    [property: JsonPropertyName("runtimeReleaseDate")]
    string? RuntimeReleaseDate,
    [property: JsonPropertyName("ideVersion")]
    string? IdeVersion,
    [property: JsonPropertyName("ideReleaseDate")]
    string? IdeReleaseDate,
    [property: JsonPropertyName("betaVersion")]
    string? BetaVersion,
    [property: JsonPropertyName("testedHwAssemblyTypes")]
    HwAssemblyType[]? HwAssemblyTypes,
    [property: JsonPropertyName("isTestLinkReportingEnabled")]
    bool? IsTestLinkReportingEnabled,
    [property: JsonPropertyName("isDebugModeEnabled")]
    bool? IsDebugModeEnabled,
    [property: JsonPropertyName("isBetaVersion")]
    bool? IsBetaVersion)
{
    public AppState() : this(null, null, null, null, null, null, null, null, null)
    {
    }
}