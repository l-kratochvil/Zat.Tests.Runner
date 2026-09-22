namespace Zat.Tests.Runner.TuiApp;

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
}