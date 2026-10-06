namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

using System.Globalization;
using System.Text.RegularExpressions;

using FluentValidation;

using Fluxor;

using Zat.Tests.Runner.Common.Net.Application.Logging;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The test configuration as the configurator shows it: the values being edited, which of them are
/// asked for at all, and what is wrong with them.
/// </summary>
public sealed partial class TestConfigurationViewModel : ViewModelBase, IDisposable
{
    // How release dates are written down in TestLink, which takes them as they are typed.
    private const string DateFormat = "d.M.yyyy";

    private readonly IState<TestConfigurationState> state;
    private readonly IAppSettingsStore appSettingsStore;
    private readonly ILogger logger;
    private readonly IDispatcher dispatcher;
    private readonly StateBinder<TestConfigurationViewModel, TestConfigurationState> stateBinder;

    public TestConfigurationViewModel(
        IState<TestConfigurationState> state,
        IAppSettingsStore appSettingsStore,
        ILogger<LogSources.App> logger,
        IDispatcher dispatcher)
    {
        this.state = state;
        this.appSettingsStore = appSettingsStore;
        this.logger = logger;
        this.dispatcher = dispatcher;

        this.stateBinder = new StateBinder<TestConfigurationViewModel, TestConfigurationState>(this, this.state);

        this.HasErrorsChanged +=
            hasErrors => this.dispatcher.Dispatch(
                new StatusChangedAction(
                    NewHasErrors: new ValueChange<bool>(hasErrors)));

        this.InitValidator();
    }

    // TODO: Determine based on selected test entities
    public bool IsRuntimeTest { get; private set; }

    public IReadOnlyList<string> RuntimeVersions
        => field ??= this.InitRuntimeVersions();

    public string? RuntimeVersion
    {
        get => this.RuntimeVersionState.Value;
        set => this.RuntimeVersionState.Value = value;
    }

    public Version? IdeVersion
    {
        get => this.IdeVersionState.Value;
        set => this.IdeVersionState.Value = value;
    }

    public HwAssemblyType? TestedHwAssembly
    {
        get => this.TestedHwAssemblyState.Value;
        set => this.TestedHwAssemblyState.Value = value;
    }

    public bool IsTestLinkReportEnabled
    {
        get => this.IsTestLinkReportEnabledState.Value;
        set => this.IsTestLinkReportEnabledState.Value = value;
    }

    public string? IdeReleaseDate
    {
        get => this.IdeReleaseDateState.Value;
        set => this.IdeReleaseDateState.Value = value;
    }

    public string? RuntimeReleaseDate
    {
        get => this.RuntimeReleaseDateState.Value;
        set => this.RuntimeReleaseDateState.Value = value;
    }

    public bool IsBetaVersion
    {
        get => this.IsBetaVersionState.Value;
        set => this.IsBetaVersionState.Value = value;
    }

    public string? BetaVersion
    {
        get => this.BetaVersionState.Value;
        set => this.BetaVersionState.Value = value;
    }

    public bool IsDebugModeEnabled
    {
        get => this.IsDebugModeEnabledState.Value;
        set => this.IsDebugModeEnabledState.Value = value;
    }

