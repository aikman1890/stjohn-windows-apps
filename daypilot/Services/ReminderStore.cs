using System.Text.Json;
using DayPilot.Models;
using Windows.Storage;

namespace DayPilot.Services;

/// <summary>
/// Persists reminders as JSON in the app's local folder (reminders.json).
/// </summary>
public sealed class ReminderStore
{
    private const string FileName = "reminders.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public async Task<List<ReminderItem>> GetAllAsync()
    {
        return await LoadCoreAsync();
    }

    public async Task AddAsync(ReminderItem reminder)
    {
        var reminders = await LoadCoreAsync();
        reminders.Add(reminder);
        await SaveCoreAsync(reminders);
    }

    public async Task UpdateAsync(ReminderItem reminder)
    {
        var reminders = await LoadCoreAsync();
        var index = reminders.FindIndex(r => r.Id == reminder.Id);
        if (index >= 0)
            reminders[index] = reminder;
        else
            reminders.Add(reminder);
        await SaveCoreAsync(reminders);
    }

    public async Task DeleteAsync(Guid id)
    {
        var reminders = await LoadCoreAsync();
        reminders.RemoveAll(r => r.Id == id);
        await SaveCoreAsync(reminders);
    }

    private static async Task<List<ReminderItem>> LoadCoreAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<ReminderItem>>(json, Options) ?? new List<ReminderItem>();
        }
        catch (FileNotFoundException)
        {
            return new List<ReminderItem>();
        }
        catch (JsonException)
        {
            // Corrupt file: start fresh rather than crash.
            return new List<ReminderItem>();
        }
    }

    private static async Task SaveCoreAsync(List<ReminderItem> reminders)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(reminders, Options);
        await FileIO.WriteTextAsync(file, json);
    }
}
