namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;

using Zat.Z2xxTests.Common.Model;

/// <summary>
/// The test stations the configurator offers, and what each one is called on screen.
/// </summary>
/// <remarks>
/// The labels are written out one by one rather than derived from the names of
/// <see cref="HwAssemblyType"/>, so that what the tester reads can be changed without
/// touching the domain and a station whose name does not follow the pattern needs no exception.
/// <para>
/// <see cref="HwAssemblyType.Unknown"/> is deliberately not offered: it stands for a station
/// nobody chose, which is what leaving the field empty already says.
/// </para>
/// </remarks>
public static class TestStationLabels
{
    private static readonly IReadOnlyDictionary<HwAssemblyType, string> LabelsByStation =
        new Dictionary<HwAssemblyType, string>
        {
            [HwAssemblyType.HW00] = "HW00",
            [HwAssemblyType.HW01] = "HW01",
            [HwAssemblyType.HW02_BB1M] = "HW02 - BB1M",
            [HwAssemblyType.HW02_BB37M] = "HW02 - BB37M",
            [HwAssemblyType.HW03] = "HW03",
            [HwAssemblyType.HW02_BD1M] = "HW04 - BD1M",
        };

    /// <summary>
    /// Gets the stations that can be chosen, in the order they are offered in.
    /// </summary>
    public static IReadOnlyList<HwAssemblyType> Offered { get; } = [.. LabelsByStation.Keys];

    /// <summary>
    /// Reads what a station is called on screen.
    /// </summary>
    /// <param name="station">Station to name.</param>
    /// <returns>The label, falling back to the name of the value when there is none.</returns>
    public static string For(HwAssemblyType station)
        => LabelsByStation.TryGetValue(station, out var label) ? label : station.ToString();
}