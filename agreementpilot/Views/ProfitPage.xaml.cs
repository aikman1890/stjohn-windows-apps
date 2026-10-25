using System.Net;
using System.Text;
using AgreementPilot.Models;
using AgreementPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace AgreementPilot.Views;

public sealed partial class ProfitPage : Page
{
    private readonly JsonListStore<Agreement> _agreementStore = new("agreements.json");
    private readonly JsonListStore<TradeCustomer> _customerStore = new("customers.json");
    private readonly JsonListStore<Equipment> _equipmentStore = new("equipment.json");
    private readonly JsonListStore<Visit> _visitStore = new("visits.json");
    private readonly TradeSettingsStore _settingsStore = new();
    private List<TradeCustomer> _customers = new();

    public ProfitPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var agreements = await _agreementStore.LoadAsync();
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.DisplayLine).ToList();

        TotalsText.Text =
            $"Book: {agreements.Count(a => a.Status != "Lapsed")} live agreement(s) - " +
            $"price on the book {agreements.Where(a => a.Status != "Lapsed").Sum(a => a.Price):C}/yr - " +
            $"discounts honored to date {agreements.Sum(a => a.DiscountsHonored):C} - " +
            $"net after discounts {agreements.Where(a => a.Status != "Lapsed").Sum(a => a.Price - a.DiscountsHonored):C}";

        PerAgreementList.ItemsSource = agreements.OrderBy(a => a.CustomerLabel).Select(a =>
            $"{a.CustomerLabel} - {a.PlanName}: price {a.Price:C} - discounts {a.DiscountsHonored:C} - net {a.Price - a.DiscountsHonored:C} - visits {a.VisitsDelivered}/{a.VisitsIncluded} delivered - {a.Status}").ToList();

        PerPlanList.ItemsSource = agreements.GroupBy(a => a.PlanName).OrderBy(g => g.Key).Select(g =>
            $"{g.Key}: {g.Count()} agreement(s) - price {g.Sum(a => a.Price):C} - discounts {g.Sum(a => a.DiscountsHonored):C} - net {g.Sum(a => a.Price - a.DiscountsHonored):C} - visits delivered {g.Sum(a => a.VisitsDelivered)} of {g.Sum(a => a.VisitsIncluded)}").ToList();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void CsvButton_Click(object sender, RoutedEventArgs e)
    {
        var agreements = await _agreementStore.LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Customer,Address,Plan,Price,Start,Renewal,VisitsIncluded,VisitsDelivered,DiscountsHonored,Status,PaymentStatus,Covered,Notes");
        string Q(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
        foreach (var a in agreements.OrderBy(a => a.CustomerLabel))
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Q(a.CustomerLabel.Split(" - ")[0]), Q(a.CustomerLabel), Q(a.PlanName),
                a.Price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                a.StartDate.ToString("yyyy-MM-dd"), a.RenewalDate.ToString("yyyy-MM-dd"),
                a.VisitsIncluded.ToString(), a.VisitsDelivered.ToString(),
                a.DiscountsHonored.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Q(a.Status), Q(a.PaymentStatus), Q(a.CoveredNote), Q(a.Notes),
            }));
        }
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("CSV file", new List<string> { ".csv" });
        picker.SuggestedFileName = "agreement-book.csv";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Agreement book exported to {file.Path}";
    }

    private async void PacketButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count)
        {
            StatusText.Text = "Pick a customer first.";
            return;
        }
        var customer = _customers[CustomerPicker.SelectedIndex];
        var settings = await _settingsStore.LoadAsync();
        var equipment = (await _equipmentStore.LoadAsync()).Where(eq => eq.CustomerId == customer.Id).ToList();
        var agreements = (await _agreementStore.LoadAsync()).Where(a => a.CustomerId == customer.Id).ToList();
        var visits = (await _visitStore.LoadAsync()).Where(v => v.CustomerId == customer.Id)
            .OrderByDescending(v => v.Date).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Renewal packet</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:22px}h2{font-size:16px;margin-top:22px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #bbb;padding:6px 8px;text-align:left;font-size:13px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.ShopName)} - Renewal packet</h1>");
        sb.AppendLine($"<p><strong>Customer:</strong> {Esc(customer.Name)}<br/><strong>Address:</strong> {Esc(customer.Address)}</p>");

        sb.AppendLine("<h2>Equipment on record</h2><table><tr><th>Unit</th><th>Brand / model</th><th>Serial</th><th>Installed</th><th>Age</th></tr>");
        foreach (var eq in equipment)
            sb.AppendLine($"<tr><td>{Esc(eq.Type)}</td><td>{Esc(eq.Brand)} {Esc(eq.Model)}</td><td>{Esc(eq.Serial)}</td><td>{(eq.InstallDate is { } d ? d.ToString("MMM d, yyyy") : "-")}</td><td>{(eq.AgeYears is { } a ? a + " years" : "unknown")}</td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("<h2>Agreement</h2>");
        foreach (var a in agreements)
            sb.AppendLine($"<p>{Esc(a.PlanName)} - {a.Price:C}/yr - status {Esc(a.Status)} - renews {a.RenewalDate:MMM d, yyyy} - visits delivered {a.VisitsDelivered} of {a.VisitsIncluded} - covers: {Esc(a.CoveredNote)}</p>");
        if (agreements.Count == 0) sb.AppendLine("<p class=\"muted\">No agreement on record.</p>");

        sb.AppendLine("<h2>Visit history</h2><table><tr><th>Date</th><th>Unit</th><th>Work</th></tr>");
        foreach (var v in visits)
            sb.AppendLine($"<tr><td>{v.Date:MMM d, yyyy}</td><td>{Esc(v.EquipmentLabel)}</td><td>{Esc(v.WorkPerformed)}</td></tr>");
        sb.AppendLine("</table>");

        var open = visits.Where(v => v.RecResponse == "Declined" && !v.FollowUpDone).ToList();
        sb.AppendLine("<h2>Open recommendations</h2>");
        if (open.Count == 0) sb.AppendLine("<p class=\"muted\">None open.</p>");
        foreach (var v in open)
            sb.AppendLine($"<p>{Esc(v.Recommendation)} - quoted {v.RecAmount:C} on {v.Date:MMM d, yyyy} ({Esc(v.EquipmentLabel)})</p>");

        sb.AppendLine($"<p class=\"muted\">Prepared {DateTime.Now:MMM d, yyyy} by {(string.IsNullOrWhiteSpace(settings.ShopName) ? "the shop" : Esc(settings.ShopName))} with AgreementPilot.</p>");
        sb.AppendLine("</body></html>");

        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"renewal-packet-{customer.Name.Replace(" ", "-")}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Renewal packet saved to {file.Path} - open it in a browser to print.";
    }
}
