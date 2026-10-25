using System.Globalization;
using EntityPilot.Models;
using EntityPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EntityPilot.Views;

public sealed partial class DeadlinesPage : Page
{
    private readonly JsonListStore<Obligation> _store = new("obligations.json");
    private readonly JsonListStore<EntityRecord> _entityStore = new("entities.json");
    private readonly EntitySettingsStore _settingsStore = new();
    private List<Obligation> _items = new();
    private List<EntityRecord> _entities = new();
    private Obligation? _selected;

    public DeadlinesPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Annual report", "Franchise tax", "Agent renewal", "DBA renewal", "One-off", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _entities = (await _entityStore.LoadAsync()).OrderBy(x => x.LegalName).ToList();
        EntityBox.ItemsSource = _entities.Select(x => x.LegalName).ToList();
        var settings = await _settingsStore.LoadAsync();
        TemplateList.ItemsSource = settings.StateTemplates.Select(t =>
        {
            var parts = t.Split('|');
            return parts.Length >= 4
                ? $"{parts[0]} - {parts[1]}: {parts[2]} (template reviewed {parts[3]})"
                : t;
        }).ToList();
        _items = (await _store.LoadAsync()).OrderBy(o => o.DueDate).ToList();
        ObligationList.ItemsSource = _items;
        if (_items.Count > 0) ObligationList.SelectedIndex = 0;
    }

    private void ObligationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ObligationList.SelectedItem as Obligation;
        if (_selected is null) return;
        EntityBox.SelectedItem = _selected.EntityLabel;
        KindBox.SelectedItem = _selected.Kind;
        TitleBox.Text = _selected.Title;
        DuePicker.Date = _selected.DueDate;
        RecurCheck.IsChecked = _selected.RecursYearly;
        NoteBox.Text = _selected.Note;
        ErrorText.Text = string.Empty;
    }

    private void ReadForm(Obligation o)
    {
        var idx = EntityBox.SelectedIndex;
        if (idx >= 0 && idx < _entities.Count) { o.EntityId = _entities[idx].Id; o.EntityLabel = _entities[idx].LegalName; }
        o.Kind = KindBox.SelectedItem as string ?? o.Kind;
        o.Title = TitleBox.Text.Trim();
        o.DueDate = DuePicker.Date ?? o.DueDate;
        o.RecursYearly = RecurCheck.IsChecked == true;
        o.Note = NoteBox.Text.Trim();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "A title is required."; return; }
        if (EntityBox.SelectedIndex < 0) { ErrorText.Text = "Pick the entity."; return; }
        var o = _selected ?? new Obligation();
        ReadForm(o);
        if (_selected is null) _items.Add(o);
        await _store.SaveAsync(_items);
        _items = _items.OrderBy(x => x.DueDate).ToList();
        ObligationList.ItemsSource = _items;
        ObligationList.SelectedItem = _items.FirstOrDefault(x => x.Id == o.Id);
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.LastDoneDate = DateTimeOffset.Now;
        if (_selected.RecursYearly)
            _selected.DueDate = _selected.DueDate.AddYears(1);
        else
            _items.Remove(_selected);
        await _store.SaveAsync(_items);
        _items = _items.OrderBy(x => x.DueDate).ToList();
        ObligationList.ItemsSource = _items;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.Remove(_selected);
        await _store.SaveAsync(_items);
        _selected = null;
        ObligationList.ItemsSource = _items;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ObligationList.SelectedItem = null;
        TitleBox.Text = NoteBox.Text = string.Empty;
        DuePicker.Date = DateTimeOffset.Now.AddDays(30);
        RecurCheck.IsChecked = true;
    }
}
