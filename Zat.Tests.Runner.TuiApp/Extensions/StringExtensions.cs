namespace Zat.Tests.Runner.TuiApp.Extensions;

public static class TextFormattors
{
    extension(string text)
    {
        public string AsPromptTitle()
            => $"# {text.EscapeMarkup()}:";

        public string AsErrorText()
            => $"[red]{text.EscapeMarkup()}[/]";

        public string AsTextValuePair(string? value)
            => value is null
                ? $"[italic]{text.EscapeMarkup()}?[/]"
                : $"{text.EscapeMarkup()}: [yellow]{value.EscapeMarkup()}[/]";
    }
}