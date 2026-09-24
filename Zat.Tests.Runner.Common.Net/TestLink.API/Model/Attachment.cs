namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

using System;

/// <summary>
/// The object returned from Testlinkt when requesting an attachment
/// </summary>
public sealed record Attachment(
    byte[] Content,
    DateTime Date_added,
    string File_type,
    int Id,
    string Name,
    string Title);