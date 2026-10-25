using System.Text.Json;
using Windows.Storage;

namespace HuntPilot.Services;

public sealed class HuntSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<Models.HuntSettings> LoadAsync()
    {
        try
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<Models.HuntSettings>(json) ?? new Models.HuntSettings();
        }
        catch (FileNotFoundException)
        {
            return new Models.HuntSettings();
        }
    }

    public async Task SaveAsync(Models.HuntSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}
