namespace Zat.Tests.Runner.TuiApp.Utils;

internal static class CommonUtils
{
    /// <summary>
    /// Picks the reference object and sets the reference to null.  
    /// </summary>
    /// <typeparam name="T">Type of the object to pick.</typeparam>
    /// <param name="reference">Reference to the object to pick.</param>
    /// <returns>Picked object.</returns>
    public static T? PickRef<T>(ref T? reference) where T : class
    {
        var value = reference;
        reference = null;
        return value;
    }
}