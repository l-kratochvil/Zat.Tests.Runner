namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

public record SelectionChangedAction(
    ValueChange<IReadOnlyList<string>>? NewSelectedExecutionPaths = null);