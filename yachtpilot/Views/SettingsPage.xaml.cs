using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly YachtSettingsStore _store = new();

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
        CurrencyBox.Text = s.DefaultCurrency;
        TypesBox.Text = string.Join(Environment.NewLine, s.CertTypes);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.CompanyName = CompanyBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        s.DefaultCurrency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        s.CertTypes = TypesBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
