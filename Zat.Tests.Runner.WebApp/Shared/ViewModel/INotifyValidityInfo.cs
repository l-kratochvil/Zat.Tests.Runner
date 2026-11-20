namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using Zat.Tests.Runner.WebApp.Shared.Validation;

/// <summary>
/// A view model that says what is wrong with the values it holds.
/// </summary>
/// <remarks>
/// A control bound to one property asks about that property alone, so the view model answers per
/// property rather than handing out everything it found.
/// </remarks>
public interface INotifyValidityInfo
{
    event Action<bool>? HasErrorsChanged;

    /// <summary>
    /// Gets a value indicating whether every value held may be used.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Reads what is wrong with one property.
    /// </summary>
    /// <param name="propertyName">Property to ask about.</param>
    /// <returns>
    /// The <see cref="Validity"/> of <paramref name="propertyName"/>, or <see langword="null"/>
    /// while it has not been looked at.
    /// </returns>
    Validity? GetValidity(string propertyName);
}