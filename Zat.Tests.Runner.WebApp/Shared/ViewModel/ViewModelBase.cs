namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using System.Collections.Concurrent;
using System.ComponentModel;

using DevKit.Core.Extensions.Functional;

using FluentValidation;

using Zat.Tests.Runner.WebApp.Shared.Validation;

/// <summary>
/// Base class for view models, adding property validation on top of <see cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/>.
/// </summary>
public abstract class ViewModelBase
    : CommunityToolkit.Mvvm.ComponentModel.ObservableObject,
      INotifyValidityInfo, INotifyDataInfo
{
    private readonly ConcurrentDictionary<string, Validity> propertyValidities = new();

    private IValidator? validator;

    /// <inheritdoc/>
    public event Action<bool>? HasErrorsChanged;

    /// <inheritdoc/>
    public event EventHandler? DataChanged;

    protected ViewModelBase()
    {
        this.HasErrorsChanged += _ => this.DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public bool HasErrors
        => this.propertyValidities.Any(x => x.Value.HasErrors);

    /// <inheritdoc/>
    public Validity? GetValidity(string propertyName)
        => this.propertyValidities.GetValueOrDefault(propertyName);

    /// <summary>
    /// Announces a change of <paramref name="propertyName"/> made outside the view model.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    internal void NotifyPropertyChanged(string propertyName)
        => this.OnPropertyChanged(propertyName);

    /// <summary>
    /// Initializes the validator for the view model.
    /// </summary>
    /// <typeparam name="TValidated">The type of the view model being validated.</typeparam>
    /// <param name="validated">The instance of the view model being validated.</param>
    /// <param name="initialized">An action to initialize the inline validator.</param>
    /// <exception cref="ArgumentException">Thrown if the validated instance is not of the same type as the view model.</exception>
    protected void InitValidator<TValidated>(
        TValidated validated, Action<InlineValidator<TValidated>> initialized)
        where TValidated : notnull
    {
        if (validated.GetType() != this.GetType())
        {
            throw new ArgumentException(
                "The validated instance must be of the same type as the view model.",
                nameof(validated));
        }

        var inlineValidator = new InlineValidator<TValidated>();

        initialized(inlineValidator);

        this.validator = inlineValidator;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        this.ValidateProperty(e.PropertyName);
    }

    /// <summary>
    /// Validates the specified property using the view model's validator.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns>The validity result of the property.</returns>
    protected Validity ValidateProperty(string? propertyName)
    {
        if (this.validator is null || propertyName is null)
        {
            return Validity.Valid;
        }

        var validity = ValidationContext<object>
            .CreateWithOptions(this, x => x.IncludeProperties(propertyName))
            .Pipe(this.validator.Validate)
            .ToValidity();

        if (this.propertyValidities.TryGetValue(propertyName, out var looked) && looked == validity)
        {
            return validity;
        }

        var oldHasErrors = this.HasErrors;
        this.propertyValidities[propertyName] = validity;

        var newHasErrors = this.HasErrors;
        if (newHasErrors == oldHasErrors)
        {
            return validity;
        }

        this.HasErrorsChanged?.Invoke(newHasErrors);
        this.OnPropertyChanged(nameof(this.HasErrors));

        return validity;
    }
}