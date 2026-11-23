namespace Zat.Tests.Runner.TuiApp.Extensions;

internal static class MultiSelectionPromptExtensions
{
    extension<T>(MultiSelectionPrompt<T> self)
        where T : notnull
    {
        /// <summary>
        /// Applies the app-wide highlight style, which also colors the selection checkbox;
        /// its built-in grey for groups may blend with the background.
        /// </summary>
        public MultiSelectionPrompt<T> ApplyDefaultStyle()
            => self.HighlightStyle(new Style(
                foreground: Color.Aqua,
                decoration: Spectre.Console.Decoration.Bold));
    }
}