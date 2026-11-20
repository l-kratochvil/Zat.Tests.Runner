namespace Zat.Tests.Runner.TuiApp.Extensions;

using Zat.Tests.Runner.TuiApp.Common;

internal static class SelectionPromptExtensions
{
    /// <summary>
    /// Adds the choices with their children as a tree, keeping every node selectable.
    /// </summary>
    public static SelectionPrompt<Choice<TValue>> AddChoiceTree<TValue>(
        this SelectionPrompt<Choice<TValue>> prompt,
        IEnumerable<Choice<TValue>> choices)
    {
        foreach (var choice in choices)
        {
            AddChildren(prompt.AddChoice(choice), choice.Children);
        }

        return prompt.Mode(SelectionMode.Independent);
    }

    private static void AddChildren<TValue>(ISelectionItem<Choice<TValue>> parent, IEnumerable<Choice<TValue>> children)
    {
        foreach (var child in children)
        {
            AddChildren(parent.AddChild(child), child.Children);
        }
    }
}