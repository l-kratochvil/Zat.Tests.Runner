namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

using System.Globalization;
using System.Text.RegularExpressions;

using DevKit.Core.Extensions.Types;

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

    private readonly IAppSettingsStore appSettingsStore;
    private readonly ILogger logger;
    private readonly IDispatcher dispatcher;
    private readonly IDisposable[] disposables;

    private readonly StateBinding<string?> runtimeVersion;
    private readonly StateBinding<Version?> ideVersion;
    private readonly StateBinding<HwAssemblyType?> testedHwAssembly;
    private readonly StateBinding<bool> isTestLinkReportEnabled;
    private readonly StateBinding<string?> ideReleaseDate;
    private readonly StateBinding<string?> runtimeReleaseDate;
    private readonly StateBinding<bool> isBetaVersion;
    private readonly StateBinding<string?> betaVersion;
    private readonly StateBinding<bool> isDebugModeEnabled;

    public TestConfigurationViewModel(
        IState<TestConfigurationState> state,
        IAppSettingsStore appSettingsStore,
        ILogger<LogSources.App> logger,
        IDispatcher dispatcher)
    {
        this.appSettingsStore = appSettingsStore;
        this.logger = logger;
        this.dispatcher = dispatcher;

        this.disposables =
        [
            this.runtimeVersion = this.BindToState(
                state,
                selectValue: static s => s.RuntimeVersion,
                property: static vm => vm.RuntimeVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewRuntimeVersion: new ValueChange<string?>(value)))),
            this.ideVersion = this.BindToState(
                state,
                selectValue: static s => s.IdeVersion,
                property: static vm => vm.IdeVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIdeVersion: new ValueChange<Version?>(value)))),
            this.testedHwAssembly = this.BindToState(
                state,
                selectValue: static s => s.TestedHwAssembly,
                property: static vm => vm.TestedHwAssembly,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewTestedHwAssembly: new ValueChange<HwAssemblyType?>(value)))),
            this.isTestLinkReportEnabled = this.BindToState(
                state,
                selectValue: static s => s.IsTestLinkReportEnabled,
                property: static vm => vm.IsTestLinkReportEnabled,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsTestLinkReportEnabled: new ValueChange<bool>(value)))),
            this.ideReleaseDate = this.BindToState(
                state,
                selectValue: static s => s.IdeReleaseDate,
                property: static vm => vm.IdeReleaseDate,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIdeReleaseDate: new ValueChange<string?>(value)))),
            this.runtimeReleaseDate = this.BindToState(
                state,
                selectValue: static s => s.RuntimeReleaseDate,
                property: static vm => vm.RuntimeReleaseDate,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewRuntimeReleaseDate: new ValueChange<string?>(value)))),
            this.isBetaVersion = this.BindToState(
                state,
                selectValue: static s => s.IsBetaVersion,
                property: static vm => vm.IsBetaVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsBetaVersion: new ValueChange<bool>(value)))),
            this.betaVersion = this.BindToState(
                state,
                selectValue: static s => s.BetaVersion,
                property: static vm => vm.BetaVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewBetaVersion: new ValueChange<string?>(value)))),
            this.isDebugModeEnabled = this.BindToState(
                state,
                selectValue: static s => s.IsDebugModeEnabled,
                property: static vm => vm.IsDebugModeEnabled,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsDebugModeEnabled: new ValueChange<bool>(value)))),
        ];

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
        get => this.runtimeVersion.Value;
        set => this.runtimeVersion.Value = value;
    }

    public Version? IdeVersion
    {
        get => this.ideVersion.Value;
        set => this.ideVersion.Value = value;
    }

    public HwAssemblyType? TestedHwAssembly
    {
        get => this.testedHwAssembly.Value;
        set => this.testedHwAssembly.Value = value;
    }

    public bool IsTestLinkReportEnabled
    {
        get => this.isTestLinkReportEnabled.Value;
        set => this.isTestLinkReportEnabled.Value = value;
    }

    public string? IdeReleaseDate
    {
        get => this.ideReleaseDate.Value;
        set => this.ideReleaseDate.Value = value;
    }

    public string? RuntimeReleaseDate
    {
        get => this.runtimeReleaseDate.Value;
        set => this.runtimeReleaseDate.Value = value;
    }

    public bool IsBetaVersion
    {
        get => this.isBetaVersion.Value;
        set => this.isBetaVersion.Value = value;
    }

    public string? BetaVersion
    {
        get => this.betaVersion.Value;
        set => this.betaVersion.Value = value;
    }

    public bool IsDebugModeEnabled
    {
        get => this.isDebugModeEnabled.Value;
        set => this.isDebugModeEnabled.Value = value;
    }

    /// <inheritdoc/>
    public void Dispose()
        => this.disposables.DisposeAll();

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