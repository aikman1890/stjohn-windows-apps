using System.Text.Json;
using PlotPilot.Models;
using Windows.Storage;

namespace PlotPilot.Services;

public sealed class PlotSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<PlotSettings> LoadAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<PlotSettings>(json, Options) ?? new PlotSettings();
            }
        }
        catch { /* blank settings */ }
        return new PlotSettings();
    }

    public async Task SaveAsync(PlotSettings settings)
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}
