namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

using System;

/// <summary>
/// The object returned from Testlinkt when requesting an attachment
/// </summary>
public sealed record Attachment(
    byte[] Content,
    DateTime DateAdded,
    string FileType,
    int Id,
    string Name,
    string Title);