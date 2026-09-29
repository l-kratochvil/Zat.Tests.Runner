namespace Zat.Tests.Runner.Common.Net.Utils;

using System.Collections.Immutable;

public static class LocalizationUtils
{
    private static readonly ImmutableDictionary<string, string> LocalizedTextsMap =
        new Dictionary<string, string>
        {
            { "PdpClientTests", Resources.PdpClientTests },
            { "Pertinax6Tests", Resources.Pertinax6Tests },
            { "RuntimeTests", Resources.RuntimeTests },
        }.ToImmutableDictionary();

    /// <summary>
    /// Maps the given text to its localized equivalent if available; otherwise, returns the original text.
    /// </summary>
    /// <param name="text">The text to be localized.</param>
    /// <returns>The localized equivalent if available; otherwise, the original text.</returns>
    public static string MapTextToLocalized(string text)
        => LocalizedTextsMap.TryGetValue(text, out var localized) ? localized : text;
}