    private StateBinding<string?> RuntimeVersionState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.RuntimeVersion,
                property: static vm => vm.RuntimeVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewRuntimeVersion: new ValueChange<string?>(value))));

    private StateBinding<Version?> IdeVersionState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.IdeVersion,
                property: static vm => vm.IdeVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIdeVersion: new ValueChange<Version?>(value))));

    private StateBinding<HwAssemblyType?> TestedHwAssemblyState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.TestedHwAssembly,
                property: static vm => vm.TestedHwAssembly,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewTestedHwAssembly: new ValueChange<HwAssemblyType?>(value))));

    private StateBinding<bool> IsTestLinkReportEnabledState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.IsTestLinkReportEnabled,
                property: static vm => vm.IsTestLinkReportEnabled,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsTestLinkReportEnabled: new ValueChange<bool>(value))));

    private StateBinding<string?> IdeReleaseDateState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.IdeReleaseDate,
                property: static vm => vm.IdeReleaseDate,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIdeReleaseDate: new ValueChange<string?>(value))));

    private StateBinding<string?> RuntimeReleaseDateState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.RuntimeReleaseDate,
                property: static vm => vm.RuntimeReleaseDate,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewRuntimeReleaseDate: new ValueChange<string?>(value))));

    private StateBinding<bool> IsBetaVersionState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.IsBetaVersion,
                property: static vm => vm.IsBetaVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsBetaVersion: new ValueChange<bool>(value))));

    private StateBinding<string?> BetaVersionState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.BetaVersion,
                property: static vm => vm.BetaVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewBetaVersion: new ValueChange<string?>(value))));

    private StateBinding<bool> IsDebugModeEnabledState
        => field ??= this.stateBinder.Bind(
                selectValue: static s => s.IsDebugModeEnabled,
                property: static vm => vm.IsDebugModeEnabled,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsDebugModeEnabled: new ValueChange<bool>(value))));

    /// <inheritdoc/>
    public void Dispose()
        => this.stateBinder.Dispose();

    [GeneratedRegex(@"^\d+")]
    private static partial Regex LeadingNumber();

    private static bool IsDateOrEmpty(string? text)
        => string.IsNullOrEmpty(text)
            || DateTime.TryParseExact(
                text,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);

    // Semantic versioning with the patch left out, which is how the IDE versions are written down.
    [GeneratedRegex(@"^\d+\.\d+(\.\d+)?$")]
    private static partial Regex IdeVersionFormat();

    private void InitValidator()
        => this.InitValidator(
            this,
            validator =>
            {
                validator
                    .RuleFor(static x => x.RuntimeVersion)
                    .NotEmpty()
                    .WithMessage("Runtime version is required.");

                validator
                    .RuleFor(static x => x.IdeVersion)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .WithMessage("IDE version required.")
                    .Must(static x => x is not null && IdeVersionFormat().IsMatch(x.ToString()))
                    .WithMessage("Write the IDE version as x.y or x.y.z, for example 6.1 or 6.1.4.")
                    .When(static x => x.IsTestLinkReportEnabled);

                validator
                    .RuleFor(static x => x.TestedHwAssembly)
                    .NotNull()
                    .NotEqual(HwAssemblyType.Unknown)
                    .WithMessage("Tested hardware assembly is required.")
                    .When(static x => x.IsRuntimeTest);

                validator
                    .RuleFor(static x => x.IdeReleaseDate)
                    .Must(IsDateOrEmpty)
                    .When(static x => x.IsTestLinkReportEnabled)
                    .WithMessage("Write the IDE release date as day.month.year, for example 1.10.2026.");

                validator
                    .RuleFor(static x => x.RuntimeReleaseDate)
                    .Must(IsDateOrEmpty)
                    .When(static x => x.IsTestLinkReportEnabled)
                    .WithMessage("Write the runtime release date as day.month.year, for example 1.10.2026.");

                validator
                    .RuleFor(static x => x.BetaVersion)
                    .Must(static x => string.IsNullOrEmpty(x) || int.TryParse(x, out _))
                    .When(static x => x.IsTestLinkReportEnabled && x.IsBetaVersion)
                    .WithMessage("Write the beta number as a whole number, for example 3.");
            },
            chains => chains
                .Chain(
                    rootProperty: static x => x.IsTestLinkReportEnabled,
                    static x => x.IdeVersion,
                    static x => x.IdeReleaseDate,
                    static x => x.RuntimeReleaseDate,
                    static x => x.BetaVersion)
                .Chain(
                    rootProperty: static x => x.IsBetaVersion,
                    static x => x.BetaVersion)
                .Chain(
                    rootProperty: static x => x.IsRuntimeTest,
                    static x => x.TestedHwAssembly));

    private IReadOnlyList<string> InitRuntimeVersions()
    {
        var installFolderPath = this.appSettingsStore.Current.IdeInstallFolderPath;

        IReadOnlyList<string> folderNames;
        try
        {
            folderNames =
            [
                ..Directory
                    .GetDirectories(installFolderPath)
                    .Select(Path.GetFileName)
                    .OfType<string>()
            ];
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            this.logger.Log(
                LogLevel.Warning,
                "Error while reading the IDE install folder {InstallFolderPath} - no runtime versions available." +
                "\nException: {Exception}",
                installFolderPath,
                exception);

            return [];
        }

        return
        [
            ..folderNames
                .Where(static name => !string.IsNullOrEmpty(name) && LeadingNumber().IsMatch(name))
                .OrderBy(static name => int.TryParse(name, out var parsed) ? parsed : char.MaxValue)
                .ThenBy(static name =>
                    int.TryParse(LeadingNumber().Match(name).Value, out var parsed)
                        ? parsed
                        : char.MaxValue)
                .ThenBy(static name => name, StringComparer.Ordinal)
                .Reverse()
        ];
    }
}