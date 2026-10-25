using System.Text.Json;
using Windows.Storage;

namespace YachtPilot.Services;

/// <summary>
/// Persists a list of items as JSON in the app's local folder.
/// Local storage only — nothing ever leaves this device.
/// </summary>
public sealed class JsonListStore<T>
{
    private readonly string _fileName;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public JsonListStore(string fileName)
    {
        _fileName = fileName;
    }

    public async Task<List<T>> LoadAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(_fileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }
        catch (FileNotFoundException)
        {
            return new List<T>();
        }
        catch (JsonException)
        {
            // Corrupt file: start fresh rather than crash.
            return new List<T>();
        }
    }

    public async Task SaveAsync(List<T> items)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(_fileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(items, Options);
        await FileIO.WriteTextAsync(file, json);
    }
}
