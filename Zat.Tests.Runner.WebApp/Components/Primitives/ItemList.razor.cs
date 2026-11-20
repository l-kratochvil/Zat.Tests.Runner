namespace Zat.Tests.Runner.WebApp.Components.Primitives;

using CSharpFunctionalExtensions;

public static class ItemList
{
    /// <summary>
    /// The Add button of an <see cref="ItemList{TItem, TCollection}"/>; giving one asks for the button, which is
    /// why it cannot be given without what supplies the items to add.
    /// </summary>
    /// <typeparam name="TItem">The type of the items added.</typeparam>
    /// <param name="CreateItem">What supplies an item to add, or none when adding is abandoned.</param>
    /// <param name="IsEnabled">A value indicating whether the Add button can be clicked.</param>
    /// <param name="IsVisible">A value indicating whether the Add button is shown.</param>
    public sealed record AddCommandConfig<TItem>(
        Func<Task<Maybe<TItem>>> CreateItem,
        bool IsVisible,
        bool IsEnabled)
    {
        public AddCommandConfig(Func<Task<Maybe<TItem>>> createItem)
            : this(createItem, true, true)
        {
        }
    }


    /// <summary>
    /// The Remove buttons of an <see cref="ItemList{TItem, TCollection}"/>; giving one asks for the buttons.
    /// </summary>
    /// <param name="IsVisible">A value indicating whether the Remove buttons are shown.</param>
    /// <param name="IsEnabled">A value indicating whether the Remove buttons can be clicked.</param>
    public sealed record RemoveCommandConfig(bool IsVisible, bool IsEnabled)
    {
        public RemoveCommandConfig()
            : this(true, true)
        {
        }
    }
}