using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StJohn.Licensing;
using Windows.System;

namespace ClaimPilot.Views;

/// <summary>
/// Full-window lockout page. Shown by MainWindow instead of the normal shell
/// when no valid base license key is stored (and the creator unlock is not
/// active). All navigation is blocked until the user activates a key.
/// </summary>
public sealed partial class LockoutPage : Page
{
    private readonly LicenseManager _licensing;

    /// <summary>Raised when the user successfully activates a license key.</summary>
    public event EventHandler? Unlocked;

    public LockoutPage(LicenseManager licensing)
    {
        this.InitializeComponent();
        _licensing = licensing ?? throw new ArgumentNullException(nameof(licensing));
        AppNameText.Text = licensing.Config.AppName;
        StatusText.Text = licensing.GetStatusText();
        BuyButton.Content = $"Buy — {licensing.Config.PriceDisplay}";
        BackupSummaryText.Text = licensing.GetSavedWorkSummary();
    }

    private void Activate_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (_licensing.TryActivate(KeyBox.Text, EmailBox.Text, out var error))
        {
            Unlocked?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ErrorText.Text = error;
        }
    }

    private async void Buy_Click(object sender, RoutedEventArgs e)
    {
        try { await Launcher.LaunchUriAsync(new Uri(_licensing.Config.BuyMailto)); }
        catch { /* mail client unavailable */ }
    }

    private async void Support_Click(object sender, RoutedEventArgs e)
    {
        try { await Launcher.LaunchUriAsync(new Uri(_licensing.Config.SupportUrl)); }
        catch { /* browser unavailable */ }
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        RestoreResultText.Text = "Restoring…";
        try
        {
            var (files, zips) = await _licensing.RestoreSavedWorkAsync();
            RestoreResultText.Text = (files + zips) == 0
                ? "Nothing to restore."
                : $"Restored {files} file(s)" + (zips > 0 ? $" and extracted {zips} backup(s)." : ".");
            BackupSummaryText.Text = _licensing.GetSavedWorkSummary();
        }
        catch
        {
            RestoreResultText.Text = "Restore failed. Your Saved Work folder in Documents is untouched.";
        }
    }
}
