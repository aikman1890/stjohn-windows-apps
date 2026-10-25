using System.Net;
using System.Text;
using DenialPilot.Models;
using DenialPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace DenialPilot.Views;

public sealed partial class LedgerPage : Page
{
    private static readonly string[] Kinds =
    {
        "Letter sent",
        "Call",
        "Insurer response",
        "Deadline met",
        "Deadline missed",
        "Document added",
        "Note",
    };

    private static readonly string[] Methods =
    {
        "Fax",
        "Certified mail",
        "Portal upload",
        "Email",
        "Regular mail",
        "Phone",
        "In person",
        "N/A",
    };

    private readonly JsonListStore<LedgerEntry> _store = new("ledger.json");
    private readonly JsonListStore<DenialCase> _caseStore = new("cases.json");
    private readonly JsonListStore<CallLogEntry> _callStore = new("calls.json");
    private readonly JsonListStore<EvidenceItem> _checklistStore = new("checklist.json");
    private List<LedgerEntry> _items = new();
    private List<DenialCase> _cases = new();
    private LedgerEntry? _selected;

    public LedgerPage()
    {
        this.InitializeComponent();
        KindCombo.ItemsSource = Kinds;
        KindCombo.SelectedIndex = 0;
        MethodCombo.ItemsSource = Methods;
        MethodCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cases = await _caseStore.LoadAsync();
        var filters = new List<string> { "All cases" };
        filters.AddRange(_cases.Select(c => c.DisplayLine));
        FilterPicker.ItemsSource = filters;
        FilterPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    // Filter index 0 = all cases; index i>0 = _cases[i-1].
    private DenialCase? FilterCase =>
        FilterPicker.SelectedIndex > 0 && FilterPicker.SelectedIndex - 1 < _cases.Count
            ? _cases[FilterPicker.SelectedIndex - 1]
            : null;

    private void FilterPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

    private void RefreshList()
    {
        var fc = FilterCase;
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items
            .Where(i => fc is null || i.CaseId == fc.Id)
            .OrderBy(i => i.Date)
            .ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as LedgerEntry;
        if (_selected is null) return;
        KindCombo.SelectedItem = _selected.Kind;
        DetailBox.Text = _selected.Detail;
        MethodCombo.SelectedItem = _selected.Method;
        TrackingBox.Text = _selected.Tracking;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var fc = FilterCase;
        if (fc is null)
        {
            StatusText.Text = "Pick a specific case above to add an entry.";
            return;
        }
        var item = _selected ?? new LedgerEntry();
        item.CaseId = fc.Id;
        item.CaseLabel = fc.DisplayLine;
        item.Date = DateTimeOffset.Now;
        item.Kind = KindCombo.SelectedItem as string ?? Kinds[0];
        item.Detail = DetailBox.Text.Trim();
        item.Method = MethodCombo.SelectedItem as string ?? Methods[0];
        item.Tracking = TrackingBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = null;
        DetailBox.Text = string.Empty;
        TrackingBox.Text = string.Empty;
        RefreshList();
        StatusText.Text = "Entry added to the ledger.";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(i => i.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        RefreshList();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var fc = FilterCase;
        if (fc is null)
        {
            StatusText.Text = "Pick a specific case above to export its packet.";
            return;
        }
        if (App.WindowInstance is null) return;

        var calls = (await _callStore.LoadAsync()).Where(c => c.CaseId == fc.Id).OrderBy(c => c.CallDate).ToList();
        var checklist = (await _checklistStore.LoadAsync()).Where(i => i.CaseId == fc.Id).ToList();
        var ledger = _items.Where(i => i.CaseId == fc.Id).OrderBy(i => i.Date).ToList();

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"case-packet-{fc.ClaimNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BuildPacketHtml(fc, calls, checklist, ledger));
        StatusText.Text = $"Case packet exported to {file.Path} - open it in a browser to print or save as PDF.";
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private static string BuildPacketHtml(DenialCase c, List<CallLogEntry> calls, List<EvidenceItem> checklist, List<LedgerEntry> ledger)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Case Packet - {Enc(c.ClaimNumber)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:8px}" +
                  "th,td{border:1px solid #bbb;padding:6px;text-align:left;vertical-align:top}" +
                  "th{background:#f0f0f0}</style></head><body>");
        sb.Append("<h1>Denial Case Packet</h1>");
        sb.Append($"<p><strong>Patient:</strong> {Enc(c.PatientName)}<br>");
        sb.Append($"<strong>Insurer:</strong> {Enc(c.Insurer)} ({Enc(c.Plan)})<br>");
        sb.Append($"<strong>Claim:</strong> {Enc(c.ClaimNumber)} &middot; <strong>Provider:</strong> {Enc(c.Provider)}<br>");
        sb.Append($"<strong>Date of service:</strong> {c.ServiceDate:MMMM d, yyyy} &middot; <strong>Denied:</strong> {c.DenialDate:MMMM d, yyyy}<br>");
        sb.Append($"<strong>Amount denied:</strong> {Enc(c.AmountDenied)} &middot; <strong>Reason:</strong> {Enc(c.ReasonCategory)} &middot; <strong>Status:</strong> {Enc(c.Status)}</p>");
        sb.Append($"<p><strong>Appeal clock:</strong> {Enc(c.AppealText)}<br><strong>Response clock:</strong> {Enc(c.ResponseText)}</p>");

        sb.Append("<h2>Evidence checklist</h2>");
        if (checklist.Count == 0) sb.Append("<p>No checklist items recorded.</p>");
        else
        {
            sb.Append("<table><tr><th>Item</th><th>Gathered</th></tr>");
            foreach (var i in checklist)
                sb.Append($"<tr><td>{Enc(i.ItemName)}</td><td>{(i.IsDone ? "Yes" : "No")}</td></tr>");
            sb.Append("</table>");
        }

        sb.Append("<h2>Call log</h2>");
        if (calls.Count == 0) sb.Append("<p>No calls recorded.</p>");
        else
        {
            sb.Append("<table><tr><th>When</th><th>Representative</th><th>Reference</th><th>Promised</th><th>Follow-up</th></tr>");
            foreach (var call in calls)
                sb.Append($"<tr><td>{call.CallDate:MMM d, yyyy h:mm tt}</td><td>{Enc(call.RepName)}</td><td>{Enc(call.ReferenceNumber)}</td><td>{Enc(call.Promised)}</td><td>{Enc(call.FollowUpText)}</td></tr>");
            sb.Append("</table>");
        }

        sb.Append("<h2>Proof ledger</h2>");
        if (ledger.Count == 0) sb.Append("<p>No ledger entries recorded.</p>");
        else
        {
            sb.Append("<table><tr><th>Date</th><th>Kind</th><th>Detail</th><th>Method</th><th>Tracking</th></tr>");
            foreach (var e in ledger)
                sb.Append($"<tr><td>{e.Date:MMM d, yyyy}</td><td>{Enc(e.Kind)}</td><td>{Enc(e.Detail)}</td><td>{Enc(e.Method)}</td><td>{Enc(e.Tracking)}</td></tr>");
            sb.Append("</table>");
        }

        if (!string.IsNullOrWhiteSpace(c.Notes))
            sb.Append($"<h2>Notes</h2><p>{Enc(c.Notes)}</p>");
        sb.Append("<p>Generated by DenialPilot. General information only - not medical or legal advice.</p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
