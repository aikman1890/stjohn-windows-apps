using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly ShopSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        ShopNameBox.Text = s.ShopName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        LaborRateBox.Text = s.LaborRate.ToString("0.##");
        TaxBox.Text = s.TaxPercent.ToString("0.##");
        SuppliesBox.Text = s.SuppliesFee.ToString("0.##");
        MarkupBox.Text = s.DefaultMarkupPercent.ToString("0.##");
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = new ShopSettings
        {
            ShopName = ShopNameBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            LaborRate = ParseDec(LaborRateBox.Text),
            TaxPercent = ParseDec(TaxBox.Text),
            SuppliesFee = ParseDec(SuppliesBox.Text),
            DefaultMarkupPercent = ParseDec(MarkupBox.Text),
        };
        await _store.SaveAsync(s);
        StatusText.Text = "Settings saved.";
    }
}
