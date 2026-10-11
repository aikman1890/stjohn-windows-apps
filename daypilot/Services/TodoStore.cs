using System.Text.Json;
using DayPilot.Models;
using Windows.Storage;

namespace DayPilot.Services;

/// <summary>
/// Persists to-do tasks as JSON in the app's local folder (todos.json).
/// </summary>
public sealed class TodoStore
{
    private const string FileName = "todos.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await LoadCoreAsync();
    }

    public async Task AddAsync(TodoItem todo)
    {
        var todos = await LoadCoreAsync();
        todos.Add(todo);
        await SaveCoreAsync(todos);
    }

    public async Task UpdateAsync(TodoItem todo)
    {
        var todos = await LoadCoreAsync();
        var index = todos.FindIndex(t => t.Id == todo.Id);
        if (index >= 0)
            todos[index] = todo;
        else
            todos.Add(todo);
        await SaveCoreAsync(todos);
    }

    public async Task DeleteAsync(Guid id)
    {
        var todos = await LoadCoreAsync();
        todos.RemoveAll(t => t.Id == id);
        await SaveCoreAsync(todos);
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
