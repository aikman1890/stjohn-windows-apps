using System.Net;
using System.Text;
using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace RemodelPilot.Views;

public sealed partial class ChangeOrdersPage : Page
{
    private readonly JsonListStore<ChangeOrder> _store = new("changeorders.json");
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private readonly RemodelSettingsStore _settingsStore = new();
    private List<ChangeOrder> _items = new();
    private List<Job> _jobs = new();
    private ChangeOrder? _selected;

    public ChangeOrdersPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = new[] { "Draft", "Sent", "Approved", "Declined" };
        StatusCombo.SelectedIndex = 0;
        MethodCombo.ItemsSource = new[] { "In person", "Email", "Text message", "Phone", "Signed document" };
        MethodCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        CategoryCombo.ItemsSource = settings.Categories;
        if (settings.Categories.Count > 0) CategoryCombo.SelectedIndex = 0;
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => j.DisplayLine).ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(c => c.CreatedDate).ToList();
    }

    private Job? PickedJob =>
        JobPicker.SelectedIndex >= 0 && JobPicker.SelectedIndex < _jobs.Count
            ? _jobs[JobPicker.SelectedIndex]
            : null;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ChangeOrder;
        if (_selected is null) return;
        var idx = _jobs.FindIndex(j => j.Id == _selected.JobId);
        if (idx >= 0) JobPicker.SelectedIndex = idx;
        DescBox.Text = _selected.Description;
        CategoryCombo.SelectedItem = _selected.Category;
        PriceBox.Text = _selected.Price.ToString("0.##");
        StatusCombo.SelectedItem = _selected.Status;
        MethodCombo.SelectedItem = string.IsNullOrWhiteSpace(_selected.Method) ? "In person" : _selected.Method;
        ApproverBox.Text = _selected.ApprovedBy;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null) return;
        var item = _selected ?? new ChangeOrder
        {
            Number = $"CO-{_items.Count(c => c.JobId == job.Id) + 1}",
            CreatedDate = DateTimeOffset.Now,
        };
        item.JobId = job.Id;
        item.JobLabel = job.DisplayLine;
        item.Description = DescBox.Text.Trim();
        item.Category = CategoryCombo.SelectedItem as string ?? "Other";
        item.Price = decimal.TryParse(PriceBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m;
        var newStatus = StatusCombo.SelectedItem as string ?? "Draft";
        if (newStatus == "Sent" && item.SentDate is null) item.SentDate = DateTimeOffset.Now;
        if ((newStatus == "Approved" || newStatus == "Declined") && item.Status != newStatus)
        {
            item.DecisionDate = DateTimeOffset.Now;
            item.Method = MethodCombo.SelectedItem as string ?? string.Empty;
            item.ApprovedBy = ApproverBox.Text.Trim();
        }
        item.Status = newStatus;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        StatusText.Text = $"{item.Number} saved - {item.Status}. Contract impact: {(item.Status == "Approved" ? item.Price.ToString("C") : "none until approved")}.";
        RefreshList();
    }

    private async void ApproveButton_Click(object sender, RoutedEventArgs e) => await DecideAsync("Approved");
    private async void DeclineButton_Click(object sender, RoutedEventArgs e) => await DecideAsync("Declined");

    private async Task DecideAsync(string decision)
    {
        if (_selected is null) { StatusText.Text = "Select a change order first (or save the new one)."; return; }
        _selected.Status = decision;
        _selected.DecisionDate = DateTimeOffset.Now;
        _selected.Method = MethodCombo.SelectedItem as string ?? string.Empty;
        _selected.ApprovedBy = ApproverBox.Text.Trim();
        await _store.SaveAsync(_items);
        StatusCombo.SelectedItem = decision;
        StatusText.Text = $"{_selected.Number} {decision} on {DateTime.Now:MMM d, yyyy} by {_selected.ApprovedBy} ({_selected.Method})." +
            (decision == "Approved" ? " The contract price now includes it." : " It stays on file - the scope conversation, documented.");
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
        DescBox.Text = string.Empty;
        PriceBox.Text = string.Empty;
        ApproverBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        StatusText.Text = string.Empty;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || App.WindowInstance is null) return;
        var settings = await _settingsStore.LoadAsync();
        var job = _jobs.FirstOrDefault(j => j.Id == _selected.JobId);
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"change-order-{_selected.Number}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildCoHtml(_selected, job, settings));
        StatusText.Text = $"Change order exported to {file.Path}";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildCoHtml(ChangeOrder co, Job? job, RemodelSettings s)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Change Order {Enc(co.Number)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  ".sig{margin-top:48px;border-top:1px solid #222;padding-top:6px;width:60%}</style></head><body>");
        sb.Append($"<h1>{Enc(s.BusinessName)}</h1><p>{Enc(s.Address)}<br>{Enc(s.Phone)}</p>");
        sb.Append($"<h2>Change Order {Enc(co.Number)}</h2>");
        sb.Append($"<p><strong>Client:</strong> {Enc(job?.ClientName ?? "")}<br><strong>Job:</strong> {Enc(job?.Address ?? "")}<br>" +
                  $"<strong>Category:</strong> {Enc(co.Category)}<br><strong>Date:</strong> {co.CreatedDate:MMMM d, yyyy}</p>");
        sb.Append($"<p><strong>Description of change:</strong><br>{Enc(co.Description)}</p>");
        sb.Append($"<p><strong>Price:</strong> {co.Price:C} - this amount {(co.Price >= 0 ? "is added to" : "is credited against")} the contract price when approved.</p>");
        sb.Append($"<p><strong>Status:</strong> {Enc(co.Status)}" +
                  (co.DecisionDate is { } d ? $" - decided {d:MMMM d, yyyy} by {Enc(co.ApprovedBy)} ({Enc(co.Method)})" : string.Empty) + "</p>");
        sb.Append("<div class=\"sig\">Client signature &nbsp;&nbsp;&nbsp;&nbsp; Date</div>");
        sb.Append("<div class=\"sig\">Contractor signature &nbsp;&nbsp;&nbsp;&nbsp; Date</div>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
