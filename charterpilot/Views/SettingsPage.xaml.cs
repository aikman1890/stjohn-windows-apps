using CharterPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CharterPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly CharterSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        BrokerBox.Text = s.BrokerName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        CurrencyBox.Text = s.DefaultCurrency;
        ClassesBox.Text = string.Join(Environment.NewLine, s.AircraftClasses);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.BrokerName = BrokerBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        s.DefaultCurrency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        s.AircraftClasses = ClassesBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
