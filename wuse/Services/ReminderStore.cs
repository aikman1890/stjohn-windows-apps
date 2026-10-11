using System.Text.Json;
using Windows.Storage;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// Persists wuse reminders as JSON in the app's local folder (wuse-reminders.json).
/// </summary>
public sealed class ReminderStore
{
    private const string FileName = "wuse-reminders.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public async Task<List<ReminderItem>> GetAllAsync() => await LoadCoreAsync();

    public async Task<List<ReminderItem>> GetUpcomingAsync()
    {
        var all = await LoadCoreAsync();
        return all.Where(r => !r.IsDone)
                  .OrderBy(r => r.DueAt)
                  .ToList();
    }

    public async Task<List<ReminderItem>> GetDueOnAsync(DateTime day)
    {
        var all = await LoadCoreAsync();
        return all.Where(r => !r.IsDone && r.DueAt.Date == day.Date)
                  .OrderBy(r => r.DueAt)
                  .ToList();
    }

    public async Task AddAsync(ReminderItem reminder)
    {
        var reminders = await LoadCoreAsync();
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
