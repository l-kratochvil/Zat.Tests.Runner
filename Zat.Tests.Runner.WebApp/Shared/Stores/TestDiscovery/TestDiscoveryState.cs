namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

using Fluxor;

/// <summary>
/// The current test selection.
/// </summary>
/// <remarks>
/// Belongs to one browser and is remembered there. Kept as execution paths rather than test cases,
/// so a selection remembered before the test assemblies changed still reads back.
/// </remarks>
/// <param name="SelectedExecutionPaths">
/// Execution paths of the selected test cases. Suites and fixtures are derived from these, so the
/// test selection has one source of truth.
/// </param>
[FeatureState]
public sealed record TestDiscoveryState(IReadOnlyList<string> SelectedExecutionPaths)
{
    public TestDiscoveryState()
        : this(SelectedExecutionPaths: [])
    {
    }
}