using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KennelPilot.Views;

public sealed partial class VaccineWatchPage : Page
{
    private readonly JsonListStore<Pet> _petStore = new("pets.json");

    public VaccineWatchPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var pets = await _petStore.LoadAsync();
        var today = DateTimeOffset.Now.Date;
        var expired = new List<string>();
        var d30 = new List<string>();
        var d60 = new List<string>();
        var d90 = new List<string>();
        var missing = new List<string>();

        foreach (var pet in pets.OrderBy(p => p.Name))
        {
            if (pet.Vaccinations.Count == 0)
            {
                missing.Add($"{pet.FullLabel} - no vaccination records on file");
                continue;
            }
            foreach (var v in pet.Vaccinations)
            {
                if (v.Expiry is not { } exp) continue;
                var days = (exp.Date - today).Days;
                var line = $"{pet.Name} ({pet.OwnerLabel}) - {v.Name} expires {exp:MMM d, yyyy}";
                if (days < 0) expired.Add($"{pet.Name} ({pet.OwnerLabel}) - {v.Name} EXPIRED {exp:MMM d, yyyy}");
                else if (days <= 30) d30.Add(line);
                else if (days <= 60) d60.Add(line);
                else if (days <= 90) d90.Add(line);
            }
        }
        ExpiredList.ItemsSource = expired;
        Days30List.ItemsSource = d30;
        Days60List.ItemsSource = d60;
        Days90List.ItemsSource = d90;
        MissingList.ItemsSource = missing;
    }
}
