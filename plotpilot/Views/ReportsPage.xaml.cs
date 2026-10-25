using System.Text;
using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PlotPilot.Views;

public sealed partial class ReportsPage : Page
{
    private readonly JsonListStore<PlotSpace> _spaceStore = new("spaces.json");
    private readonly JsonListStore<BurialRecord> _burialStore = new("burials.json");
    private readonly JsonListStore<ChainEntry> _chainStore = new("chain.json");

    public ReportsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var spaces = await _spaceStore.LoadAsync();
        var burials = await _burialStore.LoadAsync();
        var chain = await _chainStore.LoadAsync();

        InventoryList.ItemsSource = spaces.GroupBy(s => s.Section).OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()} spaces - {g.Count(s => s.Status == "Available")} available, {g.Count(s => s.Status == "Sold")} sold, {g.Count(s => s.Status == "Occupied")} occupied, {g.Count(s => s.Status == "Unknown")} unknown")
            .ToList();

        BurialList.ItemsSource = burials.GroupBy(b => b.IntermentDate.Year).OrderByDescending(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()} burial(s)").ToList();

        SalesList.ItemsSource = chain.Where(c => c.Kind == "Sale").GroupBy(c => c.Date.Year).OrderByDescending(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()} sale(s) - {g.Sum(c => c.Amount):C} - care contributions {g.Sum(c => c.CareContribution):C}")
            .ToList();
    }

    private async void CsvButton_Click(object sender, RoutedEventArgs e)
    {
        var spaces = await _spaceStore.LoadAsync();
        var burials = await _burialStore.LoadAsync();
        string Q(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
        var sb = new StringBuilder();
        sb.AppendLine("SPACES");
        sb.AppendLine("Section,Block,Lot,Space,Status,Owner,DeedRef,PurchaseDate,PurchasePrice,CareContribution,Source,Notes");
        foreach (var s in spaces.OrderBy(s => s.Address))
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Q(s.Section), Q(s.Block), Q(s.Lot), Q(s.Space), Q(s.Status), Q(s.OwnerName), Q(s.DeedRef),
                s.PurchaseDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                s.PurchasePrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                s.CareContribution.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Q(s.Source), Q(s.Notes),
            }));
        }
        sb.AppendLine();
        sb.AppendLine("BURIALS");
        sb.AppendLine("Space,Decedent,Born,Died,Interred,FuneralHome,Marker,Veteran,Source");
        foreach (var b in burials.OrderBy(b => b.IntermentDate))
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Q(b.SpaceLabel), Q(b.DecedentName),
                b.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                b.DeathDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                b.IntermentDate.ToString("yyyy-MM-dd"),
                Q(b.FuneralHome), Q(b.MarkerNotes), b.Veteran ? "Yes" : "No", Q(b.Source),
            }));
        }
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("CSV file", new List<string> { ".csv" });
        picker.SuggestedFileName = "cemetery-register-export.csv";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Full register exported to {file.Path}";
    }
}
