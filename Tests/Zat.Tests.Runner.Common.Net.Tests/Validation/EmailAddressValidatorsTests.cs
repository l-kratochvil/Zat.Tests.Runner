namespace Zat.Tests.Runner.Common.Net.Tests.Validation;

using NUnit.Framework;

using Zat.Tests.Runner.Common.Net.Validation;

[TestFixture]
public class EmailAddressValidatorsTests
{
    [TestCase("tester@zat.cz", ExpectedResult = true)]
    [TestCase("first.last@example.com", ExpectedResult = true)]
    [TestCase(null, ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase("   ", ExpectedResult = false)]
    [TestCase("tester", ExpectedResult = false)]
    [TestCase("tester@", ExpectedResult = false)]
    [TestCase("@zat.cz", ExpectedResult = false)]
    [TestCase("tester@zat.cz, other@zat.cz", ExpectedResult = false)]
    [TestCase("Tester <tester@zat.cz>", ExpectedResult = false)]
    [TestCase(" tester@zat.cz", ExpectedResult = false)]
    public bool IsValidEmail__WhenGivenAddress__ThenShouldAcceptOnlyWholeSingleAddress(string? address)
        => EmailAddressValidators.IsValidEmail(address);

    [TestCase("tester@zat.cz", ExpectedResult = true)]
    [TestCase("tester@ZAT.CZ", ExpectedResult = true)]
    [TestCase("tester@example.com", ExpectedResult = false)]
    [TestCase("tester@mail.zat.cz", ExpectedResult = false)]
    [TestCase("tester@zat.cz.example.com", ExpectedResult = false)]
    [TestCase("tester@notzat.cz", ExpectedResult = false)]
    [TestCase("tester", ExpectedResult = false)]
    public bool IsInZatDomain__WhenGivenAddress__ThenShouldAcceptOnlyZatCzDomain(string address)
        => EmailAddressValidators.IsInZatDomain(address);
}