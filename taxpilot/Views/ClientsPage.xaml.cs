using System.Net;
using System.Text;
using TaxPilot.Models;
using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace TaxPilot.Views;

public sealed partial class ClientsPage : Page
{
    private readonly JsonListStore<TaxClient> _store = new("clients.json");
    private readonly JsonListStore<Engagement> _engStore = new("engagements.json");
    private readonly JsonListStore<DocItem> _docStore = new("docs.json");
    private readonly TaxSettingsStore _settingsStore = new();
    private List<TaxClient> _items = new();
    private TaxClient? _selected;

    public ClientsPage()
    {
        this.InitializeComponent();
        KindCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        EntityCombo.ItemsSource = settings.EntityTypes;
        if (settings.EntityTypes.Count > 0) EntityCombo.SelectedIndex = 0;
        RollYearBox.Text = (settings.CurrentTaxYear + 1).ToString();
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(c => c.Name).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as TaxClient;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        KindCombo.SelectedItem = _selected.Kind;
        EntityCombo.SelectedItem = _selected.EntityType;
        ContactBox.Text = _selected.Contact;
        EmailBox.Text = _selected.Email;
        PhoneBox.Text = _selected.Phone;
        CarryBox.Text = _selected.CarryNotes;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new TaxClient();
        item.Name = NameBox.Text.Trim();
        item.Kind = KindCombo.SelectedItem as string ?? "Individual";
        item.EntityType = EntityCombo.SelectedItem as string ?? "1040";
        item.Contact = ContactBox.Text.Trim();
        item.Email = EmailBox.Text.Trim();
        item.Phone = PhoneBox.Text.Trim();
        item.CarryNotes = CarryBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(c => c.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        NameBox.Text = string.Empty;
        ContactBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        PhoneBox.Text = string.Empty;
        CarryBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        StatusText.Text = string.Empty;
    }

    private async void EngageButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Select a client first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var engagements = await _engStore.LoadAsync();
        if (engagements.Any(x => x.ClientId == _selected.Id && x.TaxYear == settings.CurrentTaxYear))
        {
            StatusText.Text = $"{_selected.Name} already has a TY{settings.CurrentTaxYear} engagement.";
            return;
        }
        engagements.Add(new Engagement
        {
            ClientId = _selected.Id, ClientLabel = _selected.Name,
            TaxYear = settings.CurrentTaxYear, EntityType = _selected.EntityType,
            Status = "Not started",
            Notes = _selected.CarryNotes,
        });
        await _engStore.SaveAsync(engagements);
        StatusText.Text = $"TY{settings.CurrentTaxYear} engagement opened for {_selected.Name}.";
    }

    private async void RollButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RollYearBox.Text, out var targetYear)) return;
        var engagements = await _engStore.LoadAsync();
        var added = 0;
        foreach (var client in _items)
        {
            if (engagements.Any(x => x.ClientId == client.Id && x.TaxYear == targetYear)) continue;
            engagements.Add(new Engagement
            {
                ClientId = client.Id, ClientLabel = client.Name,
                TaxYear = targetYear, EntityType = client.EntityType,
                Status = "Not started", Notes = client.CarryNotes,
            });
            added++;
        }
        await _engStore.SaveAsync(engagements);
        var settings = await _settingsStore.LoadAsync();
        settings.CurrentTaxYear = targetYear;
        await _settingsStore.SaveAsync(settings);
        StatusText.Text = $"Rolled {added} client(s) into tax year {targetYear} - now the current season. Carry-forward notes came along; statuses are reset.";
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var engagements = (await _engStore.LoadAsync()).Where(x => x.TaxYear == settings.CurrentTaxYear).ToList();
        DeadlineCalc.ApplyDueDates(engagements, settings);
        var docs = await _docStore.LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Season packet</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #bbb;padding:5px 8px;font-size:13px;text-align:left}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.OfficeName)} - Season packet, tax year {settings.CurrentTaxYear}</h1>");
        sb.AppendLine($"<p>Filed/accepted: {engagements.Count(x => x.Status is "Accepted" or "Done")} of {engagements.Count} - fees quoted {engagements.Sum(x => x.FeeQuoted):C}, collected {engagements.Sum(x => x.FeePaid):C}, outstanding {engagements.Sum(x => x.FeeOwed):C}.</p>");
        sb.AppendLine("<table><tr><th>Client</th><th>Return</th><th>Status</th><th>Due</th><th>Docs missing</th><th>Fee owed</th></tr>");
        foreach (var x in engagements.OrderBy(x => x.ClientLabel))
        {
            var missing = docs.Count(d => d.EngagementId == x.Id && !d.Received);
            sb.AppendLine($"<tr><td>{Esc(x.ClientLabel)}</td><td>{Esc(x.EntityType)}</td><td>{Esc(x.Status)}</td><td>{(x.ComputedDue is { } d ? d.ToString("MMM d, yyyy") : "-")}</td><td>{missing}</td><td>{x.FeeOwed:C}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine("<p class=\"muted\">Produced by TaxPilot from the office's own records. Deadline dates come from the office's confirmed tables.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"season-packet-ty{settings.CurrentTaxYear}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Season packet saved to {file.Path}";
    }
}
