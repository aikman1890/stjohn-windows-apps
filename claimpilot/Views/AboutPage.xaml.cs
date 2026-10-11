using ClaimPilot.Services;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ClaimPilot.Views;

/// <summary>
/// About page: full disclaimer, version, data handling — plus the hidden
/// Creator Unlock gesture (tap the version line 7 times). The Creator panel
/// lets the app's creator set/enter a local PIN that unlocks all paid
/// features on this machine. Local-only, salted-hash PIN, no network.
/// </summary>
public sealed partial class AboutPage : Page
{
    private const int CreatorTapThreshold = 7;
    private int _versionTaps;

    public AboutViewModel ViewModel { get; } = new();

    public AboutPage()
    {
        InitializeComponent();
    }

    private void VersionTapTarget_Tapped(object sender, TappedRoutedEventArgs e)
    {
        _versionTaps++;
        if (_versionTaps >= CreatorTapThreshold)
        {
            _versionTaps = 0;
            ShowCreatorPanel();
        }
    }

    private void ShowCreatorPanel()
    {
        CreatorPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        RefreshCreatorPanel();
    }

    private void RefreshCreatorPanel()
    {
        var hasPin = CreatorUnlockService.HasPin;
        var unlocked = CreatorUnlockService.IsUnlocked;

        CreatorHintText.Text = hasPin
            ? "Enter your creator PIN to unlock all paid features on this machine."
            : "First use: choose a creator PIN (4+ characters). It is stored only on this machine, salted and hashed — never in plaintext.";

        CreatorPrimaryButton.Content = hasPin ? "Unlock" : "Set PIN & Unlock";

        CreatorStatusText.Text = unlocked
            ? "Creator Unlock is ACTIVE on this machine. All paid features (including Bankruptcy Plus) are unlocked."
            : "Creator Unlock is not active.";

        CreatorPinBox.Password = string.Empty;
    }

    private void CreatorPrimaryButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var pin = CreatorPinBox.Password ?? string.Empty;

        bool ok;
        if (!CreatorUnlockService.HasPin)
        {
            ok = CreatorUnlockService.SetPin(pin);
            CreatorStatusText.Text = ok
                ? "Creator PIN set. Creator Unlock is ACTIVE on this machine — all paid features unlocked."
                : "Choose a PIN of at least 4 characters.";
        }
        else
        {
            ok = CreatorUnlockService.VerifyPin(pin);
            CreatorStatusText.Text = ok
                ? "Correct PIN. Creator Unlock is ACTIVE on this machine — all paid features unlocked."
                : "Wrong PIN. Try again.";
        }

        if (ok)
        {
            CreatorPinBox.Password = string.Empty;
        }
        RefreshCreatorPanelKeepStatus();
    }

    private void RefreshCreatorPanelKeepStatus()
    {
        var keep = CreatorStatusText.Text;
        RefreshCreatorPanel();
        CreatorStatusText.Text = keep;
    }

    private void CreatorLockButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CreatorUnlockService.Lock();
        CreatorStatusText.Text = "Creator Unlock deactivated on this machine. Enter the PIN to re-activate.";
        CreatorPinBox.Password = string.Empty;
        RefreshLicenseStatus();
    }

    private void AboutPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        RefreshLicenseStatus();
    }

    private void RefreshLicenseStatus()
    {
        BaseLicenseStatusText.Text = "App: " + Licensing.AppLicense.Base.GetStatusText();
        PlusLicenseStatusText.Text = "Bankruptcy Plus: " +
            (Licensing.AppLicense.CreatorUnlocked ? "unlocked (creator)." : Licensing.AppLicense.Plus.GetStatusText());
        LegalMattersStatusText.Text = "Legal Matters: " +
            (Licensing.AppLicense.CreatorUnlocked ? "unlocked (creator)." : Licensing.AppLicense.LegalMatters.GetStatusText());
    }

    private void ActivateBaseKey_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        BaseKeyResultText.Text = string.Empty;
        if (Licensing.AppLicense.Base.TryActivate(BaseKeyBox.Text, BaseEmailBox.Text, out var error))
        {
            BaseKeyResultText.Text = "Activated — thank you!";
            RefreshLicenseStatus();
        }
        else
        {
            BaseKeyResultText.Text = error;
        }
    }
}
