using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class DeclinedPage : Page
{
    private sealed record DeclinedRow(RepairOrder Ro, RoLine Line, string Text, int Urgency);

    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private List<DeclinedRow> _rows = new();

    public DeclinedPage()
    {
        this.InitializeComponent();
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(7);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var orders = await _orderStore.LoadAsync();
        _rows = new List<DeclinedRow>();
        foreach (var ro in orders)
        {
            foreach (var line in ro.Lines.Where(l => l.Decision == "Declined"))
            {
                var urgency = line.FollowUpDone ? 3
                    : line.FollowUpDate is { } f && f.Date < DateTimeOffset.Now.Date ? 0
                    : line.FollowUpDate is not null ? 1
                    : 2;
                var fu = line.FollowUpDone ? "followed up"
                    : line.FollowUpDate is { } f2
                        ? (f2.Date < DateTimeOffset.Now.Date ? $"follow-up OVERDUE - was {f2:MMM d, yyyy}" : $"follow up by {f2:MMM d, yyyy}")
                        : "no follow-up set";
                var text = $"{line.Description} - {line.Amount:C} - {ro.VehicleLabel} ({ro.CustomerLabel}) - declined {ro.OpenedDate:MMM d, yyyy}" +
                           (string.IsNullOrWhiteSpace(line.DeclinedReason) ? "" : $" - {line.DeclinedReason}") +
                           $" - {fu}";
                _rows.Add(new DeclinedRow(ro, line, text, urgency));
            }
        }
        _rows = _rows.OrderBy(r => r.Urgency).ThenByDescending(r => r.Line.Amount).ToList();
        ItemsList.ItemsSource = _rows.Select(r => r.Text).ToList();
        TotalText.Text = $"{_rows.Count} declined item(s) on the register - {_rows.Where(r => !r.Line.FollowUpDone).Sum(r => r.Line.Amount):C} of work waiting to be called back.";
    }

    private DeclinedRow? PickedRow =>
        ItemsList.SelectedIndex >= 0 && ItemsList.SelectedIndex < _rows.Count
            ? _rows[ItemsList.SelectedIndex]
            : null;

    private async void SetFollowUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (PickedRow is not { } row) { StatusText.Text = "Select a declined item first."; return; }
        await UpdateLineAsync(row, l =>
        {
            l.FollowUpDate = FollowUpPicker.Date;
            l.FollowUpDone = false;
        });
        StatusText.Text = $"Follow-up set for {row.Line.Description}.";
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (PickedRow is not { } row) { StatusText.Text = "Select a declined item first."; return; }
        await UpdateLineAsync(row, l => l.FollowUpDone = true);
        StatusText.Text = $"Marked followed up: {row.Line.Description}.";
    }

    private async Task UpdateLineAsync(DeclinedRow row, Action<RoLine> apply)
    {
        var orders = await _orderStore.LoadAsync();
        var stored = orders.FirstOrDefault(o => o.Id == row.Ro.Id);
        var sl = stored?.Lines.FirstOrDefault(l => l.Id == row.Line.Id);
        if (sl is not null)
        {
            apply(sl);
            await _orderStore.SaveAsync(orders);
        }
        await LoadAsync();
    }
}
