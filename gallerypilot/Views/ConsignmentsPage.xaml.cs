using System.Globalization;
using System.Net;
using System.Text;
using GalleryPilot.Models;
using GalleryPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GalleryPilot.Views;

public sealed partial class ConsignmentsPage : Page
{
    private readonly JsonListStore<Consignment> _store = new("consignments.json");
    private readonly JsonListStore<Artwork> _workStore = new("artworks.json");
    private readonly JsonListStore<Sale> _saleStore = new("sales.json");
    private List<Consignment> _items = new();
    private Consignment? _selected;

    public ConsignmentsPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Artist", "Collector", "Estate" };
        StatusBox.ItemsSource = new[] { "Active", "Renewed", "Returned", "Closed" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = (await _store.LoadAsync()).OrderBy(c => c.TermEnd).ToList();
        ConsignmentList.ItemsSource = _items;
        if (_items.Count > 0) ConsignmentList.SelectedIndex = 0;
    }

    private async void ConsignmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ConsignmentList.SelectedItem as Consignment;
        if (_selected is null) return;
        NameBox.Text = _selected.ConsignorName;
        KindBox.SelectedItem = _selected.ConsignorKind;
        ContactBox.Text = _selected.Contact;
        SplitBox.Text = _selected.SplitPercent.ToString(CultureInfo.InvariantCulture);
        StartPicker.Date = _selected.StartDate;
        EndPicker.Date = _selected.TermEnd;
        StatusBox.SelectedItem = _selected.Status;
        TermsBox.Text = _selected.TermsNote;
        var works = await _workStore.LoadAsync();
        var covered = works.Where(w => w.ConsignmentId == _selected.Id).ToList();
        CoverText.Text = $"Works covered: {covered.Count} ({covered.Count(w => w.Status == "Available")} available, {covered.Count(w => w.Status == "Sold")} sold, {covered.Count(w => w.Status == "Reserved")} reserved).";
    }

    private void ReadForm(Consignment c)
    {
        c.ConsignorName = NameBox.Text.Trim();
        c.ConsignorKind = KindBox.SelectedItem as string ?? c.ConsignorKind;
        c.Contact = ContactBox.Text.Trim();
        if (decimal.TryParse(SplitBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var s)) c.SplitPercent = s;
        c.StartDate = StartPicker.Date ?? c.StartDate;
        c.TermEnd = EndPicker.Date ?? c.TermEnd;
        c.Status = StatusBox.SelectedItem as string ?? c.Status;
        c.TermsNote = TermsBox.Text.Trim();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A consignor is required."; return; }
        var c = _selected ?? new Consignment();
        ReadForm(c);
        if (_selected is null) _items.Add(c);
        await _store.SaveAsync(_items);
        _items = _items.OrderBy(x => x.TermEnd).ToList();
        ConsignmentList.ItemsSource = _items;
        ConsignmentList.SelectedItem = _items.FirstOrDefault(x => x.Id == c.Id);
    }

    private async void RenewButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.TermEnd = _selected.TermEnd.AddMonths(6);
        _selected.Status = "Active";
        await _store.SaveAsync(_items);
        ConsignmentList.ItemsSource = null;
        ConsignmentList.ItemsSource = _items;
        ConsignmentList.SelectedItem = _selected;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.Remove(_selected);
        await _store.SaveAsync(_items);
        _selected = null;
        ConsignmentList.ItemsSource = _items;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ConsignmentList.SelectedItem = null;
        NameBox.Text = ContactBox.Text = TermsBox.Text = string.Empty;
        SplitBox.Text = "50";
        KindBox.SelectedItem = "Artist";
        StatusBox.SelectedItem = "Active";
        StartPicker.Date = DateTimeOffset.Now;
        EndPicker.Date = DateTimeOffset.Now.AddMonths(6);
        CoverText.Text = string.Empty;
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void StatementButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the consignment first."; return; }
        var settings = await new GallerySettingsStore().LoadAsync();
        var works = await _workStore.LoadAsync();
        var covered = works.Where(w => w.ConsignmentId == _selected.Id).ToList();
        var sales = (await _saleStore.LoadAsync()).Where(s => s.ArtistLabel == _selected.ConsignorName).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Consignment statement</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:20px}li{margin:3px 0}.tot{font-weight:600}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.GalleryName)} - Consignment statement</h1>");
        sb.AppendLine($"<p>For {Esc(_selected.ConsignorName)} - split {_selected.SplitPercent:0}% - term {_selected.StartDate:MMM d, yyyy} to {_selected.TermEnd:MMM d, yyyy} ({Esc(_selected.Status)})</p>");
        sb.AppendLine("<h2>Works held</h2><ul>");
        foreach (var w in covered)
            sb.AppendLine($"<li>{Esc(w.Artist)} - \"{Esc(w.Title)}\" - {Esc(w.Status)} @ {Esc(w.Location)} - asking {w.AskingPrice:0.00} {Esc(w.Currency)}</li>");
        sb.AppendLine("</ul><h2>Sales &amp; amounts owed</h2><ul>");
        foreach (var s in sales)
            sb.AppendLine($"<li>{s.Date:MMM d, yyyy} - {Esc(s.ArtworkLabel)} - sold {s.SalePrice:0.00} {Esc(s.Currency)} - owed to consignor {s.ArtistOwed:0.00} - {(s.IsPaid ? $"paid {s.PaidDate:MMM d, yyyy}" : $"UNPAID, due {s.DueDate:MMM d, yyyy}")}</li>");
        sb.AppendLine("</ul>");
        foreach (var g in sales.Where(s => !s.IsPaid).GroupBy(s => s.Currency))
            sb.AppendLine($"<p class=\"tot\">Total currently owed ({Esc(g.Key)}): {g.Sum(s => s.ArtistOwed):0.00}</p>");
        sb.AppendLine("<p>Generated by GalleryPilot from the gallery's local records.</p></body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = "consignment-statement.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Statement saved to {file.Path}";
    }
}
