namespace Zat.Tests.Runner.TuiApp.Common;

using CSharpFunctionalExtensions;

internal sealed record Choice<TValue>
{
    public Choice(
        TValue value,
        string displayText,
        string? displayValue = null)
    {
        this.Value = value;
        this.Text = displayText.AsTextValuePair(displayValue);
    }

    public string Text { get; }

    public TValue Value { get; }

    /// <summary>
    /// Gets the choices that depend on this one; they are shown one level deeper in the choice tree.
    /// </summary>
    public IReadOnlyList<Choice<TValue>> Children { get; init; } = [];
}

internal static class Choice
{
    public static Maybe<Choice<TChoiceValue>> InitChoice<TChoiceValue>(
        TChoiceValue choiceValue,
        string choiceDisplayText,
        Func<bool>? shouldInitPredicate = null)
        => InitChoice(
            choiceValue,
            choiceDisplayText,
            default(string),
            shouldInitPredicate);

    public static Maybe<Choice<TChoiceValue>> InitChoice<TChoiceValue>(
        TChoiceValue choiceValue,
        string choiceDisplayText,
        bool? choiceDisplayValue,
        Func<bool>? shouldInitPredicate = null)
        => InitChoice(
            choiceValue: choiceValue,
            choiceDisplayText: choiceDisplayText,
            choiceDisplayValue: choiceDisplayValue.HasValue
                ? (choiceDisplayValue.Value ? Resources.Yes : Resources.No)
                : null,
            shouldInitPredicate: shouldInitPredicate);

    public static Maybe<Choice<TChoiceValue>> InitChoice<TChoiceValue>(
        TChoiceValue choiceValue,
        string choiceDisplayText,
        string? choiceDisplayValue,
        Func<bool>? shouldInitPredicate = null)
        => shouldInitPredicate?.Invoke() ?? true
            ? Maybe<Choice<TChoiceValue>>.From(new Choice<TChoiceValue>(
                value: choiceValue,
                displayText: choiceDisplayText,
                displayValue: choiceDisplayValue))
            : Maybe<Choice<TChoiceValue>>.None;
}