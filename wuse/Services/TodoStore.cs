using System.Text.Json;
using Windows.Storage;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// Persists wuse's todo list as JSON in the app's local folder (wuse-todos.json).
/// </summary>
public sealed class TodoStore
{
    private const string FileName = "wuse-todos.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public async Task<List<TodoItem>> GetAllAsync() => await LoadCoreAsync();

    public async Task<List<TodoItem>> GetOpenAsync()
    {
        var all = await LoadCoreAsync();
        return all.Where(t => !t.IsDone)
                  .OrderBy(t => t.CreatedAt)
                  .ToList();
    }

    public async Task AddAsync(TodoItem todo)
    {
        var todos = await LoadCoreAsync();
        todos.Add(todo);
        await SaveCoreAsync(todos);
    }

    public async Task MarkDoneAsync(Guid id)
    {
        var todos = await LoadCoreAsync();
        var match = todos.FirstOrDefault(t => t.Id == id);
        if (match != null)
        {
            match.IsDone = true;
            await SaveCoreAsync(todos);
        }
    }

    public async Task<int> ClearCompletedAsync()
    {
        var todos = await LoadCoreAsync();
        int removed = todos.RemoveAll(t => t.IsDone);
        if (removed > 0)
            await SaveCoreAsync(todos);
        return removed;
    }

    private static async Task<List<TodoItem>> LoadCoreAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<TodoItem>>(json, Options) ?? new List<TodoItem>();
        }
        catch (FileNotFoundException)
        {
            return new List<TodoItem>();
        }
        catch (JsonException)
        {
            // Corrupt file: start fresh rather than crash.
            return new List<TodoItem>();
        }
    }

    private static async Task SaveCoreAsync(List<TodoItem> todos)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(todos, Options);
        await FileIO.WriteTextAsync(file, json);
    }
}
