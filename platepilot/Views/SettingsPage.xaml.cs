using PlatePilot.Models;
using PlatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlatePilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly PlateSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        NameBox.Text = s.RestaurantName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        TargetBox.Text = s.DefaultTargetPercent.ToString("0.#");
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = new PlateSettings
        {
            RestaurantName = NameBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            DefaultTargetPercent = decimal.TryParse(TargetBox.Text.Trim(), out var v) && v > 0 ? v : 30m,
        };
        await _store.SaveAsync(s);
        StatusText.Text = "Settings saved.";
    }
}
