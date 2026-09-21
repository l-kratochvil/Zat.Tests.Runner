namespace Zat.Tests.Runner.TuiApp.Model;

using System.Text.Json.Serialization;

using Zat.Z2xxTests.Common;

internal record AppState(
    [property: JsonPropertyName("runtimeVersion")]
    string? RuntimeVersion,
    [property: JsonPropertyName("ideVersion")]
    string? IdeVersion,
    [property: JsonPropertyName("testedHwAssemblyType")]
    TestedHwAssemblyType? TestedHwAssemblyType);