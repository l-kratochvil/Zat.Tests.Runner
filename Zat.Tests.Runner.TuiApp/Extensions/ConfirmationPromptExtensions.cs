namespace Zat.Tests.Runner.TuiApp.Extensions;

internal static class ConfirmationPromptExtensions
{
    extension(ConfirmationPrompt self)
    {
        public ConfirmationPrompt ConfigureDefaultOptions()
            => self
                .No(char.Parse(Resources.NoShorten))
                .Yes(char.Parse(Resources.YesShorten));
    }
}