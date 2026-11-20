namespace Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

using System.Text.Json.Serialization;
using Fluxor;
using Zat.Tests.Runner.WebApp.Shared.Model;

[FeatureState]
public record AppLoggingState
{
    [JsonIgnore]
    public IReadOnlyList<LogEntry> Entries { get; init; } = [];
}