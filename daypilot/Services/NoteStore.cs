using System.Text.Json;
using DayPilot.Models;
using Windows.Storage;

namespace DayPilot.Services;

/// <summary>
/// Persists notes as JSON in the app's local folder (notes.json).
/// </summary>
public sealed class NoteStore
{
    private const string FileName = "notes.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public async Task<List<Note>> GetAllAsync()
    {
        var notes = await LoadCoreAsync();
        notes.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
        return notes;
    }

    public async Task AddAsync(Note note)
    {
        var notes = await LoadCoreAsync();
        notes.Add(note);
        await SaveCoreAsync(notes);
    }

    public async Task UpdateAsync(Note note)
    {
        var notes = await LoadCoreAsync();
        var index = notes.FindIndex(n => n.Id == note.Id);
        if (index >= 0)
            notes[index] = note;
        else
            notes.Add(note);
        await SaveCoreAsync(notes);
    }

    public async Task DeleteAsync(Guid id)
    {
        var notes = await LoadCoreAsync();
        notes.RemoveAll(n => n.Id == id);
        await SaveCoreAsync(notes);
    }

    private static async Task<List<Note>> LoadCoreAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<Note>>(json, Options) ?? new List<Note>();
        }
        catch (FileNotFoundException)
        {
            return new List<Note>();
        }
        catch (JsonException)
        {
            // Corrupt file: start fresh rather than crash.
            return new List<Note>();
        }
    }

    private static async Task SaveCoreAsync(List<Note> notes)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(notes, Options);
        await FileIO.WriteTextAsync(file, json);
    }
}
