namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

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
    private readonly IAppSettingsStore appSettingsStore;
    private readonly ILogger logger;
    private readonly IDispatcher dispatcher;
    private readonly IDisposable[] disposables;

    private readonly StateBinding<string?> runtimeVersion;
    private readonly StateBinding<Version?> ideVersion;
    private readonly StateBinding<HwAssemblyType?> testedHwAssembly;
    private readonly StateBinding<bool> isTestLinkReportEnabled;

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
                selectValue: s => s.RuntimeVersion,
                property: vm => vm.RuntimeVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewRuntimeVersion: new ValueChange<string?>(value)))),
            this.ideVersion = this.BindToState(
                state,
                selectValue: s => s.IdeVersion,
                property: vm => vm.IdeVersion,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIdeVersion: new ValueChange<Version?>(value)))),
            this.testedHwAssembly = this.BindToState(
                state,
                selectValue: s => s.TestedHwAssembly,
                property: vm => vm.TestedHwAssembly,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewTestedHwAssembly: new ValueChange<HwAssemblyType?>(value)))),
            this.isTestLinkReportEnabled = this.BindToState(
                state,
                selectValue: s => s.IsTestLinkReportEnabled,
                property: vm => vm.IsTestLinkReportEnabled,
                writeValue: value => this.dispatcher.Dispatch(
                    new DataChangedAction(NewIsTestLinkReportEnabled: new ValueChange<bool>(value)))),
        ];

        this.HasErrorsChanged +=
            hasErrors => this.dispatcher.Dispatch(
                new StatusChangedAction(
                    NewHasErrors: new ValueChange<bool>(hasErrors)));

        this.InitValidator(this, validator =>
        {
            validator
                .RuleFor(x => x.RuntimeVersion)
                .NotEmpty()
                .WithMessage("Runtime version is required.");

            validator
                .RuleFor(x => x.IdeVersion)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("IDE version required.")
                .Must(x => x is not null && IdeVersionFormat().IsMatch(x.ToString()))
                .WithMessage("Write the IDE version as x.y or x.y.z, for example 6.1 or 6.1.4.");

            validator
                .RuleFor(x => x.TestedHwAssembly)
                .NotNull()
                .NotEqual(HwAssemblyType.Unknown)
                .WithMessage("Tested hardware assembly is required.");
        });
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

    /// <inheritdoc/>
    public void Dispose()
        => this.disposables.DisposeAll();

    [GeneratedRegex(@"^\d+")]
    private static partial Regex LeadingNumber();

    // Semantic versioning with the patch left out, which is how the IDE versions are written down.
    [GeneratedRegex(@"^\d+\.\d+(\.\d+)?$")]
    private static partial Regex IdeVersionFormat();

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