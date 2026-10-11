using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wuse.Services;

namespace Wuse.ViewModels;

/// <summary>
/// A login row in the vault list (password never leaves the locker).
/// </summary>
public sealed class VaultLoginRow
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public partial class VaultViewModel : ObservableObject
{
    private readonly VaultService _vault = new();
    private readonly ClipboardService _clipboard = new();

    public ObservableCollection<VaultLoginRow> Logins { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string newSiteName = string.Empty;

    [ObservableProperty]
    private string newUsername = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public VaultViewModel()
    {
        Refresh();
    }

    partial void OnSearchTextChanged(string value) => Refresh();

    [RelayCommand]
    private void Refresh()
    {
        Logins.Clear();
        var q = SearchText.Trim().ToLowerInvariant();
        foreach (var entry in _vault.GetAll())
        {
            if (q.Length > 0 &&
                !entry.Name.Contains(q, StringComparison.OrdinalIgnoreCase) &&
                !entry.Username.Contains(q, StringComparison.OrdinalIgnoreCase))
                continue;
            Logins.Add(new VaultLoginRow { Name = entry.Name, Username = entry.Username });
        }
        if (Logins.Count == 0)
            StatusMessage = q.Length > 0 ? "No logins match your search." : "No logins saved yet.";
        else
            StatusMessage = string.Empty;
    }

    /// <summary>
    /// Called by the page with the password from the masked PasswordBox.
    /// The page clears its own box immediately after.
    /// </summary>
    public void AddLogin(string password)
    {
        var site = NewSiteName.Trim();
        if (site.Length == 0)
        {
            StatusMessage = "Give the login a name first — like the app or site.";
            return;
        }
        if (password.Length == 0)
        {
            StatusMessage = "Type a password to save.";
            return;
        }
        try
        {
            _vault.Save(site, NewUsername.Trim(), password);
            NewSiteName = string.Empty;
            NewUsername = string.Empty;
            Refresh();
            StatusMessage = $"Saved your login for {site} — encrypted in the Windows vault.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't save that: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CopyUsernameAsync(string site)
    {
        var retrieved = _vault.Retrieve(site);
        if (retrieved == null)
        {
            StatusMessage = $"Couldn't find a login for {site}.";
            return;
        }
        await _clipboard.CopyTextAsync(retrieved.Value.Username);
        StatusMessage = $"Username for {site} copied.";
    }

    [RelayCommand]
    private async Task CopyPasswordAsync(string site)
    {
        var retrieved = _vault.Retrieve(site);
        if (retrieved == null)
        {
            StatusMessage = $"Couldn't find a login for {site}.";
            return;
        }
        // Password goes to the clipboard and auto-clears after 30 seconds.
        await _clipboard.CopyTextAsync(retrieved.Value.Password, autoClearAfterSeconds: 30);
        StatusMessage = $"Password for {site} copied — the clipboard clears itself in 30 seconds.";
    }

    [RelayCommand]
    private async Task DeleteAsync(string site)
    {
        // The page shows the confirmation dialog and calls ConfirmDelete.
        await Task.CompletedTask;
    }

    public void ConfirmDelete(string site)
    {
        if (_vault.Delete(site))
        {
            Refresh();
            StatusMessage = $"Deleted your login for {site}.";
        }
        else
        {
            StatusMessage = $"Couldn't delete the login for {site}.";
        }
    }
}
