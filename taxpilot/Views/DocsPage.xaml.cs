using TaxPilot.Models;
using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class DocsPage : Page
{
    private readonly JsonListStore<DocItem> _store = new("docs.json");
    private readonly JsonListStore<Engagement> _engStore = new("engagements.json");
    private readonly TaxSettingsStore _settingsStore = new();
    private List<DocItem> _items = new();
    private List<Engagement> _engagements = new();
    private TaxSettings _settings = new();
    private DocItem? _selected;

    public DocsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        _engagements = (await _engStore.LoadAsync()).Where(x => x.TaxYear == _settings.CurrentTaxYear).ToList();
        EngagementPicker.ItemsSource = _engagements.Select(x => $"{x.ClientLabel} ({x.EntityType})").ToList();
        _items = await _store.LoadAsync();
        if (_engagements.Count > 0) EngagementPicker.SelectedIndex = 0;
        RefreshAll();
    }

    private Engagement? Current =>
        EngagementPicker.SelectedIndex >= 0 && EngagementPicker.SelectedIndex < _engagements.Count
            ? _engagements[EngagementPicker.SelectedIndex] : null;

    private void RefreshAll()
    {
        ChecklistList.ItemsSource = Current is null
            ? null
            : _items.Where(d => d.EngagementId == Current.Id).ToList();
        var byEng = _engagements.ToDictionary(x => x.Id);
        ChaseList.ItemsSource = _items
            .Where(d => !d.Received && byEng.ContainsKey(d.EngagementId))
            .OrderBy(d => byEng[d.EngagementId].LastContacted ?? DateTimeOffset.MinValue)
            .Select(d =>
            {
                var eng = byEng[d.EngagementId];
                var last = eng.LastContacted is { } l ? $"last contacted {l:MMM d}" : "never contacted";
                var overdue = eng.LastContacted is { } lc && (DateTimeOffset.Now - lc).TotalDays > 7;
                return $"{(overdue ? "[FOLLOW-UP OVERDUE] " : string.Empty)}{d.ClientLabel} - missing: {d.Label} - {last}";
            })
            .ToList();
    }

    private void EngagementPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshAll();

    private async void TemplateButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) return;
        foreach (var template in _settings.DocTemplates)
        {
            var parts = template.Split('|');
            if (parts.Length < 2 || parts[0].Trim() != Current.EntityType) continue;
            foreach (var label in parts.Skip(1))
            {
                var trimmed = label.Trim();
                if (trimmed.Length == 0) continue;
                if (_items.Any(d => d.EngagementId == Current.Id && d.Label == trimmed)) continue;
                _items.Add(new DocItem { EngagementId = Current.Id, ClientLabel = Current.ClientLabel, Label = trimmed });
            }
        }
        await _store.SaveAsync(_items);
        RefreshAll();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null || string.IsNullOrWhiteSpace(DocBox.Text)) return;
        _items.Add(new DocItem
        {
            EngagementId = Current.Id, ClientLabel = Current.ClientLabel,
            Label = DocBox.Text.Trim(), FiledRef = RefBox.Text.Trim(),
        });
        await _store.SaveAsync(_items);
        DocBox.Text = string.Empty;
        RefreshAll();
    }

    private void ChecklistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ChecklistList.SelectedItem as DocItem;
        if (_selected is null) return;
        DocBox.Text = _selected.Label;
        RefBox.Text = _selected.FiledRef;
        ReceivedCheck.IsChecked = _selected.Received;
    }

    private async void ReceivedCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _selected.Received = ReceivedCheck.IsChecked == true;
        _selected.ReceivedDate = _selected.Received ? DateTimeOffset.Now : null;
        if (!string.IsNullOrWhiteSpace(RefBox.Text)) _selected.FiledRef = RefBox.Text.Trim();
        await _store.SaveAsync(_items);
        RefreshAll();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(d => d.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        RefreshAll();
    }
}
