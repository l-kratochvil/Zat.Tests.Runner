namespace Zat.Tests.Runner.WebApp.Features.TestExecution.Components;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DevKit.Mvvm.ViewModel.Commands;

using Fluxor;

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
    private readonly ITestRunnerEngine testRunnerEngine;
    private readonly IEnumerable<ITestResultHandler> testResultHandlers;
    private readonly IState<TestConfigurationState> configurationState;
    private readonly ITestDiscoveryStore testDiscoveryStore;
    private readonly IAppPathsProvider appPathsProvider;

    public TestExecutionViewModel(
        ITestRunnerEngine testRunnerEngine,
        IEnumerable<ITestResultHandler> testResultHandlers,
        IState<TestConfigurationState> configurationState,
        IAppPathsProvider appPathsProvider,
        ITestDiscoveryStore testDiscoveryStore)
    {
        this.testRunnerEngine = testRunnerEngine;
        this.testResultHandlers = testResultHandlers;
        this.configurationState = configurationState;
        this.testDiscoveryStore = testDiscoveryStore;
        this.appPathsProvider = appPathsProvider;

        // The test run belongs to the environment rather than to this client, so another client may
        // start or stop it at any time.
        this.testRunnerEngine.StateChanged += this.OnTestRunnerEngineStateChanged;
        this.testDiscoveryStore.Changed += this.OnTestSelectionChanged;
        this.configurationState.StateChanged += this.OnConfigurationChanged;

        this.UpdateTestsSelected();
        this.UpdateConfigurationHasErrors();
        this.UpdateTestRunState(this.testRunnerEngine.State);
    }

    /// <summary>
    /// Gets the state of the test run, whoever started it.
    /// </summary>
    [ObservableProperty]
    public partial TestRunState TestRunState { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the test configuration cannot be run with.
    /// </summary>
    /// <remarks>
    /// Read from the state rather than validated here: the configurator owns the rules and puts its
    /// answer into the state.
    /// </remarks>
    [ObservableProperty]
    public partial bool ConfigurationHasErrors { get; private set; }

    /// <summary>
    /// Gets a value indicating whether any test case is selected to run.
    /// </summary>
    [ObservableProperty]
    public partial bool TestsSelected { get; private set; }

    /// <summary>
    /// Gets what can be done with the test run in its current state.
    /// </summary>
    public ExtendedCommand CurrentCommand
        => this.TestRunState switch
        {
            TestRunState.Running => this.StopTestCommand,
            TestRunState.Stopping => this.StoppingTestCommand,
            _ => this.StartTestCommand,
        };

    /// <summary>
    /// Gets why a test run cannot start; empty when nothing stands in its way.
    /// </summary>
    public IReadOnlyList<string> ExecutionBlockingReasons
        => [..this.GetExecutionBlockingReasons()];

    private ExtendedCommand StopTestCommand
        => field ??= new ExtendedCommand(
            command: new RelayCommand(
                execute: this.testRunnerEngine.StopTestRun,
                canExecute: () => this.TestRunState is TestRunState.Running),
            displayText: "Stop");

    private ExtendedCommand StoppingTestCommand
        => field ??= new ExtendedCommand(
            command: new RelayCommand(
                execute: static () => { },
                canExecute: static () => false),
            displayText: "Stopping …");

    private ExtendedCommand StartTestCommand
        => field ??= new ExtendedCommand(
            command: new AsyncRelayCommand(
                execute: this.StartTestRunAsync,
                canExecute: () =>
                    this.TestRunState is TestRunState.Stopped
                    && this.TestsSelected
                    && !this.ConfigurationHasErrors),
            displayText: "Start");

    /// <inheritdoc/>
    public void Dispose()
    {
        this.testRunnerEngine.StateChanged -= this.OnTestRunnerEngineStateChanged;
        this.testDiscoveryStore.Changed -= this.OnTestSelectionChanged;
        this.configurationState.StateChanged -= this.OnConfigurationChanged;
    }

    private async Task StartTestRunAsync()
    {
        throw new NotImplementedException("TODO (see comments)");

        // TODO:
        // Start (using var) RPC server (see NUnitTestRunnerProxy.Program)
        // Use registred mockable service

        // CancellationToken.None: the test run must finish even when this client goes away.
        await this.testRunnerEngine.RunTestAsync(
            testRunEntities: this.testDiscoveryStore.Current.SelectedTestCases,
            config: new ITestRunnerEngine.Config(
                IsDebug: false, // TODO
                TestedRuntimeVersion: this.configurationState.Value.RuntimeVersion,
                TestedHwAssemblyTypes: [], // TODO
                TestResultHandlers: this.testResultHandlers,
                TestAssemblyDllPath: this.appPathsProvider.Files.Test),
            cancellationToken: CancellationToken.None);

        // TODO: Report result:
        // - The report feature should handle testlink reporting and showing it in the UI
        // - Maybe it uses fluxor Effect?
    }

    private void OnTestRunnerEngineStateChanged(TestRunState state)
        => this.UpdateTestRunState(this.testRunnerEngine.State);

    private void OnTestSelectionChanged(TestDiscoveryState state)
        => this.UpdateTestsSelected();

    private void OnConfigurationChanged(object? sender, EventArgs e)
        => this.UpdateConfigurationHasErrors();

    private void UpdateTestRunState(TestRunState state)
        => this.TestRunState = state;

    private void UpdateTestsSelected()
        => this.TestsSelected = this.testDiscoveryStore.Current.SelectedTestCases.Count > 0;

    private void UpdateConfigurationHasErrors()
        => this.ConfigurationHasErrors = this.configurationState.Value.HasErrors;

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