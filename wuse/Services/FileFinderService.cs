using Windows.System;

namespace Wuse.Services;

/// <summary>
/// A single file-search hit.
/// </summary>
public sealed class FileHit
{
    public string FullPath { get; set; } = string.Empty;
    public string Name => Path.GetFileName(FullPath);
    public string Directory => Path.GetDirectoryName(FullPath) ?? string.Empty;
    public long SizeBytes { get; set; }
}

/// <summary>
/// Read-only file finding across the user's everyday folders: Documents,
/// Downloads, Desktop, OneDrive, and the profile root (one level deep).
/// Never moves, renames, or deletes anything.
/// </summary>
public sealed class FileFinderService
{
    private const int MaxResults = 20;

    /// <summary>
    /// Searches for files whose names contain the query. Fast by design:
    /// top two levels of each root, hidden/system skipped.
    /// </summary>
    public Task<IReadOnlyList<FileHit>> FindAsync(string query)
    {
        var q = (query ?? string.Empty).Trim();
        var hits = new List<FileHit>();
        if (q.Length < 2)
            return Task.FromResult<IReadOnlyList<FileHit>>(hits);

        foreach (var root in SearchRoots())
        {
            if (hits.Count >= MaxResults || !Directory.Exists(root))
                continue;
            try
            {
                ScanLevel(root, q, hits);
                foreach (var dir in SafeEnumerateDirectories(root))
                {
                    if (hits.Count >= MaxResults)
                        break;
                    if (IsSkippedDirectory(dir))
                        continue;
                    ScanLevel(dir, q, hits);
                }
            }
            catch
            {
                // One bad root never sinks the whole search.
            }
        }
        return Task.FromResult<IReadOnlyList<FileHit>>(hits);
    }

    public async Task<string> OpenAsync(FileHit hit)
    {
        try
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(hit.FullPath);
            var opened = await Launcher.LaunchFileAsync(file);
            return opened ? $"Opening {hit.Name}…" : $"The system declined to open {hit.Name}.";
        }
        catch (Exception ex)
        {
            return $"I couldn't open {hit.Name}: {ex.Message}";
        }
    }

    public static string FormatSize(long bytes) =>
        bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
            < 1024L * 1024 * 1024 => $"{bytes / 1048576.0:0.#} MB",
            _ => $"{bytes / 1073741824.0:0.#} GB",
        };

    private static IEnumerable<string> SearchRoots()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var roots = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(profile, "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Path.Combine(profile, "OneDrive"),
            profile, // top level only — quick wins like installers on C:\Users\<name>
        };
        return roots.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void ScanLevel(string dir, string query, List<FileHit> hits)
    {
        foreach (var file in SafeEnumerateFiles(dir))
        {
            if (hits.Count >= MaxResults)
                return;
            var name = Path.GetFileName(file);
            if (!name.Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                var info = new FileInfo(file);
                hits.Add(new FileHit { FullPath = file, SizeBytes = info.Length });
            }
            catch
            {
                // Unreadable file: skip it.
            }
        }
    }

    private static bool IsSkippedDirectory(string dir)
    {
        var name = Path.GetFileName(dir);
        return name.Equals("AppData", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith(".", StringComparison.Ordinal)
            || name.Equals("OneDrive", StringComparison.OrdinalIgnoreCase); // searched as its own root
    }

    private static IEnumerable<string> SafeEnumerateFiles(string dir)
    {
        try
        {
            return Directory.EnumerateFiles(dir, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
            });
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string dir)
    {
        try
        {
            return Directory.EnumerateDirectories(dir, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
            });
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }
}
