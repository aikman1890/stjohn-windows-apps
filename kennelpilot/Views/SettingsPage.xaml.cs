using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KennelPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly KennelSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        NameBox.Text = s.KennelName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        NightlyBox.Text = s.NightlyRate.ToString("0.##");
        SecondBox.Text = s.SecondPetRate.ToString("0.##");
        BathBox.Text = s.BathRate.ToString("0.##");
        MedsBox.Text = s.MedicationRatePerDay.ToString("0.##");
        CapacityBox.Text = s.Capacity.ToString();
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = new KennelSettings
        {
            KennelName = NameBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            NightlyRate = ParseDec(NightlyBox.Text),
            SecondPetRate = ParseDec(SecondBox.Text),
            BathRate = ParseDec(BathBox.Text),
            MedicationRatePerDay = ParseDec(MedsBox.Text),
            Capacity = int.TryParse(CapacityBox.Text.Trim(), out var c) ? c : 0,
        };
        await _store.SaveAsync(s);
        StatusText.Text = "Settings saved.";
    }
}
