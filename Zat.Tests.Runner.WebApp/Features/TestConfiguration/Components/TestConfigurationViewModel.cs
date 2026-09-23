namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

using System.Text.RegularExpressions;

using DevKit.Core.Interfaces;

using FluentValidation;

using Fluxor;

using Zat.Tests.Runner.WebApp.Shared.Logging;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;
using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The test configuration as the configurator shows it: the values being edited, which of them are
/// asked for at all, and what is wrong with them.
/// </summary>
/// <remarks>
/// Holds what the tester is typing, which the configuration itself must not: an IDE version is only
/// written into the state once it has been found to be one, so the half-typed text has to live
/// somewhere that is not the state. Knows nothing about Fluxor or the browser, so the rules of
/// editing can be exercised on their own.
/// </remarks>
public partial class TestConfigurationViewModel : ViewModelBase, IInitializable
{
    private readonly IState<TestConfigurationState> state;
    private readonly IAppSettingsStore appSettingsStore;
    private readonly IAppLogger logger;
    private readonly IDispatcher dispatcher;

    public TestConfigurationViewModel(
        IState<TestConfigurationState> state,
        IAppSettingsStore appSettingsStore,
        IAppLoggerFactory loggerFactory,
        IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
        this.state = state;
        this.appSettingsStore = appSettingsStore;

        this.logger = loggerFactory.CreateLogger(LogSources.App);

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

    public string RuntimeVersion
    {
        get;
        set => this.SetProperty(
            field,
            value,
            value => this.dispatcher.Dispatch(
                new DataChangedAction
                {
                    NewRuntimeVersion = new ValueChange<string?>(value),
                }));
    } = string.Empty;

    public Version? IdeVersion
    {
        get;
        set => this.SetProperty(
            field,
            value,
            value => this.dispatcher.Dispatch(
                new DataChangedAction
                {
                    NewIdeVersion = new ValueChange<Version?>(value),
                }));
    }

    public HwAssemblyType? TestedHwAssembly
    {
        get;
        set => this.SetProperty(
            field,
            value,
            value => this.dispatcher.Dispatch(
                new DataChangedAction
                {
                    NewTestedHwAssembly = new ValueChange<HwAssemblyType?>(value),
                }));
    }

    public bool IsTestLinkReportEnabled
    {
        get;
        set => this.SetProperty(
            field,
            value,
            value => this.dispatcher.Dispatch(
                new DataChangedAction
                {
                    NewIsTestLinkReportEnabled = new ValueChange<bool>(value),
                }));
    } = false;

    /// <inheritdoc/>
    public void Initialize()
    {
        this.IdeVersion = this.state.Value.IdeVersion;
        this.RuntimeVersion = this.state.Value.RuntimeVersion ?? string.Empty;
        this.TestedHwAssembly = this.state.Value.TestedHwAssembly;
        this.IsTestLinkReportEnabled = this.state.Value.IsTestLinkReportEnabled;
    }

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
            this.logger.Warning(
                "The IDE install folder could not be read, so there are no runtime versions to " +
                $"choose from ({installFolderPath}).",
                exception.ToString());

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