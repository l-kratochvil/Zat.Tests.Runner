namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
/// Represents a platform against which a test result can be recorded
/// </summary>
/// <param name="Id">primary key</param>
public sealed record TestPlatform(int Id, string Name, string Notes);