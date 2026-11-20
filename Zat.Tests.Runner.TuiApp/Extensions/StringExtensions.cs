namespace Zat.Tests.Runner.TuiApp.Extensions;

using System.Text;

public static class StringExtensions
{
    private const int TabSize = 4;

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

        /// <summary>
        /// Turns tabs into spaces and lone carriage returns into line breaks, so the text takes as many cells in the
        /// console as Spectre.Console measures; otherwise lines overflow and live displays redraw out of place.
        /// </summary>
        public string ToConsoleSafeText()
        {
            var sb = new StringBuilder(text.Length);
            var column = 0;

            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\t':
                        var spaceCount = TabSize - (column % TabSize);
                        sb.Append(' ', spaceCount);
                        column += spaceCount;
                        break;
                    case '\r' when i + 1 < text.Length && text[i + 1] == '\n':
                        break;
                    case '\r' or '\n':
                        sb.Append('\n');
                        column = 0;
                        break;
                    default:
                        sb.Append(text[i]);
                        column++;
                        break;
                }
            }

            return sb.ToString();
        }
    }
}