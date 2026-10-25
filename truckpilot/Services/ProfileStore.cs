using System.Text.Json;
using TruckPilot.Models;
using Windows.Storage;

namespace TruckPilot.Services;

public sealed class ProfileStore
{
    private const string FileName = "profile.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<OperatorProfile> LoadAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var item = await folder.TryGetItemAsync(FileName);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<OperatorProfile>(json, Options) ?? new OperatorProfile();
            }
        }
        catch
        {
            // Corrupt or unreadable profile falls back to a blank profile.
        }
        return new OperatorProfile();
    }

    public async Task SaveAsync(OperatorProfile profile)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(profile, Options));
    }
}
