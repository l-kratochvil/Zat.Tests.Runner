namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

using CSharpFunctionalExtensions;

using FluentValidation;

using Zat.Tests.Runner.Common.Net.Validation;
using Zat.Tests.Runner.WebApp.Shared.ViewModel;

/// <summary>
/// An email recipient list with what is wrong with it, and the one recipient being added to it: whether
/// it is being added at all, the address typed so far, and what is wrong with it.
/// </summary>
/// <remarks>
/// Owned by one editor, so the address being typed is never shared with, nor overwritten by, another.
/// </remarks>
public sealed class EmailAddressesEditorViewModel : ViewModelBase
{
    private TaskCompletionSource<Maybe<string>>? pendingAdd;

    public EmailAddressesEditorViewModel()
    {
        this.EmailAddresses = [];

        this.InitValidator(
            this,
            validator =>
            {
                validator
                    .RuleFor(static x => x.EmailAddresses)
                    .Custom(static (addresses, context) =>
                    {
                        foreach (var address in addresses)
                        {
                            if (EmailAddressValidators.Validate(address).TryGetError(out var error))
                            {
                                context.AddFailure($"{address}: {error}");
                            }
                        }
                    });

                validator
                    .RuleFor(static x => x.EmailAddress)
                    .Custom((address, context) =>
                    {
                        // An empty address has nothing wrong with it to tell yet; it only cannot be confirmed.
                        if (!string.IsNullOrWhiteSpace(address) && this.ValidateNew(address).TryGetError(out var error))
                        {
                            context.AddFailure(error);
                        }
                    })
                    .When(static x => x.IsEditing);
            });
    }

    /// <summary>
    /// Gets or sets the recipients listed.
    /// </summary>
    public IReadOnlyList<string> EmailAddresses
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                this.OnPropertyChanged(nameof(this.CanConfirm));
            }
        }
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
    /// <returns>
    /// The trimmed address once <see cref="Confirm"/> finishes the adding, or none once <see cref="Cancel"/>
    /// abandons it.
    /// </returns>
    public Task<Maybe<string>> BeginAdd()
    {
        this.Complete(Maybe<string>.None);

        this.pendingAdd = new TaskCompletionSource<Maybe<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        this.EmailAddress = string.Empty;
        this.IsEditing = true;

        return this.pendingAdd.Task;
    }

    /// <summary>
    /// Abandons the address being added.
    /// </summary>
    public void Cancel()
    {
        this.IsEditing = false;
        this.Complete(Maybe<string>.None);
    }

    /// <summary>
    /// Finishes adding the typed address, unless it cannot join the recipients.
    /// </summary>
    public void Confirm()
    {
        if (!this.CanConfirm)
        {
            return;
        }

        this.IsEditing = false;
        this.Complete(this.EmailAddress.Trim());
    }

    private void Complete(Maybe<string> address)
    {
        this.pendingAdd?.TrySetResult(address);
        this.pendingAdd = null;
    }

    private Result ValidateNew(string address)
        => EmailAddressValidators.ValidateNew(address.Trim(), this.EmailAddresses);
}