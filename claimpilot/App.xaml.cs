using ClaimPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QuestPDF;
using Windows.Storage;

namespace ClaimPilot;

/// <summary>
/// Application entry point. Sets the QuestPDF Community license, opens the main
/// window, initializes on-device storage, starts deadline reminders, and shows
/// the mandatory first-run legal disclaimer.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>Gets the application's main window once launched.</summary>
    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // QuestPDF Community license (see BUILD.md for license notes).
        Settings.License = LicenseType.Community;

        _window = new MainWindow();
        MainWindow = _window;
        _window.Activate();

        try
        {
            await DatabaseService.Instance.InitializeAsync();
        }
        catch
        {
            // Storage init failure must not crash startup; pages retry on demand.
        }

        try
        {
            NotificationService.Initialize();
        }
        catch
        {
            // Toast/reminder init is best-effort on this PC.
        }

        await ShowDisclaimerIfNeededAsync();
    }

    /// <summary>
    /// Shows the mandatory first-run disclaimer. The dialog cannot be dismissed
    /// without clicking "I Understand"; acceptance is stored in LocalSettings.
    /// </summary>
    private async Task ShowDisclaimerIfNeededAsync()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values["DisclaimerAccepted"] is true)
            {
                return;
            }

            if (_window?.Content is not FrameworkElement root)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Important Notice",
                Content = LegalText.Disclaimer,
                PrimaryButtonText = "I Understand",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = root.XamlRoot
            };

            // Block every dismissal path except the "I Understand" button.
            dialog.Closing += (s, e) =>
            {
                if (e.Result != ContentDialogResult.Primary)
                {
                    e.Cancel = true;
                }
            };

            await dialog.ShowAsync();
            settings.Values["DisclaimerAccepted"] = true;
        }
        catch
        {
            // A dialog failure must not crash startup; About page repeats the text.
        }
    }
}
