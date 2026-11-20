namespace Zat.Tests.Runner.WebApp.Tests;

using AngleSharp.Dom;

/// <summary>
/// The HTML names the tests need that <see cref="TagNames"/>, <see cref="AttributeNames"/> and <see cref="EventNames"/> lack.
/// </summary>
internal static class Html
{
    /// <summary>
    /// Narrows the element down by qualifiers glued right to its name, such as <c>[type=checkbox]</c> or <c>.row</c>.
    /// </summary>
    public static string MakeButtonSelector(params string[] parts)
        => Make(TagNames.Button, parts);

    /// <inheritdoc cref="MakeButtonSelector"/>
    public static string MakeInputSelector(params string[] parts)
        => Make(TagNames.Input, parts);

    /// <inheritdoc cref="MakeButtonSelector"/>
    public static string MakeLabelSelector(params string[] parts)
        => Make(TagNames.Label, parts);

    private static string Make(string element, string[] parts)
        => string.Concat([element, .. parts]);

    /// <summary>
    /// Event handler attribute names, as Blazor binds them.
    /// </summary>
    /// <remarks>
    /// Spelled out, as <see cref="EventNames"/> holds no constants that attribute arguments could build on.
    /// </remarks>
    public static class Events
    {
        public const string OnChange = "onchange";

        public const string OnInput = "oninput";
    }
}