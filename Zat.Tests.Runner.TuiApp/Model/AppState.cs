namespace Zat.Tests.Runner.TuiApp.Model;

using System.Text.Json.Serialization;

using Zat.Z2xxTests.Common;

internal record AppState(
    [property: JsonPropertyName("runtimeVersion")]
    string? RuntimeVersion,
    [property: JsonPropertyName("runtimeReleaseDate")]
    string? RuntimeReleaseDate,
    [property: JsonPropertyName("ideVersion")]
    string? IdeVersion,
    [property: JsonPropertyName("ideReleaseDate")]
    string? IdeReleaseDate,
    [property: JsonPropertyName("testedHwAssemblyTypes")]
    TestedHwAssemblyType[]? HwAssemblyTypes,
    [property: JsonPropertyName("isTestLinkReportingEnabled")]
    bool? IsTestLinkReportingEnabled);