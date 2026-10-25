using System.Globalization;
using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class HuntsPage : Page
{
    private readonly JsonListStore<HuntBooking> _store = new("hunts.json");
    private readonly JsonListStore<Hunter> _hunterStore = new("hunters.json");
    private List<HuntBooking> _hunts = new();
    private List<Hunter> _hunters = new();
    private HuntBooking? _selected;

    public HuntsPage()
    {
        this.InitializeComponent();
        WaiverBox.ItemsSource = new[] { "Signed", "Partial", "Unsigned" };
        StatusBox.ItemsSource = new[] { "Booked", "In camp", "Completed", "Cancelled" };
        StartPicker.Date = DateTimeOffset.Now;
        EndPicker.Date = DateTimeOffset.Now.AddDays(2);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _hunters = (await _hunterStore.LoadAsync()).OrderBy(h => h.Name).ToList();
        HunterBox.ItemsSource = _hunters.Select(h => h.Name).ToList();
        _hunts = (await _store.LoadAsync()).OrderByDescending(h => h.StartDate).ToList();
        HuntList.ItemsSource = _hunts;
        if (_hunts.Count > 0) HuntList.SelectedIndex = 0;
    }

    private void HuntList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = HuntList.SelectedItem as HuntBooking;
        if (_selected is null) return;
        HunterBox.SelectedItem = _selected.HunterLabel;
        GuideBox.Text = _selected.GuideLabel;
        StartPicker.Date = _selected.StartDate;
        EndPicker.Date = _selected.EndDate;
        SpeciesBox.Text = _selected.SpeciesTargeted;
        PackageBox.Text = _selected.PackageLabel;
        PriceBox.Text = _selected.PackagePrice.ToString(CultureInfo.InvariantCulture);
        DepositBox.Text = _selected.DepositReceived.ToString(CultureInfo.InvariantCulture);
        ExtraBox.Text = _selected.ExtraCharges.ToString(CultureInfo.InvariantCulture);
        PaymentsBox.Text = _selected.PaymentsReceived.ToString(CultureInfo.InvariantCulture);
        LodgeBox.Text = $"{_selected.LodgingNights},{_selected.NonHunterGuests},{_selected.NonHunterRatePerNight.ToString(CultureInfo.InvariantCulture)}";
        WaiverBox.SelectedItem = _selected.WaiverStatus;
        ArrivalBox.Text = _selected.ArrivalNote;
        StatusBox.SelectedItem = _selected.Status;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (HunterBox.SelectedIndex < 0) { ErrorText.Text = "Pick the hunter."; return; }
        var h = _selected ?? new HuntBooking();
        var hunter = _hunters[HunterBox.SelectedIndex];
        h.HunterId = hunter.Id; h.HunterLabel = hunter.Name;
        h.GuideLabel = GuideBox.Text.Trim();
        h.StartDate = StartPicker.Date ?? h.StartDate;
        h.EndDate = EndPicker.Date ?? h.EndDate;
        h.SpeciesTargeted = SpeciesBox.Text.Trim();
        h.PackageLabel = PackageBox.Text.Trim();
        if (decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) h.PackagePrice = price;
        if (decimal.TryParse(DepositBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var dep)) h.DepositReceived = dep;
        if (decimal.TryParse(ExtraBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var extra)) h.ExtraCharges = extra;
        if (decimal.TryParse(PaymentsBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay)) h.PaymentsReceived = pay;
        var lodge = LodgeBox.Text.Split(',');
        if (lodge.Length == 3)
        {
            if (int.TryParse(lodge[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) h.LodgingNights = n;
            if (int.TryParse(lodge[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var g)) h.NonHunterGuests = g;
            if (decimal.TryParse(lodge[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var r)) h.NonHunterRatePerNight = r;
        }
        h.WaiverStatus = WaiverBox.SelectedItem as string ?? h.WaiverStatus;
        h.ArrivalNote = ArrivalBox.Text.Trim();
        h.Status = StatusBox.SelectedItem as string ?? h.Status;
        if (_selected is null) _hunts.Add(h);
        await _store.SaveAsync(_hunts);
        _hunts = _hunts.OrderByDescending(x => x.StartDate).ToList();
        HuntList.ItemsSource = _hunts;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        HuntList.SelectedItem = null;
        GuideBox.Text = SpeciesBox.Text = PackageBox.Text = ArrivalBox.Text = string.Empty;
        PriceBox.Text = DepositBox.Text = ExtraBox.Text = PaymentsBox.Text = "0";
        LodgeBox.Text = "0,0,0";
        StartPicker.Date = DateTimeOffset.Now;
        EndPicker.Date = DateTimeOffset.Now.AddDays(2);
        WaiverBox.SelectedItem = "Unsigned";
        StatusBox.SelectedItem = "Booked";
    }
}
