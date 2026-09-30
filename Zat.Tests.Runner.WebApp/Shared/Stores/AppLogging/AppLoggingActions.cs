namespace Zat.Tests.Runner.WebApp.Shared.Stores.AppLogging;

using Zat.Tests.Runner.WebApp.Shared.Model;

public record DataChangedAction(
    ValueChange<LogEntry>? NewEntry);