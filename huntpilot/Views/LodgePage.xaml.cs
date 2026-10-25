using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class LodgePage : Page
{
    public LodgePage()
    {
        this.InitializeComponent();
        DayPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e) => await ShowAsync();

    private async void ShowButton_Click(object sender, RoutedEventArgs e) => await ShowAsync();

    private async Task ShowAsync()
    {
        var day = (DayPicker.Date ?? DateTimeOffset.Now).Date;
        var hunts = await new JsonListStore<HuntBooking>("hunts.json").LoadAsync();
        var inCamp = hunts.Where(h => h.Status != "Cancelled" && h.StartDate.Date <= day && h.EndDate.Date >= day)
            .OrderBy(h => h.HunterLabel).ToList();
        var arriving = hunts.Where(h => h.Status != "Cancelled" && h.StartDate.Date == day).ToList();
        BoardText.Text = inCamp.Count == 0 ? "Nobody in camp on this day."
            : string.Join("\n", inCamp.Select(h =>
                $"{h.HunterLabel} - {h.SpeciesTargeted} - guide {h.GuideLabel} - room {h.RoomNote}" +
                (h.NonHunterGuests > 0 ? $" - +{h.NonHunterGuests} non-hunter guest(s)" : string.Empty) +
                (arriving.Contains(h) && !string.IsNullOrWhiteSpace(h.ArrivalNote) ? $"\n  ARRIVING TODAY: {h.ArrivalNote}" : string.Empty)));
    }
}
