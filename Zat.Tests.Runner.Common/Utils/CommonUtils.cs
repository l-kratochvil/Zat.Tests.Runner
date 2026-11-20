namespace Zat.Tests.Runner.Common.Utils;

public static class CommonUtils
{
    internal static int CalculateHashCode(params int[] hashCodes)
        => hashCodes.Aggregate(17, (curr, prev) => curr * 31 + prev);
}