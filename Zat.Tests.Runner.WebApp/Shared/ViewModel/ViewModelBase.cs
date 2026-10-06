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

    private IReadOnlyList<string> validatedPropertyNames = [];

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
    /// Initializes the validator for the view model and validates every property it has rules for,
    /// so that what is wrong with the values held from the start is known before anything is edited.
    /// </summary>
    /// <remarks>
    /// <see cref="HasErrorsChanged"/> is raised from here when the values held are wrong, so whoever
    /// hands it on subscribes before calling this.
    /// </remarks>
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

        this.validatedPropertyNames =
        [
            ..inlineValidator.CreateDescriptor().GetMembersWithValidators().Select(x => x.Key)
        ];
        this.validator = inlineValidator;

        this.Validate();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Validated before the change is announced, so that whoever redraws on it already reads what is
    /// wrong with the new value. The whole view model is validated, because a rule of one property
    /// may hang on another.
    /// </remarks>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(this.HasErrors))
        {
            this.Validate();
        }

        base.OnPropertyChanged(e);
    }

    private void Validate()
    {
        if (this.validator is null)
        {
            return;
        }

        var validity = new ValidationContext<object>(this)
            .Pipe(this.validator.Validate)
            .ToValidity();

        var oldHasErrors = this.HasErrors;
        foreach (var propertyName in this.validatedPropertyNames)
        {
            this.propertyValidities[propertyName] = validity.For(propertyName);
        }

        var newHasErrors = this.HasErrors;
        if (newHasErrors == oldHasErrors)
        {
            return;
        }

        this.HasErrorsChanged?.Invoke(newHasErrors);
        this.OnPropertyChanged(nameof(this.HasErrors));
    }
}