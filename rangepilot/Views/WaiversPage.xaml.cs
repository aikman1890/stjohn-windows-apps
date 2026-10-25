using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class WaiversPage : Page
{
    private readonly JsonListStore<Participant> _store = new("participants.json");
    private readonly JsonListStore<Booking> _bookingStore = new("bookings.json");
    private List<Booking> _bookings = new();

    public WaiversPage()
    {
        this.InitializeComponent();
        SignedBox.ItemsSource = new[] { "Signed", "Unsigned" };
        BookingBox.SelectionChanged += async (_, _) => await RefreshAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _bookings = (await _bookingStore.LoadAsync())
            .Where(b => b.Status != "Cancelled")
            .OrderBy(b => b.SlotDate).ToList();
        BookingBox.ItemsSource = _bookings.Select(b => $"{b.SlotDate:MMM d} {b.SlotTimeNote} - {b.LeadGuest}").ToList();
        if (_bookings.Count > 0) BookingBox.SelectedIndex = 0;
        await RefreshAsync();
    }

    private Booking? Current =>
        BookingBox.SelectedIndex >= 0 && BookingBox.SelectedIndex < _bookings.Count
            ? _bookings[BookingBox.SelectedIndex] : null;

    private async Task RefreshAsync()
    {
        if (Current is null) { PartyList.ItemsSource = null; GateText.Text = string.Empty; return; }
        var all = await _store.LoadAsync();
        var party = all.Where(p => p.BookingId == Current.Id).ToList();
        PartyList.ItemsSource = party;
        var unsigned = party.Count(p => !p.WaiverSigned);
        GateText.Text = party.Count == 0 ? "No participants registered for this booking yet."
            : unsigned == 0 ? $"GATE CLEAR: all {party.Count} participant(s) signed."
            : $"GATE NOT CLEAR: {unsigned} of {party.Count} participant(s) unsigned - resolve before the slot.";
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) { ErrorText.Text = "Pick the booking."; return; }
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "Name the participant."; return; }
        var all = await _store.LoadAsync();
        var existing = all.FirstOrDefault(p => p.BookingId == Current.Id &&
            p.Name.Equals(NameBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
        var signed = SignedBox.SelectedItem as string == "Signed";
        if (existing is not null)
        {
            existing.WaiverSigned = signed;
            existing.WaiverVersion = VersionBox.Text.Trim();
            existing.GuardianSigner = GuardianBox.Text.Trim();
            existing.IsMinor = !string.IsNullOrWhiteSpace(GuardianBox.Text);
        }
        else
        {
            all.Add(new Participant
            {
                BookingId = Current.Id,
                BookingLabel = $"{Current.LeadGuest} {Current.SlotTimeNote}",
                Name = NameBox.Text.Trim(),
                WaiverSigned = signed,
                WaiverVersion = VersionBox.Text.Trim(),
                GuardianSigner = GuardianBox.Text.Trim(),
                IsMinor = !string.IsNullOrWhiteSpace(GuardianBox.Text),
            });
        }
        await _store.SaveAsync(all);
        NameBox.Text = GuardianBox.Text = string.Empty;
        await RefreshAsync();
    }
}
