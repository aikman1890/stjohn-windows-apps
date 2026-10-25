using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        this.InitializeComponent();
        DayPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e) => await RefreshAsync();

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await RefreshAsync();

    private async Task RefreshAsync()
    {
        var settings = await new RangeSettingsStore().LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.BusinessName))
            TitleText.Text = $"{settings.BusinessName} - day board";
        var day = (DayPicker.Date ?? DateTimeOffset.Now).Date;

        var bookings = await new JsonListStore<Booking>("bookings.json").LoadAsync();
        var participants = await new JsonListStore<Participant>("participants.json").LoadAsync();
        var todays = bookings.Where(b => b.SlotDate.Date == day && b.Status != "Cancelled")
            .OrderBy(b => b.SlotTimeNote).ToList();
        BookingText.Text = todays.Count == 0 ? "No bookings on this day."
            : string.Join("\n", todays.Select(b =>
            {
                var party = participants.Where(p => p.BookingId == b.Id).ToList();
                var unsigned = party.Count(p => !p.WaiverSigned);
                var waiver = party.Count == 0 ? "no participants registered yet"
                    : unsigned == 0 ? "WAIVERS COMPLETE" : $"{unsigned} WAIVER(S) UNSIGNED - gate not clear";
                return $"{b.SlotTimeNote} - {b.PackageLabel} - {b.LeadGuest} x{b.PartySize} - balance {b.BalanceDue:0.00} {b.Currency} - {waiver}" +
                    (string.IsNullOrWhiteSpace(b.AssetLabel) ? string.Empty : $" - asset: {b.AssetLabel}") +
                    (string.IsNullOrWhiteSpace(b.StaffNote) ? string.Empty : $" - staff: {b.StaffNote}");
            }));

        var assets = await new JsonListStore<AssetItem>("assets.json").LoadAsync();
        var media = await new JsonListStore<MediaItem>("media.json").LoadAsync();
        var owed = media.Where(m => !m.Delivered).OrderByDescending(m => m.DaysWaiting).ToList();
        OpsText.Text = "Assets: " + (assets.Count == 0 ? "none recorded."
                : string.Join(" | ", assets.Select(a => $"{a.Name}: {a.Status}"))) +
            "\nMedia owed: " + (owed.Count == 0 ? "none - every package delivered."
                : string.Join("\n", owed.Select(m => m.DisplayLine)));
    }
}
