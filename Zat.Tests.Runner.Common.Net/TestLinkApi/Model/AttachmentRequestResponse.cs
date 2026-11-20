namespace Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

/// <summary>
/// this is returned as a response to an attachment request
/// </summary>
/// <param name="Description">description</param>
/// <param name="FileName">filename</param>
/// <param name="FileType">mime type</param>
/// <param name="ForeignKeyId">the foreign key</param>
/// <param name="LinkedTableName">the name of the table containing hte event this is attached to (an execution for instance)</param>
/// <param name="Size">size in bytes</param>
/// <param name="Title">title of the attachment</param>
public sealed record AttachmentRequestResponse(
    string Description,
    string FileName,
    string FileType,
    int ForeignKeyId,
    string LinkedTableName,
    int Size,
    string Title);