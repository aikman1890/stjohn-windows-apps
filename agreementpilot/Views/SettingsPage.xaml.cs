using AgreementPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AgreementPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly TradeSettingsStore _store = new();

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
        OrangeBox.Text = s.RenewalOrangeDays.ToString();
        WatchBox.Text = s.WatchAgeYears.ToString();
        PlansBox.Text = string.Join(Environment.NewLine, s.PlanNames);
        TypesBox.Text = string.Join(Environment.NewLine, s.EquipmentTypes);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.ShopName = ShopBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        if (int.TryParse(OrangeBox.Text, out var o)) s.RenewalOrangeDays = o;
        if (int.TryParse(WatchBox.Text, out var wa)) s.WatchAgeYears = wa;
        s.PlanNames = PlansBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        s.EquipmentTypes = TypesBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
