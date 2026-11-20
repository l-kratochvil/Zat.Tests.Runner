namespace Zat.Tests.Runner.Common.Extensions;

public static class IEnumerableExtensions
{
    /// <summary>
    /// NOTE: This should not be used on big data (it's allegedly inefficient)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
    {
        foreach (var item in array)
        {
            action(item);
        }
    }
}