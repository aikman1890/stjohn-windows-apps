using System.Text.Json;
using BayPilot.Models;
using Windows.Storage;

namespace BayPilot.Services;

/// <summary>
/// Persists the single ShopSettings object as JSON in the local folder.
/// </summary>
public sealed class ShopSettingsStore
{
    private const string FileName = "settings.json";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<ShopSettings> LoadAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<ShopSettings>(json, Options) ?? new ShopSettings();
        }
        catch (FileNotFoundException)
        {
            return new ShopSettings();
        }
        catch (JsonException)
        {
            return new ShopSettings();
        }
    }

    public async Task SaveAsync(ShopSettings settings)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(settings, Options));
    }
}
