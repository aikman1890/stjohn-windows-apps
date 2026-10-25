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

public sealed partial class DrawsPage : Page
{
    private readonly JsonListStore<Draw> _store = new("draws.json");
    private readonly JsonListStore<Job> _jobStore = new("jobs.json");
    private readonly JsonListStore<ChangeOrder> _coStore = new("changeorders.json");
    private readonly RemodelSettingsStore _settingsStore = new();
    private List<Draw> _items = new();
    private List<Job> _jobs = new();

    public DrawsPage()
    {
        this.InitializeComponent();
        DuePicker.Date = DateTimeOffset.Now.AddDays(7);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _jobs = await _jobStore.LoadAsync();
        JobPicker.ItemsSource = _jobs.Select(j => j.DisplayLine).ToList();
        if (_jobs.Count > 0) JobPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshAll();
    }

    private Job? PickedJob =>
        JobPicker.SelectedIndex >= 0 && JobPicker.SelectedIndex < _jobs.Count
            ? _jobs[JobPicker.SelectedIndex]
            : null;

    private void JobPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshAll();

    private async void RefreshAll()
    {
        OverdueList.ItemsSource = _items.Where(d => d.OverdueToInvoice).OrderBy(d => d.DueDate).ToList();
        var job = PickedJob;
        ItemsList.ItemsSource = job is null ? null : _items.Where(d => d.JobId == job.Id).OrderBy(d => d.DueDate).ToList();
        if (job is not null)
        {
            var cos = await _coStore.LoadAsync();
            var contract = JobMath.ContractPrice(job, cos);
            var paid = _items.Where(d => d.JobId == job.Id && d.PaidDate is not null).Sum(d => d.Amount);
            var invoiced = _items.Where(d => d.JobId == job.Id && d.InvoicedDate is not null && d.PaidDate is null).Sum(d => d.Amount);
            JobTotalsText.Text = $"{job.ClientName}: contract {contract:C} - paid {paid:C} - invoiced unpaid {invoiced:C} - remaining {contract - paid - invoiced:C}";
        }
        else JobTotalsText.Text = string.Empty;
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null || string.IsNullOrWhiteSpace(LabelBox.Text)) return;
        var amount = decimal.TryParse(AmountBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m;
        if (amount == 0m && decimal.TryParse(PercentBox.Text.Trim(), out var pct) && pct > 0)
        {
            var cos = await _coStore.LoadAsync();
            amount = Math.Round(JobMath.ContractPrice(job, cos) * pct / 100m, 2);
        }
        _items.Add(new Draw
        {
            JobId = job.Id,
            JobLabel = job.DisplayLine,
            Label = LabelBox.Text.Trim(),
            Amount = amount,
            TriggerNote = TriggerBox.Text.Trim(),
            DueDate = DueCheck.IsChecked == true ? DuePicker.Date : null,
        });
        await _store.SaveAsync(_items);
        LabelBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        PercentBox.Text = string.Empty;
        TriggerBox.Text = string.Empty;
        RefreshAll();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not Draw draw) return;
        _items.RemoveAll(d => d.Id == draw.Id);
        await _store.SaveAsync(_items);
        RefreshAll();
    }

    private async void InvoiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not Draw draw) return;
        draw.InvoicedDate = DateTimeOffset.Now;
        await _store.SaveAsync(_items);
        StatusText.Text = $"{draw.Label} invoiced {DateTime.Now:MMM d, yyyy}.";
        RefreshAll();
    }

    private async void PaidButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not Draw draw) return;
        if (draw.InvoicedDate is null) draw.InvoicedDate = DateTimeOffset.Now;
        draw.PaidDate = DateTimeOffset.Now;
        await _store.SaveAsync(_items);
        StatusText.Text = $"{draw.Label} marked paid {DateTime.Now:MMM d, yyyy}.";
        RefreshAll();
    }

    private async void StatementButton_Click(object sender, RoutedEventArgs e)
    {
        var job = PickedJob;
        if (job is null || App.WindowInstance is null) return;
        var settings = await _settingsStore.LoadAsync();
        var cos = (await _coStore.LoadAsync()).Where(c => c.JobId == job.Id).ToList();
        var draws = _items.Where(d => d.JobId == job.Id).OrderBy(d => d.DueDate).ToList();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"final-statement-{job.ClientName.Replace(' ', '-').ToLowerInvariant()}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildStatementHtml(job, cos, draws, settings));
        StatusText.Text = $"Final statement exported to {file.Path}";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildStatementHtml(Job job, List<ChangeOrder> cos, List<Draw> draws, RemodelSettings s)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Final Statement - {Enc(job.ClientName)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        sb.Append($"<h1>{Enc(s.BusinessName)}</h1><p>{Enc(s.Address)}<br>{Enc(s.Phone)}</p>");
        sb.Append($"<h2>Statement - {Enc(job.ClientName)}</h2><p>{Enc(job.Address)}</p>");
        sb.Append("<table><tr><th>Description</th><th class=\"r\">Amount</th></tr>");
        sb.Append($"<tr><td>Contract price (winning estimate)</td><td class=\"r\">{job.BaseContractPrice:C}</td></tr>");
        foreach (var co in cos.Where(c => c.Status == "Approved"))
            sb.Append($"<tr><td>{Enc(co.Number)} - {Enc(co.Description)}</td><td class=\"r\">{co.Price:C}</td></tr>");
        var contract = JobMath.ContractPrice(job, cos);
        sb.Append($"<tr class=\"tot\"><td class=\"r\">Total contract</td><td class=\"r\">{contract:C}</td></tr>");
        sb.Append("</table>");
        sb.Append("<h3>Draws</h3><table><tr><th>Draw</th><th class=\"r\">Amount</th><th>Status</th></tr>");
        foreach (var d in draws)
            sb.Append($"<tr><td>{Enc(d.Label)}</td><td class=\"r\">{d.Amount:C}</td><td>{Enc(d.State)}{(d.PaidDate is { } pd ? $" - {pd:MMM d, yyyy}" : "")}</td></tr>");
        var paid = draws.Where(d => d.PaidDate is not null).Sum(d => d.Amount);
        sb.Append($"<tr class=\"tot\"><td class=\"r\">Paid to date</td><td class=\"r\">{paid:C}</td><td></td></tr>");
        sb.Append($"<tr class=\"tot\"><td class=\"r\">Balance due</td><td class=\"r\">{contract - paid:C}</td><td></td></tr>");
        sb.Append("</table>");
        sb.Append($"<p>Generated {DateTime.Now:MMMM d, yyyy} by RemodelPilot from the job record.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
