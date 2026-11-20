namespace Zat.Tests.Runner.TuiApp.Extensions;

using Zat.Tests.Runner.Common.Model;

internal static class TestEntityExtensions
{
    extension(TestEntity)
    {
        public static EqualityComparer<TestEntity> CreateEqualityComparerByName()
            => EqualityComparer<TestEntity>.Create(
                equals: (x, y) => x?.Name == y?.Name,
                getHashCode: x => x.Name.GetHashCode(StringComparison.Ordinal));
    }
}