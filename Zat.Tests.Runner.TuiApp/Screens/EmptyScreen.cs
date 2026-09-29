namespace Zat.Tests.Runner.TuiApp.Screens;

internal class EmptyScreen(
    Lazy<HomeScreen> homeScreen,
    Lazy<ExitScreen> exitScreen,
    Lazy<SettingsScreen> settingsScreen)
    : ScreenBase(homeScreen, exitScreen, settingsScreen)
{
    /// <inheritdoc/>
    protected override ScreenRenderer CreateRenderer()
        => new()
        {
            Main = _ =>
            {
                var table = new Table()
                    .AddColumn(new TableColumn("*** EMPTY SCREEN ***"))
                    .LeftAligned()
                    .AddRow("*** (PRESS ANY KEY TO RETURN TO THE HOME SCREEN) ***");
                table.Columns[0].Alignment = Justify.Center;

                Write(table);

                AnsiConsole.Console.Input.ReadKey(true);

                return new CompletedShowPrompt(RenderOutput: RenderOutput.Default).AsTask();
            },
        };
}