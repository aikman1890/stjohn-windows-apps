using System.Globalization;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly GuardSettingsStore _store = new();

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
        OtBox.Text = s.OvertimeWeeklyThreshold.ToString(CultureInfo.InvariantCulture);
        TypesBox.Text = string.Join(Environment.NewLine, s.IncidentTypes);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.CompanyName = CompanyBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        if (decimal.TryParse(OtBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var ot)) s.OvertimeWeeklyThreshold = ot;
        s.IncidentTypes = TypesBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
