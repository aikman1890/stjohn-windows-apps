using CasePilot.Models;
using CasePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CasePilot.Views;

public sealed partial class CasesPage : Page
{
    private static readonly string[] Statuses =
    {
        "Active",
        "Awaiting ruling",
        "Discovery",
        "Hearing set",
        "Closed",
        "Dismissed",
        "Settled",
        "Judgment entered",
    };

    private readonly JsonListStore<CaseItem> _store = new("cases.json");
    private List<CaseItem> _items = new();
    private CaseItem? _selected;

    public CasesPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        DeadlinePicker.Date = DateTimeOffset.Now.AddDays(30);
        HearingPicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(c => c.NextDeadline).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as CaseItem;
        if (_selected is null) return;
        CaseNameBox.Text = _selected.CaseName;
        CaseNumberBox.Text = _selected.CaseNumber;
        CourtBox.Text = _selected.Court;
        JudgeBox.Text = _selected.Judge;
        StatusCombo.SelectedItem = _selected.Status;
        DeadlineLabelBox.Text = _selected.DeadlineLabel;
        DeadlinePicker.Date = _selected.NextDeadline;
        HearingTypeBox.Text = _selected.HearingType;
        HearingCheck.IsChecked = _selected.HearingDate is not null;
        if (_selected.HearingDate is { } h) HearingPicker.Date = h;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CaseNameBox.Text)) return;
        var item = _selected ?? new CaseItem();
        item.CaseName = CaseNameBox.Text.Trim();
        item.CaseNumber = CaseNumberBox.Text.Trim();
        item.Court = CourtBox.Text.Trim();
        item.Judge = JudgeBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.DeadlineLabel = DeadlineLabelBox.Text.Trim();
        item.NextDeadline = DeadlinePicker.Date;
        item.HearingType = HearingTypeBox.Text.Trim();
        item.HearingDate = HearingCheck.IsChecked == true ? HearingPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(c => c.Id == item.Id);
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
        CaseNameBox.Text = string.Empty;
        CaseNumberBox.Text = string.Empty;
        CourtBox.Text = string.Empty;
        JudgeBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        DeadlineLabelBox.Text = "Next deadline";
        DeadlinePicker.Date = DateTimeOffset.Now.AddDays(30);
        HearingTypeBox.Text = string.Empty;
        HearingCheck.IsChecked = false;
        HearingPicker.Date = DateTimeOffset.Now.AddDays(14);
        NotesBox.Text = string.Empty;
    }
}
