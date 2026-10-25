using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly StableSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        BarnBox.Text = s.BarnName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        CurrencyBox.Text = s.DefaultCurrency;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.BarnName = BarnBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        s.DefaultCurrency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
