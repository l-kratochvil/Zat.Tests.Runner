namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

public sealed record TestCaseFromTestPlan(
    int TestCaseId,
    string FullExternalId,
    string Name);
