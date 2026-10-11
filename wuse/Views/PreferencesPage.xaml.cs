using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Wuse.Services;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class PreferencesPage : Page
{
    public PreferencesViewModel ViewModel { get; }

    public PreferencesPage()
    {
        ViewModel = new PreferencesViewModel(App.Settings);
        this.InitializeComponent();
    }

    private void RemoveFactButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is string fact)
            ViewModel.RemoveFactCommand.Execute(fact);
    }

    private void NewFactTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.AddFactCommand.CanExecute(null))
        {
            ViewModel.AddFactCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void ActivateLicense_Click(object sender, RoutedEventArgs e)
    {
        LicenseResultText.Text = string.Empty;
        if (Licensing.AppLicense.TryActivateAny(LicenseKeyBox.Text, LicenseEmailBox.Text, out var error))
        {
            LicenseResultText.Text = "Activated — thank you!";
            LicenseStatusText.Text = Licensing.AppLicense.GetStatusText();
        }
        else
        {
            LicenseResultText.Text = error;
        }
    }

    private void PreferencesPage_Loaded(object sender, RoutedEventArgs e)
    {
        LicenseStatusText.Text = Licensing.AppLicense.GetStatusText();
    }

    private async void PreviewVoiceButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SpeechService.Instance.SpeakAsync(
                $"Hi! I'm {ViewModel.AssistantName}, and I'm happy to help with whatever you need.");
        }
        catch
        {
            // Preview is a nice-to-have.
        }
    }
}
