using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly TaxSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        OfficeBox.Text = s.OfficeName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        YearBox.Text = s.CurrentTaxYear.ToString();
        EntitiesBox.Text = string.Join(Environment.NewLine, s.EntityTypes);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.OfficeName = OfficeBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        if (int.TryParse(YearBox.Text, out var y)) s.CurrentTaxYear = y;
        s.EntityTypes = EntitiesBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
