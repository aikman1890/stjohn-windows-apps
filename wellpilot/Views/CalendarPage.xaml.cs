using WellPilot.Models;
using WellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WellPilot.Views;

public sealed partial class CalendarPage : Page
{
    private readonly JsonListStore<ComplianceDate> _store = new("calendar.json");
    private List<ComplianceDate> _items = new();
    private ComplianceDate? _selected;

    public CalendarPage()
    {
        this.InitializeComponent();
        DuePicker.Date = DateTimeOffset.Now.AddDays(14);
        RecurCombo.SelectedIndex = 1;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(c => c.DueDate).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ComplianceDate;
        if (_selected is null) return;
        TitleBox.Text = _selected.Title;
        CategoryBox.Text = _selected.Category;
        DuePicker.Date = _selected.DueDate;
        RecurCombo.SelectedItem = _selected.Recurrence;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new ComplianceDate();
        item.Title = TitleBox.Text.Trim();
        item.Category = CategoryBox.Text.Trim();
        item.DueDate = DuePicker.Date;
        item.Recurrence = RecurCombo.SelectedItem as string ?? "Monthly";
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        if (_selected.Recurrence == "One-time")
        {
            _items.RemoveAll(c => c.Id == _selected.Id);
            _selected = null;
        }
        else
        {
            _selected.DueDate = _selected.Recurrence switch
            {
                "Monthly" => _selected.DueDate.AddMonths(1),
                "Quarterly" => _selected.DueDate.AddMonths(3),
                "Annual" => _selected.DueDate.AddYears(1),
                _ => _selected.DueDate,
            };
            while (_selected.DueDate.Date < DateTimeOffset.Now.Date)
            {
                _selected.DueDate = _selected.Recurrence switch
                {
                    "Monthly" => _selected.DueDate.AddMonths(1),
                    "Quarterly" => _selected.DueDate.AddMonths(3),
                    "Annual" => _selected.DueDate.AddYears(1),
                    _ => _selected.DueDate.AddDays(1),
                };
            }
        }
        await _store.SaveAsync(_items);
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
        TitleBox.Text = string.Empty;
        CategoryBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
