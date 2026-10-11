using System.Text.Json;
using Windows.Storage;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// Persists wuse's quick text notes as JSON in the app's local folder (wuse-notes.json).
/// </summary>
public sealed class TextNoteStore
{
    private const string FileName = "wuse-notes.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public async Task<List<TextNote>> GetAllAsync()
    {
        var notes = await LoadCoreAsync();
        notes.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
        return notes;
    }

    public async Task AddAsync(TextNote note)
    {
        var notes = await LoadCoreAsync();
        notes.Add(note);
        await SaveCoreAsync(notes);
    }

    private static async Task<List<TextNote>> LoadCoreAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<TextNote>>(json, Options) ?? new List<TextNote>();
        }
        catch (FileNotFoundException)
        {
            return new List<TextNote>();
        }
        catch (JsonException)
        {
            // Corrupt file: start fresh rather than crash.
            return new List<TextNote>();
        }
    }

    private static async Task SaveCoreAsync(List<TextNote> notes)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(notes, Options);
        await FileIO.WriteTextAsync(file, json);
    }
}
