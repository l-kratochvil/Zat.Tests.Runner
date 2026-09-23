namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using System.Linq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Z2xxTests.Common.Model;

[TestFixture]
public class TestStationLabelsTests
{
    [Test]
    public void Offered__WhenTheStationsAreOffered__ThenShouldHoldEveryStationThereIs()
    {
        // Given:
        // The labels are written out one by one, so a station added to the domain would otherwise
        // quietly go missing from the configurator.
        IEnumerable<HwAssemblyType> expected = Enum
            .GetValues<HwAssemblyType>()
            .Where(static station => station is not HwAssemblyType.Unknown);

        // Then:
        Assert.That(TestStationLabels.Offered, Is.EquivalentTo(expected));
    }

    [Test]
    public void Offered__WhenTheStationsAreOffered__ThenShouldLeaveOutTheUnknownOne()
    {
        // Given:
        // Unknown stands for a station nobody chose, which the empty choice already says.

        // Then:
        Assert.That(TestStationLabels.Offered, Does.Not.Contain(HwAssemblyType.Unknown));
    }

    [Test]
    public void For__WhenAStationIsOffered__ThenShouldNameIt()
    {
        // Then:
        Assert.That(
            TestStationLabels.Offered.Select(TestStationLabels.For),
            Has.All.Not.Empty);
    }

    [TestCase(HwAssemblyType.HW00, "HW00")]
    [TestCase(HwAssemblyType.HW02_BB1M, "HW02 - 1M")]
    [TestCase(HwAssemblyType.HW02_BB37M, "HW02 - 37M")]
    public void For__WhenAStationIsNamed__ThenShouldReadAsItIsWrittenDown(
        HwAssemblyType givenStation, string expectedLabel)
    {
        // Then:
        Assert.That(TestStationLabels.For(givenStation), Is.EqualTo(expectedLabel));
    }

    [Test]
    public void For__WhenAStationIsNotOffered__ThenShouldFallBackToItsName()
    {
        // Given:
        // Nothing should ask, but a label that is missing must not take the screen down with it.

        // Then:
        Assert.That(
            TestStationLabels.For(HwAssemblyType.Unknown),
            Is.EqualTo(nameof(HwAssemblyType.Unknown)));
    }
}