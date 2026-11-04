namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

using CSharpFunctionalExtensions;

using FluentValidation;

using Zat.Tests.Runner.Common.Net.Validation;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// One recipient being added to an email recipient list: whether it is being added at all, the address
/// typed so far, and what is wrong with it.
/// </summary>
/// <remarks>
/// Owned by one editor, so the address being typed is never shared with, nor overwritten by, another.
/// </remarks>
public sealed class EmailAddressListEditorViewModel : ViewModelBase
{
    private readonly Func<IReadOnlyList<string>> readAddresses;

    /// <param name="readAddresses">Reads the recipients currently listed.</param>
    public EmailAddressListEditorViewModel(Func<IReadOnlyList<string>> readAddresses)
    {
        this.readAddresses = readAddresses;

        this.InitValidator(
            this,
            validator => validator
                .RuleFor(static x => x.EmailAddress)
                .Custom((address, context) =>
                {
                    // An empty address has nothing wrong with it to tell yet; it only cannot be confirmed.
                    if (!string.IsNullOrWhiteSpace(address) && this.ValidateNew(address).TryGetError(out var error))
                    {
                        context.AddFailure(error);
                    }
                })
                .When(static x => x.IsEditing));
    }

    /// <summary>
    /// Gets a value indicating whether an address is being added.
    /// </summary>
    public bool IsEditing
    {
        get;
        private set
        {
            if (this.SetProperty(ref field, value))
            {
                this.OnPropertyChanged(nameof(this.CanConfirm));
            }
        }
    }

    /// <summary>
    /// Gets or sets the address typed so far.
    /// </summary>
    public string EmailAddress
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                this.OnPropertyChanged(nameof(this.CanConfirm));
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the typed address can join the recipients.
    /// </summary>
    public bool CanConfirm
        => this.IsEditing && this.ValidateNew(this.EmailAddress).IsSuccess;

    /// <summary>
    /// Starts adding a new address.
    /// </summary>
    public void BeginAdd()
    {
        this.EmailAddress = string.Empty;
        this.IsEditing = true;
    }

    /// <summary>
    /// Abandons the address being added.
    /// </summary>
    public void Cancel()
        => this.IsEditing = false;

    /// <summary>
    /// Finishes adding the typed address.
    /// </summary>
    /// <returns>The trimmed address, or none when it cannot join the recipients.</returns>
    public Maybe<string> Confirm()
    {
        if (!this.CanConfirm)
        {
            return Maybe<string>.None;
        }

        var address = this.EmailAddress.Trim();
        this.IsEditing = false;

        return address;
    }

    private Result ValidateNew(string address)
        => EmailAddressValidators.ValidateNew(address.Trim(), this.readAddresses());
}