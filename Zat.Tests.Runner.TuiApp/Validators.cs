namespace Zat.Tests.Runner.TuiApp;

using Zat.Tests.Runner.Common.Net.Validation;

internal static class Validators
{
    public static ValidationResult IsDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return ValidationResult.Error(Resources.ValueRequired);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!DateTime.TryParse(dateString, out _))
        {
            return ValidationResult.Error(Resources.MustBeDateFormat);
        }

        return ValidationResult.Success();
    }

    public static ValidationResult IsInt(string? intString)
    {
        if (string.IsNullOrWhiteSpace(intString))
        {
            return ValidationResult.Error(Resources.ValueRequired);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!int.TryParse(intString, out _))
        {
            return ValidationResult.Error(Resources.MustBeInteger);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates an email address about to join <paramref name="existingAddresses"/>.
    /// </summary>
    public static ValidationResult IsNewZatEmailAddress(string? address, IEnumerable<string> existingAddresses)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return ValidationResult.Error(Resources.ValueRequired);
        }

        if (!EmailAddressValidators.IsValidEmail(address))
        {
            return ValidationResult.Error(Resources.EmailAddressInvalid_ErrorMessage);
        }

        if (!EmailAddressValidators.IsInZatDomain(address))
        {
            return ValidationResult.Error(Resources.EmailAddressNotInZatDomain_ErrorMessage);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (existingAddresses.Contains(address, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Error(Resources.EmailAddressDuplicate_ErrorMessage);
        }

        return ValidationResult.Success();
    }
}