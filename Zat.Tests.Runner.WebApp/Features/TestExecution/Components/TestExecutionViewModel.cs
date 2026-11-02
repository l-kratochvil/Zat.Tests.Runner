namespace Zat.Tests.Runner.WebApp.Features.TestExecution.Components;

using CommunityToolkit.Mvvm.Input;

using DevKit.Mvvm.ViewModel.Commands;

using Fluxor;

using Zat.Tests.Runner.Common.Model;
using Zat.Tests.Runner.Common.Net.Model;
using Zat.Tests.Runner.Common.Net.Services;
using Zat.Tests.Runner.WebApp.Application.Paths;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// The test run as the tester controls it: what can be done with it now, and what keeps a new one
/// from starting.
/// </summary>
public sealed partial class TestExecutionViewModel : ViewModelBase, IDisposable
{
    internal const string StartLabel = "Start";
    internal const string StopLabel = "Stop";
    internal const string StoppingLabel = "Stopping …";

    private readonly ITestRunnerEngine testRunnerEngine;
    private readonly IEnumerable<ITestResultHandler> testResultHandlers;
    private readonly IState<TestConfigurationState> configurationState;
    private readonly IState<TestDiscoveryState> testSelectionState;
    private readonly ITestTreeStore testTreeStore;
    private readonly IAppPathsProvider appPathsProvider;

    public TestExecutionViewModel(
        ITestRunnerEngine testRunnerEngine,
        IEnumerable<ITestResultHandler> testResultHandlers,
        IState<TestConfigurationState> configurationState,
        IAppPathsProvider appPathsProvider,
        IState<TestDiscoveryState> testSelectionState,
        ITestTreeStore testTreeStore)
    {
        this.testRunnerEngine = testRunnerEngine;
        this.testResultHandlers = testResultHandlers;
        this.configurationState = configurationState;
        this.testSelectionState = testSelectionState;
        this.testTreeStore = testTreeStore;
        this.appPathsProvider = appPathsProvider;

        this.testRunnerEngine.StateChanged += this.OnTestRunnerEngineStateChanged;
        this.testSelectionState.StateChanged += this.OnTestSelectionChanged;
        this.configurationState.StateChanged += this.OnConfigurationChanged;
    }

    /// <summary>
    /// Gets the state of the test run, whoever started it.
    /// </summary>
    public TestRunState TestRunState
        => this.testRunnerEngine.State;

    /// <summary>
    /// Gets a value indicating whether the test configuration cannot be run with.
    /// </summary>
    /// <remarks>
    /// Read from the state rather than validated here: the configurator owns the rules and puts its
    /// answer into the state.
    /// </remarks>
    public bool ConfigurationHasErrors
        => this.configurationState.Value.HasErrors;

    /// <summary>
    /// Gets a value indicating whether any test case is selected to run.
    /// </summary>
    public bool TestsSelected
        => this.SelectedTestCases().Count > 0;

    /// <summary>
    /// Gets what can be done with the test run in its current state.
    /// </summary>
    public ExtendedCommand CurrentCommand
        => this.testRunnerEngine.State switch
        {
            TestRunState.Running => this.StopTestCommand,
            TestRunState.Stopping => this.StoppingTestCommand,
            _ => this.StartTestCommand,
        };

    /// <summary>
    /// Gets why a test run cannot start; empty when nothing stands in its way.
    /// </summary>
    public IReadOnlyList<string> ExecutionBlockingReasons
        => [.. this.GetExecutionBlockingReasons()];

    private RelayCommand StopTestRelayCommand
        => field ??= new RelayCommand(
            execute: this.testRunnerEngine.StopTestRun,
            canExecute: () => this.testRunnerEngine.State is TestRunState.Running);

    private ExtendedCommand StopTestCommand
        => field ??= new ExtendedCommand(
            command: this.StopTestRelayCommand,
            displayText: StopLabel);

    private ExtendedCommand StoppingTestCommand
        => field ??= new ExtendedCommand(
            command: new RelayCommand(
                execute: static () => { },
                canExecute: static () => false),
            displayText: StoppingLabel);

    private AsyncRelayCommand StartTestRelayCommand
        => field ??= new AsyncRelayCommand(
            execute: this.StartTestRunAsync,
            canExecute: () =>
                this.testRunnerEngine.State is TestRunState.Stopped
                && this.TestsSelected
                && !this.ConfigurationHasErrors);

    private ExtendedCommand StartTestCommand
        => field ??= new ExtendedCommand(
            command: this.StartTestRelayCommand,
            displayText: StartLabel);

    /// <inheritdoc/>
    public void Dispose()
    {
        this.testRunnerEngine.StateChanged -= this.OnTestRunnerEngineStateChanged;
        this.testSelectionState.StateChanged -= this.OnTestSelectionChanged;
        this.configurationState.StateChanged -= this.OnConfigurationChanged;
    }

    private async Task StartTestRunAsync()
    {

        await this.testRunnerEngine.RunTestAsync(
            testRunEntities: this.SelectedTestCases(),
            config: new ITestRunnerEngine.Config(
                IsDebug: this.configurationState.Value.IsDebugModeEnabled,
                TestedRuntimeVersion: this.configurationState.Value.RuntimeVersion,
                TestedHwAssemblyTypes: [this.configurationState.Value.TestedHwAssembly ?? Z2xxTests.Common.Model.HwAssemblyType.Unknown], 
                TestResultHandlers: this.testResultHandlers,
                TestAssemblyDllPath: this.appPathsProvider.Files.MainAssemblyDll),
            cancellationToken: CancellationToken.None);

        // TODO: Report result:
        // - The report feature should handle testlink reporting and showing it in the UI
        // - Maybe it uses fluxor Effect?
    }

    private void OnTestRunnerEngineStateChanged(TestRunState state)
    {
        this.OnPropertyChanged(nameof(this.TestRunState));
        this.OnPropertyChanged(nameof(this.CurrentCommand));
        this.StartTestRelayCommand.NotifyCanExecuteChanged();
        this.StopTestRelayCommand.NotifyCanExecuteChanged();
    }

    private void OnTestSelectionChanged(object? sender, EventArgs e)
    {
        this.OnPropertyChanged(nameof(this.TestsSelected));
        this.OnPropertyChanged(nameof(this.ExecutionBlockingReasons));
        this.StartTestRelayCommand.NotifyCanExecuteChanged();
    }

    private void OnConfigurationChanged(object? sender, EventArgs e)
    {
        this.OnPropertyChanged(nameof(this.ConfigurationHasErrors));
        this.OnPropertyChanged(nameof(this.ExecutionBlockingReasons));
        this.StartTestRelayCommand.NotifyCanExecuteChanged();
    }

    private IReadOnlyList<TestCaseEntity> SelectedTestCases()
        => this.testSelectionState.Value.SelectedTestCases(this.testTreeStore.TestSuites);

    private IEnumerable<string> GetExecutionBlockingReasons()
    {
        if (!this.TestsSelected)
        {
            yield return "No tests selected";
        }

        if (this.ConfigurationHasErrors)
        {
            yield return "Configuration has errors";
        }
    }
}