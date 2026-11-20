namespace Zat.Tests.Runner.WebApp.Tests.TestConfiguration;

using System.Linq;

using NUnit.Framework;

using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Z2xxTests.Common.Model;

[TestFixture]
public class TestStationLabelsTests
{
    [Test]
    public void Offered__WhenStationsAreOffered__ThenShouldHoldEveryStationThereIs()
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
    public void Offered__WhenStationsAreOffered__ThenShouldLeaveOutUnknownOne()
    {
        // Given:
        // Unknown stands for a station nobody chose, which the empty choice already says.

        // Then:
        Assert.That(TestStationLabels.Offered, Does.Not.Contain(HwAssemblyType.Unknown));
    }

    [Test]
    public void For__WhenStationIsOffered__ThenShouldNameIt()
    {
        // Then:
        Assert.That(
            TestStationLabels.Offered.Select(TestStationLabels.For),
            Has.All.Not.Empty);
    }
}