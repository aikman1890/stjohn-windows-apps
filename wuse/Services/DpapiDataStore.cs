using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Apis.Util.Store;
using Windows.Storage;

namespace Wuse.Services;

/// <summary>
/// Google.Apis IDataStore that keeps OAuth tokens in a DPAPI-encrypted file
/// inside the app's LocalFolder (CurrentUser scope: only this Windows user
/// can decrypt it). Tokens never sit in plain JSON on disk.
/// </summary>
public sealed class DpapiDataStore : IDataStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Dictionary<string, string> _cache = new();
    private bool _loaded;

    public DpapiDataStore(string fileName)
    {
        _filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);
    }

    public async Task StoreAsync<T>(string key, T value)
    {
        await _gate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            _cache[key] = JsonSerializer.Serialize(value);
            await SaveAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T> GetAsync<T>(string key)
    {
        await _gate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            if (_cache.TryGetValue(key, out var json))
            {
                try { return JsonSerializer.Deserialize<T>(json)!; }
                catch { return default!; }
            }
            return default!;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _gate.WaitAsync();
        try
        {
            _cache.Clear();
            await SaveAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task DeleteAsync<T>(string key)
    {
        await _gate.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            _cache.Remove(key);
            await SaveAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded)
            return;
        _loaded = true;
        try
        {
            if (!File.Exists(_filePath))
                return;
            var protectedBytes = await File.ReadAllBytesAsync(_filePath);
            var plain = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(plain);
            _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                     ?? new Dictionary<string, string>();
        }
        catch
        {
            _cache = new Dictionary<string, string>();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache);
            var plain = Encoding.UTF8.GetBytes(json);
            var protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(_filePath, protectedBytes);
        }
        catch
        {
            // Token persistence is best-effort; the in-memory copy still works
            // for this session.
        }
    }
}
