using TruckPilot.Models;
using TruckPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TruckPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly ProfileStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
        FuelTypeCombo.ItemsSource = new[] { "Diesel", "Gasoline", "Propane", "Other" };
        FuelTypeCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var p = await _store.LoadAsync();
        LegalNameBox.Text = p.LegalName;
        DbaBox.Text = p.Dba;
        AccountBox.Text = p.IftaAccount;
        BaseBox.Text = p.BaseJurisdiction;
        UnitBox.Text = p.UnitId;
        VinBox.Text = p.Vin;
        FuelTypeCombo.SelectedItem = string.IsNullOrWhiteSpace(p.FuelType) ? "Diesel" : p.FuelType;
        NotesBox.Text = p.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var p = new OperatorProfile
        {
            LegalName = LegalNameBox.Text.Trim(),
            Dba = DbaBox.Text.Trim(),
            IftaAccount = AccountBox.Text.Trim(),
            BaseJurisdiction = BaseBox.Text.Trim(),
            UnitId = UnitBox.Text.Trim(),
            Vin = VinBox.Text.Trim(),
            FuelType = FuelTypeCombo.SelectedItem as string ?? "Diesel",
            Notes = NotesBox.Text.Trim(),
        };
        await _store.SaveAsync(p);
        StatusText.Text = "Profile saved.";
    }
}
