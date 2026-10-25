using System.Globalization;
using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class CalendarPage : Page
{
    private readonly JsonListStore<Booking> _store = new("bookings.json");
    private readonly JsonListStore<Vessel> _vesselStore = new("vessels.json");
    private List<Booking> _bookings = new();
    private List<Vessel> _vessels = new();
    private Booking? _selected;

    public CalendarPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Charter", "Owner use", "Yard" };
        StatusBox.ItemsSource = new[] { "Booked", "Completed", "Cancelled" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vessels = (await _vesselStore.LoadAsync()).OrderBy(v => v.Name).ToList();
        VesselBox.ItemsSource = _vessels.Select(v => v.Name).ToList();
        _bookings = (await _store.LoadAsync()).OrderBy(b => b.StartDate).ToList();
        BookingList.ItemsSource = _bookings;
        await RefreshFlagsAsync();
        if (_bookings.Count > 0) BookingList.SelectedIndex = 0;
    }

    private async Task RefreshFlagsAsync()
    {
        var crew = await new JsonListStore<CrewMember>("crew.json").LoadAsync();
        var certs = await new JsonListStore<Certificate>("certificates.json").LoadAsync();
        var flags = new List<string>();
        foreach (var b in _bookings.Where(b => b.Kind == "Charter" && b.Status == "Booked"))
        {
            var assigned = crew.Where(c => c.Active && c.VesselId == b.VesselId).Select(c => c.Id).ToHashSet();
            foreach (var cert in certs.Where(c => assigned.Contains(c.CrewId) && c.ExpiryDate.Date < b.EndDate.Date))
                flags.Add($"{b.VesselLabel} charter {b.StartDate:MMM d}-{b.EndDate:MMM d}: {cert.CrewLabel}'s {cert.Type} expires {cert.ExpiryDate:MMM d, yyyy} - inside or before this charter window.");
        }
        FlagsText.Text = flags.Count == 0 ? "No certificate-window conflicts on booked charters." : string.Join("\n", flags);
    }

    private void BookingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = BookingList.SelectedItem as Booking;
        if (_selected is null) return;
        VesselBox.SelectedItem = _selected.VesselLabel;
        KindBox.SelectedItem = _selected.Kind;
        StartPicker.Date = _selected.StartDate;
        EndPicker.Date = _selected.EndDate;
        ChartererBox.Text = _selected.ChartererName;
        BrokerBox.Text = _selected.Broker;
        RateBox.Text = _selected.Rate.ToString(CultureInfo.InvariantCulture);
        CurrencyBox.Text = _selected.Currency;
        ApaInBox.Text = _selected.ApaAdvance.ToString(CultureInfo.InvariantCulture);
        ApaOutBox.Text = _selected.ApaSpent.ToString(CultureInfo.InvariantCulture);
        StatusBox.SelectedItem = _selected.Status;
        NotesBox.Text = _selected.Notes;
        ErrorText.Text = string.Empty;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (VesselBox.SelectedIndex < 0) { ErrorText.Text = "Pick the vessel."; return; }
        var b = _selected ?? new Booking();
        var v = _vessels[VesselBox.SelectedIndex];
        b.VesselId = v.Id; b.VesselLabel = v.Name;
        b.Kind = KindBox.SelectedItem as string ?? b.Kind;
        b.StartDate = StartPicker.Date ?? b.StartDate;
        b.EndDate = EndPicker.Date ?? b.EndDate;
        if (b.EndDate < b.StartDate) { ErrorText.Text = "End date is before the start date."; return; }
        b.ChartererName = ChartererBox.Text.Trim();
        b.Broker = BrokerBox.Text.Trim();
        if (decimal.TryParse(RateBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var r)) b.Rate = r;
        b.Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        if (decimal.TryParse(ApaInBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var ai)) b.ApaAdvance = ai;
        if (decimal.TryParse(ApaOutBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var ao)) b.ApaSpent = ao;
        b.Status = StatusBox.SelectedItem as string ?? b.Status;
        b.Notes = NotesBox.Text.Trim();

        if (b.Status != "Cancelled")
        {
            var clash = _bookings.FirstOrDefault(o => o.Id != b.Id && o.VesselId == b.VesselId && o.Status != "Cancelled"
                && b.StartDate.Date <= o.EndDate.Date && o.StartDate.Date <= b.EndDate.Date);
            if (clash is not null)
            {
                ErrorText.Text = $"Not saved - overlaps an existing booking on {v.Name}: {clash.Kind} {clash.StartDate:MMM d} - {clash.EndDate:MMM d}.";
                return;
            }
        }
        if (_selected is null) _bookings.Add(b);
        await _store.SaveAsync(_bookings);
        _bookings = _bookings.OrderBy(x => x.StartDate).ToList();
        BookingList.ItemsSource = _bookings;
        await RefreshFlagsAsync();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _bookings.Remove(_selected);
        await _store.SaveAsync(_bookings);
        _selected = null;
        BookingList.ItemsSource = _bookings;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        BookingList.SelectedItem = null;
        ChartererBox.Text = BrokerBox.Text = RateBox.Text = ApaInBox.Text = ApaOutBox.Text = NotesBox.Text = string.Empty;
        CurrencyBox.Text = "USD";
        KindBox.SelectedItem = "Charter";
        StatusBox.SelectedItem = "Booked";
        StartPicker.Date = DateTimeOffset.Now.AddDays(14);
        EndPicker.Date = DateTimeOffset.Now.AddDays(21);
    }
}
