namespace Zat.Tests.Runner.WebApp.Tests.Components.Primitives;

using System.ComponentModel;
using System.Runtime.CompilerServices;

using Zat.Tests.Runner.WebApp.Shared.Validation;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// The view model the binding controls are drawn from and edited into in these fixtures.
/// </summary>
/// <remarks>
/// Written by hand rather than taken from a feature, so that what a control does is read off the
/// control and not off whatever a real view model makes of the edit on its way in.
/// </remarks>
internal sealed class EditedViewModel
    : INotifyPropertyChanged,
      INotifyValidityInfo,
      INotifyDataInfo
{
    private readonly Dictionary<string, Validity> validities = [];

    private string? text;
    private int number;
    private bool isEnabled;
    private Shade? shade;
    private Shade requiredShade;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event Action<bool>? HasErrorsChanged;

    /// <inheritdoc/>
    public event EventHandler? DataChanged;

    /// <summary>
    /// A value a control cannot spell out as text, so that an option has to be named by its place.
    /// </summary>
    public enum Shade
    {
        Red,
        Green,
    }

    /// <inheritdoc/>
    public bool HasErrors
        => this.validities.Values.Any(validity => validity.Issues.Count is not 0);

    public string? Text
    {
        get => this.text;
        set => this.Write(ref this.text, value);
    }

    public int Number
    {
        get => this.number;
        set => this.Write(ref this.number, value);
    }

    public bool IsEnabled
    {
        get => this.isEnabled;
        set => this.Write(ref this.isEnabled, value);
    }

    public Shade? Shading
    {
        get => this.shade;
        set => this.Write(ref this.shade, value);
    }

    public Shade RequiredShading
    {
        get => this.requiredShade;
        set => this.Write(ref this.requiredShade, value);
    }

    /// <summary>
    /// Gets a value no control may be bound to, because there is nowhere to write the edit.
    /// </summary>
    public string Fixed
        => "fixed";

    /// <inheritdoc/>
    public Validity? GetValidity(string propertyName)
        => this.validities.GetValueOrDefault(propertyName);

    /// <summary>
    /// Says what is wrong with one property from now on.
    /// </summary>
    /// <param name="propertyName">Property the issues belong to.</param>
    /// <param name="validity">What is wrong with it.</param>
    public void SaySomethingIsWrongWith(string propertyName, Validity validity)
    {
        this.validities[propertyName] = validity;

        this.HasErrorsChanged?.Invoke(this.HasErrors);
        this.PropertyChanged?.Invoke(
            this, new PropertyChangedEventArgs(nameof(INotifyValidityInfo.HasErrors)));
    }

    private void Write<TValue>(
        ref TValue field, TValue value, [CallerMemberName] string? propertyName = null)
    {
        field = value;
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}