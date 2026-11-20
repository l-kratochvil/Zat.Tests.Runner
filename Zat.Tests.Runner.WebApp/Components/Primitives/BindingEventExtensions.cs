namespace Zat.Tests.Runner.WebApp.Components.Primitives;

/// <summary>
/// Spells a <see cref="BindingEvent"/> out the way the browser names it.
/// </summary>
public static class BindingEventExtensions
{
    /// <summary>
    /// Reads the browser's name for <paramref name="bindingEvent"/>.
    /// </summary>
    /// <remarks>
    /// The name is all a binding is given to go on and nothing checks it — a misspelled one is
    /// simply never heard from — so it is spelled out in one place rather than at every control.
    /// </remarks>
    /// <param name="bindingEvent">The event to name.</param>
    /// <returns>The name <paramref name="bindingEvent"/> is known by in the browser.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bindingEvent"/> is not one of the offered events.
    /// </exception>
    public static string ToEventName(this BindingEvent bindingEvent)
        => bindingEvent switch
        {
            BindingEvent.OnChange => "onchange",
            BindingEvent.OnInput => "oninput",
            _ => throw new ArgumentOutOfRangeException(nameof(bindingEvent), bindingEvent, null),
        };
}
