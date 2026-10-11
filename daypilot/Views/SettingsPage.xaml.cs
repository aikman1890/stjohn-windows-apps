using DayPilot.Services;
using DayPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;

namespace DayPilot.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    private bool _populatingVoices;

    public SettingsPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
        LicenseStatusText.Text = Licensing.AppLicense.Manager.GetStatusText();
        PopulateVoices();
        VoiceProfileBox.Text = SpeechService.Instance.VoiceStudioProfile;
    }

    private void PopulateVoices()
    {
        _populatingVoices = true;
        try
        {
            var voices = SpeechService.Instance.AvailableVoices;
            VoicePicker.ItemsSource = voices;
            var selected = SpeechService.Instance.SelectedVoice;
            VoicePicker.SelectedItem = selected != null
                ? voices.FirstOrDefault(v => v.Id == selected.Id)
                : null;
        }
        finally
        {
            _populatingVoices = false;
        }
    }

    private async void VoicePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_populatingVoices)
            return;
        if (VoicePicker.SelectedItem is VoiceInformation voice)
        {
            SpeechService.Instance.SelectedVoice = voice;
            // A quick, friendly sample so the choice can be heard.
            await SpeechService.Instance.SpeakAsync("Hi there! This is how I sound now.");
        }
    }

    private void VoiceProfileBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            SpeechService.Instance.VoiceStudioProfile = VoiceProfileBox.Text ?? string.Empty;
        }
        catch
        {
        }
    }

    private void ActivateLicense_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        LicenseResultText.Text = string.Empty;
        if (Licensing.AppLicense.Manager.TryActivate(LicenseKeyBox.Text, LicenseEmailBox.Text, out var error))
        {
            LicenseResultText.Text = "Activated — thank you!";
            LicenseStatusText.Text = Licensing.AppLicense.Manager.GetStatusText();
        }
        else
        {
            LicenseResultText.Text = error;
        }
    }
}
