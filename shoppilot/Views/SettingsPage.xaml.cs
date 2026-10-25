using System.Globalization;
using ShopPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ShopPilot.Views;

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
        ShopBox.Text = s.ShopName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        RateBox.Text = s.LaborRate.ToString(CultureInfo.InvariantCulture);
        CentersBox.Text = string.Join(Environment.NewLine, s.WorkCenters);
        OpsBox.Text = string.Join(Environment.NewLine, s.OperationLibrary);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.ShopName = ShopBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        if (decimal.TryParse(RateBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var r)) s.LaborRate = r;
        s.WorkCenters = CentersBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        s.OperationLibrary = OpsBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
