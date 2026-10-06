namespace Zat.Tests.Runner.WebApp.Features.TestDiscovery.Components;

using System.Linq;

using Fluxor;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// The test tree as the explorer shows it: the discovered test suites turned into nodes, and the
/// selection made in them.
/// </summary>
/// <remarks>
/// The tree follows the test selection in the state, which is also how the selection the browser
/// remembers arrives, and every change the user makes in the tree is put back into the state. It
/// follows the test tree as well, which changes whenever the test assembly does.
/// </remarks>
public sealed class TestExplorerViewModel : ViewModelBase, IDisposable
{
    private readonly ITestTreeStore testTreeStore;
    private readonly IState<TestDiscoveryState> state;
    private readonly IDispatcher dispatcher;

    public TestExplorerViewModel(
        ITestTreeStore testTreeStore,
        IState<TestDiscoveryState> state,
        IDispatcher dispatcher)
    {
        this.testTreeStore = testTreeStore;
        this.state = state;
        this.dispatcher = dispatcher;

        this.Roots = CreateRoots(testTreeStore.TestSuites);

        this.testTreeStore.Changed += this.OnTestTreeChanged;
        this.state.StateChanged += this.OnStateChanged;

        this.ApplySelection(this.state.Value.SelectedExecutionPaths);
    }

    /// <summary>Gets the nodes standing for the discovered test suites.</summary>
    public IReadOnlyList<TestTreeNodeData> Roots { get; private set; }

    /// <summary>Gets a value indicating whether there is any test to show.</summary>
    public bool IsEmpty
        => this.Roots.Count == 0;

    /// <summary>
    /// Gets the selected test cases, which is what a test run is made of.
    /// </summary>
    public IReadOnlyList<TestCaseEntity> SelectedTestCases
        => [..this.SelectedTestCaseNodes().Select(node => node.Entity).OfType<TestCaseEntity>()];

    /// <summary>
    /// Selects the test cases sitting at the given execution paths and clears every other one.
    /// Paths matching no test case are ignored, so a selection made before the test assemblies
    /// changed restores as much of itself as still exists.
    /// </summary>
    /// <param name="executionPaths">Execution paths of the test cases to select.</param>
    public void ApplySelection(IEnumerable<string> executionPaths)
    {
        var selectedPaths = executionPaths.ToHashSet(StringComparer.Ordinal);

        foreach (var node in this.AllNodes().Where(node => node.IsTestCase))
        {
            node.SetChecked(selectedPaths.Contains(node.ExecutionPath));
        }

        this.ExpandTowardsSelection();
    }

    /// <summary>
    /// Puts what the user selected in the tree into the test selection.
    /// </summary>
    public void OnSelectionChanged()
        => this.dispatcher.Dispatch(
            new SelectionChangedAction(
                NewSelectedExecutionPaths: new ValueChange<IReadOnlyList<string>>(
                    [..this.SelectedExecutionPaths()])));

    /// <inheritdoc/>
    public void Dispose()
    {
        this.testTreeStore.Changed -= this.OnTestTreeChanged;
        this.state.StateChanged -= this.OnStateChanged;
    }

    private static IReadOnlyList<TestTreeNodeData> CreateRoots(IEnumerable<TestSuiteEntity> testSuites)
        => [..testSuites.Select(TestTreeNodeData.Create)];

    private void OnTestTreeChanged(TestTreeChange change)
    {
        this.Roots = CreateRoots(change.TestSuites);
        this.ApplySelection(this.state.Value.SelectedExecutionPaths);

        this.OnPropertyChanged(nameof(this.Roots));
        this.OnPropertyChanged(nameof(this.IsEmpty));
        this.OnPropertyChanged(nameof(this.SelectedTestCases));
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        var selectedPaths = this.state.Value.SelectedExecutionPaths;

        // What the tree itself put into the state comes back here as well. Applying it again would
        // open a closed group the user has just selected, so only a selection made elsewhere is.
        if (!this.SelectedExecutionPaths().ToHashSet(StringComparer.Ordinal).SetEquals(selectedPaths))
        {
            this.ApplySelection(selectedPaths);
        }

        this.OnPropertyChanged(nameof(this.SelectedTestCases));
    }

    private IEnumerable<TestTreeNodeData> AllNodes()
        => this.Roots.SelectMany(root => root.SelfAndDescendants());

    private IEnumerable<TestTreeNodeData> SelectedTestCaseNodes()
        => this.AllNodes().Where(
            node => node.IsTestCase && node.CheckState == TestTreeNodeData.State.Checked);

    private IEnumerable<string> SelectedExecutionPaths()
        => this.SelectedTestCaseNodes().Select(node => node.ExecutionPath);

    private void ExpandTowardsSelection()
    {
        // A restored selection the user cannot see is indistinguishable from none, so every group
        // holding one is opened.
        foreach (var node in this.AllNodes())
        {
            if (node.HasChildren && node.CheckState != TestTreeNodeData.State.Unchecked)
            {
                node.IsExpanded = true;
            }
        }
    }
}