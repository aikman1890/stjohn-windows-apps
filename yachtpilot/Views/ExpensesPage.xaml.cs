using System.Globalization;
using System.Net;
using System.Text;
using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace YachtPilot.Views;

public sealed partial class ExpensesPage : Page
{
    private readonly JsonListStore<Expense> _store = new("expenses.json");
    private readonly JsonListStore<Vessel> _vesselStore = new("vessels.json");
    private List<Expense> _expenses = new();
    private List<Vessel> _vessels = new();
    private Expense? _selected;

    public ExpensesPage()
    {
        this.InitializeComponent();
        CategoryBox.ItemsSource = new[] { "Berth", "Crew", "Maintenance", "Provisions", "Fuel", "Management fee", "Insurance", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vessels = (await _vesselStore.LoadAsync()).OrderBy(v => v.Name).ToList();
        VesselBox.ItemsSource = _vessels.Select(v => v.Name).ToList();
        StmtVesselBox.ItemsSource = _vessels.Select(v => v.Name).ToList();
        StmtMonthBox.Text = DateTimeOffset.Now.ToString("yyyy-MM");
        _expenses = (await _store.LoadAsync()).OrderByDescending(x => x.Date).ToList();
        ExpenseList.ItemsSource = _expenses;
    }

    private void ExpenseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ExpenseList.SelectedItem as Expense;
        if (_selected is null) return;
        VesselBox.SelectedItem = _selected.VesselLabel;
        CategoryBox.SelectedItem = _selected.Category;
        DatePicker.Date = _selected.Date;
        NoteBox.Text = _selected.Note;
        AmountBox.Text = _selected.Amount.ToString(CultureInfo.InvariantCulture);
        CurrencyBox.Text = _selected.Currency;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (VesselBox.SelectedIndex < 0) { ErrorText.Text = "Pick the vessel."; return; }
        if (!decimal.TryParse(AmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        { ErrorText.Text = "Enter the amount."; return; }
        var x = _selected ?? new Expense();
        var v = _vessels[VesselBox.SelectedIndex];
        x.VesselId = v.Id; x.VesselLabel = v.Name;
        x.Category = CategoryBox.SelectedItem as string ?? x.Category;
        x.Date = DatePicker.Date ?? x.Date;
        x.Note = NoteBox.Text.Trim();
        x.Amount = amount;
        x.Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        if (_selected is null) _expenses.Add(x);
        await _store.SaveAsync(_expenses);
        _expenses = _expenses.OrderByDescending(en => en.Date).ToList();
        ExpenseList.ItemsSource = _expenses;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _expenses.Remove(_selected);
        await _store.SaveAsync(_expenses);
        _selected = null;
        ExpenseList.ItemsSource = _expenses;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ExpenseList.SelectedItem = null;
        AmountBox.Text = NoteBox.Text = string.Empty;
        CurrencyBox.Text = "USD";
        DatePicker.Date = DateTimeOffset.Now;
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void StmtButton_Click(object sender, RoutedEventArgs e)
    {
        var vi = StmtVesselBox.SelectedIndex;
        if (vi < 0 || vi >= _vessels.Count) { ErrorText.Text = "Pick the vessel for the statement."; return; }
        var vessel = _vessels[vi];
        var month = StmtMonthBox.Text.Trim();
        var settings = await new YachtSettingsStore().LoadAsync();
        var rows = _expenses.Where(x => x.VesselId == vessel.Id && x.Date.ToString("yyyy-MM") == month)
            .OrderBy(x => x.Date).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Owner statement</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}table{border-collapse:collapse;width:100%}td,th{border-bottom:1px solid #ddd;padding:6px 8px;text-align:left}.tot{font-weight:600}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.CompanyName)} - Owner statement: {Esc(vessel.Name)}</h1>");
        sb.AppendLine($"<p>Prepared for {Esc(vessel.OwnerName)} - month {Esc(month)}</p>");
        sb.AppendLine("<table><tr><th>Date</th><th>Category</th><th>Note</th><th>Amount</th></tr>");
        foreach (var x in rows)
            sb.AppendLine($"<tr><td>{x.Date:MMM d, yyyy}</td><td>{Esc(x.Category)}</td><td>{Esc(x.Note)}</td><td>{x.Amount:0.00} {Esc(x.Currency)}</td></tr>");
        sb.AppendLine("</table>");
        foreach (var g in rows.GroupBy(x => x.Currency))
            sb.AppendLine($"<p class=\"tot\">Total {Esc(g.Key)}: {g.Sum(x => x.Amount):0.00}</p>");
        foreach (var g in rows.GroupBy(x => x.Category))
            sb.AppendLine($"<p>{Esc(g.Key)}: {string.Join(", ", g.GroupBy(x => x.Currency).Select(c => $"{c.Sum(x => x.Amount):0.00} {c.Key}"))}</p>");
        sb.AppendLine("<p>Generated by YachtPilot from the manager's local expense ledger. Records only - no payment has moved through this app.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"owner-statement-{month}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());

        var activity = await new JsonListStore<ActivityEntry>("activity.json").LoadAsync();
        activity.Add(new ActivityEntry
        {
            VesselId = vessel.Id, VesselLabel = vessel.Name, Kind = "Statement sent",
            Detail = $"Owner statement for {month} exported", MethodNote = $"File: {file.Name}", Date = DateTimeOffset.Now,
        });
        await new JsonListStore<ActivityEntry>("activity.json").SaveAsync(activity);
        StatusText.Text = $"Statement saved to {file.Path} and logged in the proof ledger.";
    }
}
