using System.Globalization;
using System.Net;
using System.Text;
using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace StablePilot.Views;

public sealed partial class BillingPage : Page
{
    private readonly JsonListStore<ExtraCharge> _extraStore = new("extras.json");
    private readonly JsonListStore<Horse> _horseStore = new("horses.json");
    private readonly JsonListStore<HorseOwner> _ownerStore = new("owners.json");
    private List<ExtraCharge> _extras = new();
    private List<Horse> _horses = new();
    private List<HorseOwner> _owners = new();
    private ExtraCharge? _selected;

    public BillingPage()
    {
        this.InitializeComponent();
        CategoryBox.ItemsSource = new[] { "Holding for vet/farrier", "Supplies", "Show fees advanced", "Other extra" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _horses = (await _horseStore.LoadAsync()).OrderBy(h => h.Name).ToList();
        HorseBox.ItemsSource = _horses.Select(h => h.Name).ToList();
        _owners = (await _ownerStore.LoadAsync()).OrderBy(o => o.Name).ToList();
        OwnerBox.ItemsSource = _owners.Select(o => o.Name).ToList();
        MonthBox.Text = DateTimeOffset.Now.ToString("yyyy-MM");
        _extras = (await _extraStore.LoadAsync()).OrderByDescending(x => x.Date).ToList();
        ExtraList.ItemsSource = _extras;
    }

    private void ExtraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ExtraList.SelectedItem as ExtraCharge;
    }

    private async void AddExtraButton_Click(object sender, RoutedEventArgs e)
    {
        if (HorseBox.SelectedIndex < 0) { ErrorText.Text = "Pick the horse."; return; }
        if (!decimal.TryParse(AmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        { ErrorText.Text = "Enter the amount."; return; }
        var horse = _horses[HorseBox.SelectedIndex];
        _extras.Add(new ExtraCharge
        {
            HorseId = horse.Id, HorseLabel = horse.Name, OwnerLabel = horse.OwnerLabel,
            Category = CategoryBox.SelectedItem as string ?? "Other extra",
            Description = DescBox.Text.Trim(), Amount = amount, Date = DateTimeOffset.Now,
        });
        await _extraStore.SaveAsync(_extras);
        _extras = _extras.OrderByDescending(x => x.Date).ToList();
        ExtraList.ItemsSource = _extras;
        DescBox.Text = AmountBox.Text = string.Empty;
    }

    private async void DeleteExtraButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _extras.Remove(_selected);
        await _extraStore.SaveAsync(_extras);
        _selected = null;
        ExtraList.ItemsSource = _extras;
    }

    private async Task<(HorseOwner? Owner, List<(Horse Horse, decimal Board, List<SessionEntry> Sessions, List<ExtraCharge> Extras)> Lines, string Month)> BuildStatementAsync()
    {
        var oi = OwnerBox.SelectedIndex;
        var month = MonthBox.Text.Trim();
        if (oi < 0 || oi >= _owners.Count) return (null, new(), month);
        var owner = _owners[oi];
        var sessions = await new JsonListStore<SessionEntry>("sessions.json").LoadAsync();
        var lines = new List<(Horse, decimal, List<SessionEntry>, List<ExtraCharge>)>();
        foreach (var horse in _horses.Where(h => h.OwnerId == owner.Id))
        {
            var hs = sessions.Where(s => s.HorseId == horse.Id && s.Date.ToString("yyyy-MM") == month).OrderBy(s => s.Date).ToList();
            var he = _extras.Where(x => x.HorseId == horse.Id && x.Date.ToString("yyyy-MM") == month).OrderBy(x => x.Date).ToList();
            lines.Add((horse, horse.BoardRateMonthly, hs, he));
        }
        return (owner, lines, month);
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void StatementButton_Click(object sender, RoutedEventArgs e)
    {
        var (owner, lines, month) = await BuildStatementAsync();
        if (owner is null) { ErrorText.Text = "Pick the owner."; return; }
        var settings = await new StableSettingsStore().LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Owner statement</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}h2{font-size:15px;margin-top:18px}li{margin:3px 0}.tot{font-weight:600;font-size:16px}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.BarnName)} - Owner statement, {Esc(month)}</h1>");
        sb.AppendLine($"<p>Prepared for {Esc(owner.Name)}</p>");
        decimal grand = 0m;
        foreach (var (horse, board, sessions, extras) in lines)
        {
            var total = board + sessions.Sum(s => s.Fee) + extras.Sum(x => x.Amount);
            grand += total;
            sb.AppendLine($"<h2>{Esc(horse.Name)} (stall {Esc(horse.Stall)})</h2><ul>");
            sb.AppendLine($"<li>Board, {Esc(month)}: {board:0.00} {Esc(horse.Currency)}</li>");
            foreach (var s in sessions)
                sb.AppendLine($"<li>{s.Date:MMM d} - {Esc(s.Kind)} with {Esc(s.Trainer)}: {s.Fee:0.00}</li>");
            foreach (var x in extras)
                sb.AppendLine($"<li>{x.Date:MMM d} - {Esc(x.Category)} - {Esc(x.Description)}: {x.Amount:0.00}</li>");
            sb.AppendLine($"</ul><p class=\"tot\">Horse total: {total:0.00} {Esc(horse.Currency)}</p>");
        }
        sb.AppendLine($"<p class=\"tot\">Statement total: {grand:0.00}</p>");
        sb.AppendLine("<p>Generated by StablePilot from the barn's local ledger. Payment is made as arranged with the barn - no payment moves through this app.</p>");
        sb.AppendLine("</body></html>");

        PreviewText.Text = $"{owner.Name}, {month}: {lines.Count} horse(s) - statement total {grand:0.00}.";
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"owner-statement-{month}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Statement saved to {file.Path}";
    }
}
