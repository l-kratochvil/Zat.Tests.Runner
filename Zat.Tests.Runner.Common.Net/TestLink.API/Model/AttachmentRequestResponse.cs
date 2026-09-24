namespace Zat.Tests.Runner.Common.Net.TestLink.API.Model;

/// <summary>
/// this is returned as a response to an attachment request
/// </summary>
/// <param name="Description">description</param>
/// <param name="File_name">filename</param>
/// <param name="File_type">mime type</param>
/// <param name="ForeignKeyId">the foreign key</param>
/// <param name="LinkedTableName">the name of the table containing hte event this is attached to (an execution for instance)</param>
/// <param name="Size">size in bytes</param>
/// <param name="Title">title of the attachment</param>
public sealed record AttachmentRequestResponse(
    string Description,
    string File_name,
    string File_type,
    int ForeignKeyId,
    string LinkedTableName,
    int Size,
    string Title);