using System.Globalization;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly HuntSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        NameBox.Text = s.RanchName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        CurrencyBox.Text = s.DefaultCurrency;
        NormBox.Text = s.StageNormDays.ToString(CultureInfo.InvariantCulture);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.RanchName = NameBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        s.DefaultCurrency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        if (int.TryParse(NormBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var norm) && norm > 0) s.StageNormDays = norm;
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
