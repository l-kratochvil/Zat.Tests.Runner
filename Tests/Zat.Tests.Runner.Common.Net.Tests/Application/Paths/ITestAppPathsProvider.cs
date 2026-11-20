namespace Zat.Tests.Runner.Common.Net.Tests.Application.Paths;

using Zat.Tests.Runner.Common.Net.Application.Paths;

/// <summary>
/// Paths service interface of <see cref="TestAppPathsProvider"/>, standing for the one each
/// application registers its paths under.
/// </summary>
internal interface ITestAppPathsProvider : ISharedAppPathsProvider;