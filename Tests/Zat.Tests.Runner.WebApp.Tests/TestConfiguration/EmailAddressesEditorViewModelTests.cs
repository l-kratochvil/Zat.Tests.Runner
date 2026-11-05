namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

[TestFixture]
public class EmailAddressesEditorViewModelTests
{
    [TestCase("someone@zat.cz", ExpectedResult = false)]
    [TestCase("someone@example.com", ExpectedResult = true)]
    [TestCase("not an address", ExpectedResult = true)]
    public bool EmailAddresses__WhenSet__ThenShouldIndicateErrorsOfEveryAddress(string givenAddress)
    {
        // Given:
        var unit = new EmailAddressesEditorViewModel();

        // When:
        unit.EmailAddresses = ["first@zat.cz", givenAddress];

        // Then:
        return HasErrors(unit);
    }

    [Test]
    public void EmailAddress__WhenBeingTypedIsInvalid__ThenShouldNotIndicateErrorsOfList()
    {
        // Given:
        var unit = new EmailAddressesEditorViewModel { EmailAddresses = ["first@zat.cz"] };
        _ = unit.BeginAdd();

        // When:
        unit.EmailAddress = "someone@example.com";

        // Then:
        Assert.That(HasErrors(unit), Is.False);
    }

    [Test]
    public void CanConfirm__WhenAddressIsAlreadyListed__ThenShouldBeFalse()
    {
        // Given:
        var unit = new EmailAddressesEditorViewModel { EmailAddresses = ["first@zat.cz"] };
        _ = unit.BeginAdd();

        // When:
        unit.EmailAddress = "first@zat.cz";

        // Then:
        Assert.That(unit.CanConfirm, Is.False);
    }

    private static bool HasErrors(EmailAddressesEditorViewModel unit)
        => unit.GetValidity(nameof(EmailAddressesEditorViewModel.EmailAddresses))?.HasErrors ?? false;
}