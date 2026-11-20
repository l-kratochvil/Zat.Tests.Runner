namespace Zat.Tests.Runner.TuiApp.Common;

using WindowsInput.Native;

internal interface ICommand
{
    VirtualKeyCode Key { get; }

    string Text { get; }
}