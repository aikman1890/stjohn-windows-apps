using System.Text.Json;
using Windows.Storage;

namespace RallyPilot.Services;

public sealed class RallySettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<Models.RallySettings> LoadAsync()
    {
        try
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<Models.RallySettings>(json) ?? new Models.RallySettings();
        }
        catch (FileNotFoundException)
        {
            return new Models.RallySettings();
        }
    }

    public async Task SaveAsync(Models.RallySettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}
