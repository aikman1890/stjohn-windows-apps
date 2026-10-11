using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Wuse.Services;

/// <summary>
/// Persists named ink pages as ISF (Ink Serialized Format) files under
/// LocalFolder\InkNotes. All offline; no network, no telemetry.
/// </summary>
public sealed class InkNoteStore
{
    private const string FolderName = "InkNotes";
    private const string Extension = ".isf";

    public async Task<StorageFolder> GetFolderAsync()
    {
        return await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            FolderName, CreationCollisionOption.OpenIfExists);
    }

    /// <summary>Page names (file names without extension), sorted A-Z.</summary>
    public async Task<List<string>> GetPageNamesAsync()
    {
        try
        {
            var folder = await GetFolderAsync();
            var files = await folder.GetFilesAsync();
            return files
                .Where(f => string.Equals(f.FileType, Extension, StringComparison.OrdinalIgnoreCase))
                .Select(f => f.DisplayName)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task SaveAsync(string pageName, IBuffer isfData)
    {
        var fileName = SanitizePageName(pageName) + Extension;
        var folder = await GetFolderAsync();
        var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteBufferAsync(file, isfData);
    }

    public async Task<IBuffer?> LoadAsync(string pageName)
    {
        try
        {
            var fileName = SanitizePageName(pageName) + Extension;
            var folder = await GetFolderAsync();
            var file = await folder.GetFileAsync(fileName);
            return await FileIO.ReadBufferAsync(file);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string pageName)
    {
        try
        {
            var fileName = SanitizePageName(pageName) + Extension;
            var folder = await GetFolderAsync();
            var file = await folder.GetFileAsync(fileName);
            await file.DeleteAsync();
        }
        catch (FileNotFoundException)
        {
            // Already gone; nothing to do.
        }
    }

    public async Task<bool> ExistsAsync(string pageName)
    {
        try
        {
            var fileName = SanitizePageName(pageName) + Extension;
            var folder = await GetFolderAsync();
            _ = await folder.GetFileAsync(fileName);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    /// <summary>Makes a user-typed page name safe for use as a file name.</summary>
    public static string SanitizePageName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        clean = clean.Trim(' ', '.', '_');
        return clean.Length > 64 ? clean.Substring(0, 64) : clean;
    }
}
