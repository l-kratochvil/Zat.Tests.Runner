namespace Zat.Tests.Runner.Common.Model;

/// <summary>
/// A file a test case attached to its result.
/// </summary>
/// <param name="description">The optional description of the attachment.</param>
/// <param name="filePath">The path of the attached file.</param>
[Serializable]
public class TestAttachment(
    string? description,
    string filePath)
{
    /// <summary>
    /// Gets the optional description of the attachment.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Gets the path of the attached file.
    /// </summary>
    public string FilePath { get; } = filePath;
}