using System.Globalization;
using TaxPilot.Models;
using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class EngagementsPage : Page
{
    private readonly JsonListStore<Engagement> _store = new("engagements.json");
    private readonly TaxSettingsStore _settingsStore = new();
    private List<Engagement> _items = new();
    private Engagement? _selected;

    public EngagementsPage()
    {
        this.InitializeComponent();
        StatusCombo.SelectedIndex = 0;
        PersonalPicker.Date = DateTimeOffset.Now.AddDays(30);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _items = (await _store.LoadAsync()).Where(x => x.TaxYear == settings.CurrentTaxYear).ToList();
        DeadlineCalc.ApplyDueDates(_items, settings);
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(x => x.ComputedDue ?? DateTimeOffset.MaxValue).ThenBy(x => x.ClientLabel).ToList();
    }

    private void ExtCheck_Changed(object sender, RoutedEventArgs e) { }
    private void PersonalCheck_Changed(object sender, RoutedEventArgs e)
    {
        PersonalPicker.IsEnabled = PersonalCheck.IsChecked == true;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Engagement;
        if (_selected is null) return;
        ClientText.Text = _selected.ClientLabel;
        YearBox.Text = _selected.TaxYear.ToString();
        EntityBox.Text = _selected.EntityType;
        StatusCombo.SelectedItem = _selected.Status;
        FeeBox.Text = _selected.FeeQuoted.ToString(CultureInfo.InvariantCulture);
        PaidBox.Text = _selected.FeePaid.ToString(CultureInfo.InvariantCulture);
        ExtCheck.IsChecked = _selected.Extended;
        PersonalCheck.IsChecked = _selected.PersonalDueDate is not null;
        if (_selected.PersonalDueDate is { } p) PersonalPicker.Date = p;
        NotesBox.Text = _selected.Notes;
        DueText.Text = _selected.ComputedDue is { } d
            ? $"Due: {d:MMM d, yyyy} ({_selected.DaysToDue} days){(_selected.Extended ? " - extended date" : string.Empty)}"
            : "No due date - add this entity to the deadline table (Deadlines page).";
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Status = StatusCombo.SelectedItem as string ?? _selected.Status;
        _selected.FeeQuoted = ParseMoney(FeeBox.Text);
        _selected.FeePaid = ParseMoney(PaidBox.Text);
        var wasExtended = _selected.Extended;
        _selected.Extended = ExtCheck.IsChecked == true;
        if (_selected.Extended && !wasExtended) _selected.ExtensionFiledDate = DateTimeOffset.Now;
        _selected.PersonalDueDate = PersonalCheck.IsChecked == true ? PersonalPicker.Date : null;
        _selected.Notes = NotesBox.Text.Trim();
        var all = await _store.LoadAsync();
        var idx = all.FindIndex(x => x.Id == _selected.Id);
        if (idx >= 0) all[idx] = _selected;
        await _store.SaveAsync(all);
        var settings = await _settingsStore.LoadAsync();
        _selected.ComputedDue = DeadlineCalc.ComputeDue(_selected, settings);
        RefreshList();
        ItemsList_SelectionChanged(this, null!);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var all = await _store.LoadAsync();
        all.RemoveAll(x => x.Id == _selected.Id);
        await _store.SaveAsync(all);
        _items.RemoveAll(x => x.Id == _selected.Id);
        _selected = null;
        RefreshList();
    }
}
