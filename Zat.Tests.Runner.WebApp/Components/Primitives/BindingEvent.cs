namespace Zat.Tests.Runner.WebApp.Components.Primitives;

using Microsoft.AspNetCore.Components;

/// <summary>
/// When a control hands an edit over to the binding it is written through.
/// </summary>
/// <remarks>
/// Only these two browser events carry an edit as <see cref="ChangeEventArgs"/>, which is the only
/// shape a binding can be written from. Every other event the browser knows reaches the binding as
/// the wrong kind of argument and fails where the tester made the edit, so this is the whole offer.
/// </remarks>
public enum BindingEvent
{
    /// <summary>
    /// Once the tester has finished the edit, which is what a binding means when it is told nothing
    /// else.
    /// </summary>
    OnChange,

    /// <summary>
    /// While the tester is still making the edit, keystroke by keystroke.
    /// </summary>
    OnInput,
}
