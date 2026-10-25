using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FarewellPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly FarewellSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private static List<string> Lines(string text) =>
        text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        NameBox.Text = s.HomeName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        DcDaysBox.Text = s.DcFilingDays.ToString();
        DepositDaysBox.Text = s.PreneedDepositDays.ToString();
        BaseChecklistBox.Text = string.Join(Environment.NewLine, s.BaseChecklist);
        BurialBox.Text = string.Join(Environment.NewLine, s.BurialExtras);
        CremationBox.Text = string.Join(Environment.NewLine, s.CremationExtras);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = new FarewellSettings
        {
            HomeName = NameBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            DcFilingDays = int.TryParse(DcDaysBox.Text.Trim(), out var d) && d > 0 ? d : 10,
            PreneedDepositDays = int.TryParse(DepositDaysBox.Text.Trim(), out var pd) && pd > 0 ? pd : 10,
            BaseChecklist = Lines(BaseChecklistBox.Text),
            BurialExtras = Lines(BurialBox.Text),
            CremationExtras = Lines(CremationBox.Text),
        };
        await _store.SaveAsync(s);
        StatusText.Text = "Settings saved. New cases build their checklists from these lists; existing case checklists are unchanged.";
    }
}
