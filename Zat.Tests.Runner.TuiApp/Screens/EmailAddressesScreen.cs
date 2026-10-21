namespace Zat.Tests.Runner.TuiApp.Screens;

using Zat.Tests.Runner.TuiApp.Common;
using Zat.Tests.Runner.TuiApp.Stores;

/// <summary>
/// Manages the email addresses the test run result is sent to: adds one at a time, removes several at once.
/// </summary>
internal class EmailAddressesScreen(
    TestRunConfigStore testRunConfigStore,
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = ct =>
            {
                var prompt = new SelectionPrompt<Choice<IScreen?>>()
                    .Title(string.Empty) // The console is buggy if no title is set
                    .PageSize(10)
                    .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
                    .AddChoices(this.GetChoices())
                    .UseConverter(choice => choice.Text)
                    .HighlightStyle(new Style(foreground: Color.Aqua, decoration: Spectre.Console.Decoration.Bold));

                // Back has no screen of its own: no next screen returns to the previous one.
                return ShowPromptAsync(
                    prompt,
                    choice => new RenderOutput(NextScreen: choice.Value),
                    ct);
            },
            Info = () =>
            {
                Write(new Rule(Resources.EmailAddresses_ChoiceText.EscapeMarkup()).LeftJustified());
                WriteLine();

                foreach (var emailAddress in testRunConfigStore.EmailAddresses)
                {
                    MarkupLine($"  [yellow]{emailAddress.EscapeMarkup()}[/]");
                }

                WriteLine();
                Write(new Rule());
            },
        };

    private IEnumerable<Choice<IScreen?>> GetChoices()
    {
        yield return new Choice<IScreen?>(
            new AddEmailAddressScreen(
                testRunConfigStore,
                this.HomeScreenLazy,
                this.ExitScreenLazy,
                this.SettingsScreenLazy),
            Resources.AddEmailAddress_ChoiceText);

        if (testRunConfigStore.EmailAddresses.Length > 0)
        {
            yield return new Choice<IScreen?>(
                new RemoveEmailAddressesScreen(
                    testRunConfigStore,
                    this.HomeScreenLazy,
                    this.ExitScreenLazy,
                    this.SettingsScreenLazy),
                Resources.RemoveEmailAddresses_ChoiceText);
        }
    }

    private class AddEmailAddressScreen(
        TestRunConfigStore testRunConfigStore,
        Lazy<HomeScreen> homeScreen,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
        : ScreenBase(homeScreen, exitScreen, settingsScreen)
    {
        /// <inheritdoc/>
        protected override ScreenRenderer CreateRenderer()
            => new()
            {
                Main = ct => ShowPromptAsync(
                    new TextPrompt<string>(Resources.EnterEmailAddress_PromptText)
                        .Validate(address => Validators.IsNewZatEmailAddress(
                            address.Trim(),
                            testRunConfigStore.EmailAddresses)),
                    address =>
                    {
                        testRunConfigStore.EmailAddresses = [.. testRunConfigStore.EmailAddresses, address.Trim()];
                        return RenderOutput.Default;
                    },
                    ct),
            };
    }

    private class RemoveEmailAddressesScreen(
        TestRunConfigStore testRunConfigStore,
        Lazy<HomeScreen> homeScreen,
        Lazy<ExitScreen> exitScreen,
        Lazy<SettingsScreen> settingsScreen)
        : ScreenBase(homeScreen, exitScreen, settingsScreen)
    {
        /// <inheritdoc/>
        protected override ScreenRenderer CreateRenderer()
            => new()
            {
                Main = ct => ShowPromptAsync(
                    () => new MultiSelectionPrompt<string>()
                        .Title(Resources.SelectEmailAddressesToRemove_PromptText.AsPromptTitle())
                        .MoreChoicesText(SharedTexts.MoreChoicesHelpText)
                        .PageSize(10)
                        .InstructionsText(SharedTexts.InstructionsHelpText)
                        .NotRequired()
                        .AddChoices(testRunConfigStore.EmailAddresses),
                    removedAddresses =>
                    {
                        testRunConfigStore.EmailAddresses =
                            [.. testRunConfigStore.EmailAddresses.Except(removedAddresses)];
                        return RenderOutput.Default;
                    },
                    ct),
            };
    }
}