using System.IO;

namespace GameVault.Models;

/// <summary>
/// One cataloged game build on disk: a zip, executable, or HTML file the
/// vault found (or was pointed at), with an editable title and genre.
/// </summary>
public sealed class GameItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = "Uncategorized";
    public string FilePath { get; set; } = string.Empty;
    public string FileKind { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine => Title;

    public string SubLine => $"{Genre} - {FileKind} - {Path.GetFileName(FilePath)}";
}
