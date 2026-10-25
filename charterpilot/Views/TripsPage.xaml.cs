using System.Globalization;
using System.Net;
using System.Text;
using CharterPilot.Models;
using CharterPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CharterPilot.Views;

public sealed partial class TripsPage : Page
{
    private readonly JsonListStore<TripFile> _store = new("trips.json");
    private List<TripFile> _trips = new();
    private TripFile? _selected;

    public TripsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _trips = (await _store.LoadAsync()).OrderBy(t => t.DepartAt).ToList();
        TripList.ItemsSource = _trips;
        if (_trips.Count > 0) TripList.SelectedIndex = 0;
    }

    private void TripList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = TripList.SelectedItem as TripFile;
        if (_selected is null) return;
        RouteText.Text = $"{_selected.ClientLabel} - {_selected.RouteLabel} - {_selected.OperatorLabel} / {_selected.TailLabel} - departs {_selected.DepartAt:ddd, MMM d, yyyy HH:mm}";
        AgreementCheck.IsChecked = _selected.AgreementSigned;
        OperatorCheck.IsChecked = _selected.OperatorConfirmed;
        PaxCheck.IsChecked = _selected.PassengerListReceived;
        CateringCheck.IsChecked = _selected.CateringOrdered;
        GroundCheck.IsChecked = _selected.GroundTransportBooked;
        PaymentCheck.IsChecked = _selected.PaymentReceived;
        PriceBox.Text = _selected.FinalPrice.ToString(CultureInfo.InvariantCulture);
        CostBox.Text = _selected.FinalOperatorCost.ToString(CultureInfo.InvariantCulture);
        OtherBox.Text = _selected.OtherCosts.ToString(CultureInfo.InvariantCulture);
        CurrencyBox.Text = _selected.Currency;
        NotesBox.Text = _selected.Notes;
        RefreshGate();
    }

    private void RefreshGate()
    {
        if (_selected is null) { ReadyText.Text = string.Empty; MarginText.Text = string.Empty; return; }
        ReadyText.Text = _selected.IsReady
            ? "READY: every checklist item is logged."
            : $"NOT READY - missing: {string.Join(", ", _selected.MissingItems)}.";
        MarginText.Text = $"Margin kept (computed): {_selected.MarginKept:0.00} {_selected.Currency}";
    }

    private void ReadForm(TripFile t)
    {
        t.AgreementSigned = AgreementCheck.IsChecked == true;
        t.OperatorConfirmed = OperatorCheck.IsChecked == true;
        t.PassengerListReceived = PaxCheck.IsChecked == true;
        t.CateringOrdered = CateringCheck.IsChecked == true;
        t.GroundTransportBooked = GroundCheck.IsChecked == true;
        t.PaymentReceived = PaymentCheck.IsChecked == true;
        if (decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) t.FinalPrice = p;
        if (decimal.TryParse(CostBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var c)) t.FinalOperatorCost = c;
        if (decimal.TryParse(OtherBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var o)) t.OtherCosts = o;
        t.Currency = string.IsNullOrWhiteSpace(CurrencyBox.Text) ? "USD" : CurrencyBox.Text.Trim().ToUpperInvariant();
        t.Notes = NotesBox.Text.Trim();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        ReadForm(_selected);
        await _store.SaveAsync(_trips);
        TripList.ItemsSource = null;
        TripList.ItemsSource = _trips;
        TripList.SelectedItem = _selected;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _trips.Remove(_selected);
        await _store.SaveAsync(_trips);
        _selected = null;
        TripList.ItemsSource = _trips;
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        ReadForm(_selected);
        var settings = await new CharterSettingsStore().LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Trip summary</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:20px}li{margin:3px 0}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.BrokerName)} - Trip summary</h1>");
        sb.AppendLine($"<p><strong>{Esc(_selected.ClientLabel)}</strong> - {Esc(_selected.RouteLabel)}<br/>Operator: {Esc(_selected.OperatorLabel)} / {Esc(_selected.TailLabel)} - departs {_selected.DepartAt:ddd, MMM d, yyyy HH:mm}</p>");
        sb.AppendLine($"<p>Readiness: {(_selected.IsReady ? "READY - all items logged" : "NOT READY - missing: " + Esc(string.Join(", ", _selected.MissingItems)))}</p>");
        sb.AppendLine("<ul>");
        sb.AppendLine($"<li>Signed charter agreement: {(_selected.AgreementSigned ? "yes" : "NO")}</li>");
        sb.AppendLine($"<li>Operator confirmation: {(_selected.OperatorConfirmed ? "yes" : "NO")}</li>");
        sb.AppendLine($"<li>Passenger list: {(_selected.PassengerListReceived ? "received" : "NOT received")}</li>");
        sb.AppendLine($"<li>Catering: {(_selected.CateringOrdered ? "ordered" : "NOT ordered")}</li>");
        sb.AppendLine($"<li>Ground transport: {(_selected.GroundTransportBooked ? "booked" : "NOT booked")}</li>");
        sb.AppendLine($"<li>Payment: {(_selected.PaymentReceived ? "received" : "NOT received")}</li>");
        sb.AppendLine("</ul>");
        sb.AppendLine($"<p>Final price {_selected.FinalPrice:0.00} {_selected.Currency} - operator cost {_selected.FinalOperatorCost:0.00} - other {_selected.OtherCosts:0.00} - <strong>margin kept {_selected.MarginKept:0.00} {_selected.Currency}</strong></p>");
        sb.AppendLine($"<p>{Esc(_selected.Notes)}</p>");
        sb.AppendLine("<p>Generated by CharterPilot from the broker's local records. Flight operations and safety remain the operator's responsibility.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = "trip-summary.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Summary saved to {file.Path}";
    }
}
