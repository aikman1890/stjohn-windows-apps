using System.Globalization;
using MovePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MovePilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly MoveSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        CompanyBox.Text = s.CompanyName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        RateBox.Text = s.DefaultHourlyRate.ToString(CultureInfo.InvariantCulture);
        CrewBox.Text = s.DefaultCrewSize.ToString();
        AckBox.Text = s.DefaultClaimAckDays.ToString();
        PresetsBox.Text = string.Join(Environment.NewLine, s.ItemPresets);
        TermsBox.Text = string.Join(Environment.NewLine, s.TermsPresets);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.CompanyName = CompanyBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        if (decimal.TryParse(RateBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var r)) s.DefaultHourlyRate = r;
        if (int.TryParse(CrewBox.Text, out var c)) s.DefaultCrewSize = c;
        if (int.TryParse(AckBox.Text, out var a)) s.DefaultClaimAckDays = a;
        s.ItemPresets = PresetsBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        s.TermsPresets = TermsBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
