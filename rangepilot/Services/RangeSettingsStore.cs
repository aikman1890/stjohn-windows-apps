using System.Text.Json;
using Windows.Storage;

namespace RangePilot.Services;

public sealed class RangeSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<Models.RangeSettings> LoadAsync()
    {
        try
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<Models.RangeSettings>(json) ?? new Models.RangeSettings();
        }
        catch (FileNotFoundException)
        {
            return new Models.RangeSettings();
        }
    }

    public async Task SaveAsync(Models.RangeSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}
