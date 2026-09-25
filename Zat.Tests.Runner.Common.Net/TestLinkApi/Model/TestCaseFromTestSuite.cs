namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

using System;

public sealed record TestCaseFromTestSuite(
    bool Active,
    int AuthorId,
    DateTime CreationTs,
    string Details,
    int ExecutionType,
    string ExternalId,
    int Id,
    int Importance,
    bool IsOpen,
    string Layout,
    DateTime ModificationTs,
    string Name,
    int NodeOrder,
    string NodeTable,
    int NodeTypeId,
    int ParentId,
    string Preconditions,
    int Status,
    string Summary,
    int TcversionId,
    int TestSuiteId,
    int UpdaterId,
    int Version);