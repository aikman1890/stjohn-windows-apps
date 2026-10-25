using BayPilot.Models;
using BayPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BayPilot.Views;

public sealed partial class ApprovalsPage : Page
{
    private readonly JsonListStore<RepairOrder> _orderStore = new("orders.json");
    private readonly JsonListStore<ApprovalEntry> _logStore = new("approvals.json");
    private List<RepairOrder> _orders = new();
    private RepairOrder? _ro;

    public ApprovalsPage()
    {
        this.InitializeComponent();
        MethodCombo.ItemsSource = new[] { "In person", "Phone" };
        MethodCombo.SelectedIndex = 0;
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _orders = await _orderStore.LoadAsync();
        RoPicker.ItemsSource = _orders.Select(o => o.DisplayLine).ToList();
        if (_orders.Count > 0) RoPicker.SelectedIndex = 0;
    }

    private async void RoPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ro = RoPicker.SelectedIndex >= 0 && RoPicker.SelectedIndex < _orders.Count
            ? _orders[RoPicker.SelectedIndex]
            : null;
        RefreshLines();
        await RefreshLogAsync();
    }

    private void RefreshLines()
    {
        LinesList.ItemsSource = null;
        LinesList.ItemsSource = _ro?.Lines;
    }

    private async Task RefreshLogAsync()
    {
        var log = await _logStore.LoadAsync();
        LogList.ItemsSource = _ro is null
            ? null
            : log.Where(a => a.RoId == _ro.Id).OrderByDescending(a => a.Date).ToList();
    }

    private async void ApproveButton_Click(object sender, RoutedEventArgs e) =>
        await DecideAsync("Approved");

    private async void DeclineButton_Click(object sender, RoutedEventArgs e) =>
        await DecideAsync("Declined");

    private async Task DecideAsync(string decision)
    {
        if (_ro is null || LinesList.SelectedItem is not RoLine line)
        {
            StatusText.Text = "Pick a repair order and a line first.";
            return;
        }
        line.Decision = decision;
        if (decision == "Declined")
        {
            line.DeclinedReason = ReasonBox.Text.Trim();
            line.FollowUpDate = FollowUpCheck.IsChecked == true ? FollowUpPicker.Date : null;
            line.FollowUpDone = false;
        }

        var orders = await _orderStore.LoadAsync();
        var stored = orders.FirstOrDefault(o => o.Id == _ro.Id);
        if (stored is not null)
        {
            var sl = stored.Lines.FirstOrDefault(l => l.Id == line.Id);
            if (sl is not null)
            {
                sl.Decision = line.Decision;
                sl.DeclinedReason = line.DeclinedReason;
                sl.FollowUpDate = line.FollowUpDate;
                sl.FollowUpDone = line.FollowUpDone;
            }
        }
        await _orderStore.SaveAsync(orders);
        _orders = orders;
        _ro = stored ?? _ro;

        var log = await _logStore.LoadAsync();
        log.Add(new ApprovalEntry
        {
            RoId = _ro.Id,
            RoLabel = _ro.DisplayLine,
            Date = DateTimeOffset.Now,
            LineDescription = line.Description,
            Decision = decision,
            Method = MethodCombo.SelectedItem as string ?? "In person",
            ApprovedBy = ApproverBox.Text.Trim(),
            Notes = decision == "Declined" ? ReasonBox.Text.Trim() : string.Empty,
        });
        await _logStore.SaveAsync(log);

        StatusText.Text = $"{decision}: {line.Description} - recorded in the approval log.";
        RefreshLines();
        await RefreshLogAsync();
    }
}
