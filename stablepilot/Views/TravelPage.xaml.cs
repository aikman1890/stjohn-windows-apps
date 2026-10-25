using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class TravelPage : Page
{
    public TravelPage()
    {
        this.InitializeComponent();
        TravelPicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) { }

    private async void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        var date = TravelPicker.Date ?? DateTimeOffset.Now;
        var horses = await new JsonListStore<Horse>("horses.json").LoadAsync();
        var clocks = await new JsonListStore<CareClock>("clocks.json").LoadAsync();
        var lines = new List<string>();
        foreach (var horse in horses.OrderBy(h => h.Name))
        {
            var mine = clocks.Where(c => c.HorseId == horse.Id).ToList();
            var problems = new List<string>();
            var coggins = mine.Where(c => c.Kind == "Coggins").OrderBy(c => c.NextDueDate).FirstOrDefault();
            if (coggins is null) problems.Add("no Coggins on record");
            else if (coggins.NextDueDate.Date < date.Date) problems.Add($"Coggins expires {coggins.NextDueDate:MMM d, yyyy} - before travel");
            foreach (var cvi in mine.Where(c => c.Kind == "CVI"))
                if (cvi.NextDueDate.Date < date.Date) problems.Add($"CVI window closes {cvi.NextDueDate:MMM d, yyyy}");
            foreach (var vax in mine.Where(c => c.Kind == "Vaccination"))
                if (vax.NextDueDate.Date < date.Date) problems.Add($"vaccination ({vax.Detail}) due {vax.NextDueDate:MMM d, yyyy}");
            lines.Add(problems.Count == 0
                ? $"TRAVEL-READY on the records: {horse.Name} (owner {horse.OwnerLabel})"
                : $"NOT READY: {horse.Name} - {string.Join("; ", problems)}");
        }
        ResultList.ItemsSource = lines;
    }
}
