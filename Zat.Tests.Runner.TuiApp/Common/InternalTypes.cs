namespace Zat.Tests.Runner.TuiApp.Common;

using WindowsInput.Native;

using Zat.Tests.Runner.TuiApp.Screens;

/// <summary>
/// Contains internal types for a plain data objects (with no behaviour).
/// </summary>
internal static class InternalTypes
{
    /// <summary>
    /// Provides render output.<br/>
    /// If ExitScreen is true, then the render is returned to the previous screen (the one that called the render of the screen).<br/>
    /// If ExitApp is true, then the application will exit.<br/>
    /// If NextScreen is provided then it is rendered after the render of the current screen is complete.<br/>
    /// </summary>
    public record RenderOutput(
        IScreen? NextScreen = null,
        bool Exit = false)
    {
        public static RenderOutput Default
            => new();
    }

    public record InterruptionCommand(
        VirtualKeyCode Key,
        string Text,
        IScreen? NextScreen)
        : CommandBase(Key, Text);

    public record ActionCommand(
        VirtualKeyCode Key,
        string Text,
        Action Action)
        : CommandBase(Key, Text);

    public record CommandBase(
        VirtualKeyCode Key,
        string Text)
        : ICommand;

    public record CompletedShowPrompt(RenderOutput RenderOutput)
        : ShowPromptResult
    {
        public static CompletedShowPrompt Default
            => new(RenderOutput.Default);
    }

    public record InterruptedShowPrompt : ShowPromptResult;

    public record ShowPromptResult;

    public record TextValueItem<TValue>(
        string Text,
        TValue Value);
}

/// <summary>
/// Provides behavior of internal types for a plain data objects.
/// </summary>
internal static class InternalTypesBehavior;