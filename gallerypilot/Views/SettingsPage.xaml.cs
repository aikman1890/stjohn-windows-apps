using System.Globalization;
using GalleryPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GalleryPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly GallerySettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        GalleryBox.Text = s.GalleryName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        CurrencyBox.Text = s.DefaultCurrency;
        TermsBox.Text = s.PaymentTermsDays.ToString(CultureInfo.InvariantCulture);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.GalleryName = GalleryBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        s.DefaultCurrency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        if (int.TryParse(TermsBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days)) s.PaymentTermsDays = days;
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
