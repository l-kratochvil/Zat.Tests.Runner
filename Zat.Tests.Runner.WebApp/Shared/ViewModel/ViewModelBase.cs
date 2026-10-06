namespace Zat.Tests.Runner.WebApp.Shared.ViewModel;

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
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
    private Func<string, IReadOnlyCollection<string>> chainedTo = static _ => [];

    protected ViewModelBase()
    {
        this.HasErrorsChanged += _ => this.DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private IReadOnlyList<string> validatedPropertyNames = [];

    /// <inheritdoc/>
    public event Action<bool>? HasErrorsChanged;

    /// <inheritdoc/>
    public event EventHandler? DataChanged;

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
    /// <param name="chained">
    /// An action to say which properties are validated again when another one changes, for rules
    /// that apply only while that one holds; <see langword="null"/> where there are none.
    /// </param>
    /// <exception cref="ArgumentException">Thrown if the validated instance is not of the same type as the view model.</exception>
    protected void InitValidator<TValidated>(
        TValidated validated,
        Action<InlineValidator<TValidated>> initialized,
        Action<ValidationChains<TValidated>>? chained = null)
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
        var chains = new ValidationChains<TValidated>();

        chained?.Invoke(chains);

        this.validator = inlineValidator;
        this.chainedTo = chains.ChainedTo;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Validated before the change is announced, so that whoever redraws on it already reads what is
    /// wrong with the new value. The whole view model is validated, because a rule of one property
    /// may hang on another.
    /// </remarks>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        this.ValidateProperty(e.PropertyName);

        if (e.PropertyName is null)
        {
            return;
        }

        // Only what has been validated already, so a value nobody has been to stays unmarked.
        foreach (var propertyName in this.chainedTo(e.PropertyName).Where(this.propertyValidities.ContainsKey))
        {
            this.ValidateProperty(propertyName);
        }
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
        if (newHasErrors != oldHasErrors)
        {
            this.HasErrorsChanged?.Invoke(newHasErrors);
            this.OnPropertyChanged(nameof(this.HasErrors));
        }

        return validity;
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

    /// <summary>
    /// Which properties of a view model are validated again when another one changes, for values whose
    /// rules apply only while that one holds.
    /// </summary>
    /// <typeparam name="TValidated">The type of the view model being validated.</typeparam>
    public sealed class ValidationChains<TValidated>
    {
        private readonly Dictionary<string, HashSet<string>> chains = [];

        /// <summary>
        /// Makes the validity of <paramref name="chained"/> follow <paramref name="rootProperty"/>.
        /// </summary>
        /// <typeparam name="TRootProperty">The type of the root property.</typeparam>
        /// <param name="rootProperty">The property whose change revalidates <paramref name="chained"/> properties.</param>
        /// <param name="chained">The revalidated properties.</param>
        /// <returns>The same chains, so that more can follow.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="rootProperty"/> or any of <paramref name="chained"/> is not a property of
        /// <typeparamref name="TValidated"/>.
        /// </exception>
        public ValidationChains<TValidated> Chain<TRootProperty>(
            Expression<Func<TValidated, TRootProperty>> rootProperty,
            params Expression<Func<TValidated, object?>>[] chained)
        {
            var conditionName = Helpers.GetExpressionPropertyName(rootProperty);

            if (!this.chains.TryGetValue(conditionName, out var known))
            {
                this.chains[conditionName] = known = [];
            }

            known.UnionWith(chained.Select(Helpers.GetExpressionPropertyName));

            return this;
        }

        /// <summary>
        /// Gets the names of the properties chained to <paramref name="conditionName"/>.
        /// </summary>
        /// <param name="conditionName">The name of the condition property.</param>
        /// <returns>The names of the chained properties; empty when nothing is chained to it.</returns>
        internal IReadOnlyCollection<string> ChainedTo(string conditionName)
            => this.chains.TryGetValue(conditionName, out var chained) ? chained : [];
    }
}