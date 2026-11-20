namespace Zat.Tests.Runner.WebApp.Components.Primitives;

using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.AspNetCore.Components;

using Zat.Tests.Runner.WebApp.Shared.Validation;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// A control bound to one property of the view model cascaded to it.
/// </summary>
/// <remarks>
/// The binding names the property, which is all three things a control needs: what to show, where
/// to write an edit, and what to ask the view model about when showing what is wrong. Naming it
/// once instead of three times is what keeps the three from drifting apart. What is left to the
/// control itself is how the value is shown and how the tester changes it.
/// </remarks>
/// <typeparam name="TViewModel">The view model the control is drawn from.</typeparam>
/// <typeparam name="TBindingValue">The type of the property the control is bound to.</typeparam>
public abstract class BindingComponentBase<TViewModel, TBindingValue>
    : MvvmComponentBase<TViewModel>
    where TViewModel : class, INotifyPropertyChanged, INotifyDataInfo
{
    private Func<TViewModel, TBindingValue> readValue = null!;

    /// <summary>
    /// Gets or sets the property of the view model the control is bound to.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required Expression<Func<TViewModel, TBindingValue>> Binding { get; set; }

    /// <summary>
    /// Gets or sets what is told of an edit once it has been written to the view model.
    /// </summary>
    /// <remarks>
    /// The view model is what the control edits, so this is for whoever has to hear about it as
    /// well — a store the edit is dispatched to — and not for keeping a second copy of the value.
    /// </remarks>
    [Parameter]
    public EventCallback<TBindingValue> OnChange { get; set; }

    /// <summary>
    /// Gets or sets when an edit is written to the view model.
    /// </summary>
    /// <remarks>
    /// Left unset, an edit is heard once the tester has finished making it, which is what a binding
    /// means on its own. A page that wants the view model to keep up with every keystroke — to have
    /// what is wrong with the value said while it is still being typed — asks for it here.
    /// </remarks>
    [Parameter]
    public BindingEvent BindingEvent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets everything else written on the control in the markup.
    /// </summary>
    /// <remarks>
    /// A binding says which value the control is about and nothing about how it looks or is found,
    /// so what the page has to say about that — a class, an identifier, a label to read out — is
    /// passed on to the element the control renders.
    /// </remarks>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the property <see cref="Binding"/> names.
    /// </summary>
    protected PropertyInfo BoundProperty { get; private set; } = null!;

    /// <summary>
    /// Gets the name <see cref="BindingEvent"/> is known by in the browser.
    /// </summary>
    /// <remarks>
    /// What the control renders is told which event to listen for by name, because that is all a
    /// binding takes — which is why the choice is offered as a <see cref="BindingEvent"/> and named
    /// here rather than written out in the markup.
    /// </remarks>
    protected string BindingEventName
        => this.BindingEvent.ToEventName();

    /// <summary>
    /// Gets the value to show.
    /// </summary>
    /// <remarks>
    /// Read from the view model, so that what is shown and what is held cannot drift apart.
    /// </remarks>
    protected TBindingValue CurrentValue
        => this.readValue(this.ViewModel);

    /// <summary>
    /// Gets what is wrong with the value shown.
    /// </summary>
    /// <remarks>
    /// Asked of the view model at every render rather than kept, so that what is shown is what the
    /// view model makes of the value now, including a problem another field's edit brought about —
    /// which is what a view model reporting <see cref="INotifyValidityInfo.HasErrors"/> changed asks
    /// every control to look at anew.
    /// </remarks>
    protected Validity Validity
        => this.ViewModel is INotifyValidityInfo source
            ? source.GetValidity(this.BoundProperty.Name) ?? Validity.Valid
            : Validity.Valid;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        this.BoundProperty = this.ReadBoundProperty();

        // A binding names one property of one view model, so it is compiled once rather than on
        // every edit.
        this.readValue = this.Binding.Compile();

        // Listened to last, because a view model fed by a store says what changed on whatever
        // thread it was changed on, and what is listened for is the property read just above.
        base.OnInitialized();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Only the bound property is drawn here, so a change to any other one is somebody else's to
    /// redraw. Two are heard besides it: a view model that says everything changed at once, because
    /// the property it means is every property, and one that says whether it may be used has
    /// changed, because what is wrong with this property may be another property's doing.
    /// </remarks>
    protected override bool ShouldRerenderOn(string? propertyName)
        => string.IsNullOrEmpty(propertyName)
           || string.Equals(propertyName, this.BoundProperty.Name, StringComparison.Ordinal)
           || string.Equals(
               propertyName,
               nameof(INotifyValidityInfo.HasErrors),
               StringComparison.Ordinal);

    /// <summary>
    /// Writes an edit to the view model and tells <see cref="OnChange"/> of it.
    /// </summary>
    /// <param name="value">
    /// The edited value, which is <see langword="null"/> where the tester left the control holding
    /// nothing and the bound property can hold that.
    /// </param>
    /// <returns>A task that completes once everyone has been told of the edit.</returns>
    protected async Task WriteAsync(TBindingValue? value)
    {
        // Writing through the property runs the view model's setter, which is what tells whoever is
        // listening — this component among them — that the value and its validity have changed.
        this.BoundProperty.SetValue(this.ViewModel, value);

        await this.OnChange.InvokeAsync(value);
    }

    private PropertyInfo ReadBoundProperty()
    {
        // The control writes what the tester did back to the view model it was cascaded, so a
        // binding that names anything else — a property of another object, or one reached through
        // another property — is a mistake in the markup and not something to find out about at the
        // first edit.
        if (this.Binding.Body is not MemberExpression
            {
                Member: PropertyInfo property,
                Expression: ParameterExpression,
            })
        {
            throw new InvalidOperationException(
                $"The binding of a {this.GetType().Name} must name a property of " +
                $"{typeof(TViewModel).Name} directly.");
        }

        if (!property.CanWrite)
        {
            throw new InvalidOperationException(
                $"Property '{typeof(TViewModel).Name}.{property.Name}' cannot be written to.");
        }

        return property;
    }
}