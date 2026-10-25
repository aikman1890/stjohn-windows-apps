using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EstatePilot.Views;

public sealed partial class ChecklistsPage : Page
{
    private readonly JsonListStore<ChecklistRun> _store = new("checklistruns.json");
    private readonly JsonListStore<Property> _propertyStore = new("properties.json");
    private readonly EstateSettingsStore _settingsStore = new();
    private List<Property> _properties = new();
    private List<(string Name, List<string> Items)> _templates = new();
    private readonly List<CheckBox> _boxes = new();
    private readonly List<string> _currentItems = new();

    public ChecklistsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _properties = (await _propertyStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PropertyBox.ItemsSource = _properties.Select(p => p.Name).ToList();
        var settings = await _settingsStore.LoadAsync();
        _templates = settings.ChecklistTemplates.Select(t =>
        {
            var parts = t.Split('|');
            var items = parts.Length > 1
                ? parts[1].Split(';').Select(x => x.Trim()).Where(x => x.Length > 0).ToList()
                : new List<string>();
            return (parts[0], items);
        }).ToList();
        TemplateBox.ItemsSource = _templates.Select(t => t.Name).ToList();
        RunList.ItemsSource = (await _store.LoadAsync()).OrderByDescending(r => r.Date).ToList();
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        ItemsPanel.Children.Clear();
        _boxes.Clear();
        _currentItems.Clear();
        var ti = TemplateBox.SelectedIndex;
        if (ti < 0 || ti >= _templates.Count) { ErrorText.Text = "Pick a checklist."; return; }
        foreach (var item in _templates[ti].Items)
        {
            var box = new CheckBox { Content = item };
            _boxes.Add(box);
            _currentItems.Add(item);
            ItemsPanel.Children.Add(box);
        }
    }

    private async void SaveRunButton_Click(object sender, RoutedEventArgs e)
    {
        if (PropertyBox.SelectedIndex < 0) { ErrorText.Text = "Pick the property."; return; }
        if (_boxes.Count == 0) { ErrorText.Text = "Load a checklist first."; return; }
        var p = _properties[PropertyBox.SelectedIndex];
        var missed = _currentItems.Where((item, i) => _boxes[i].IsChecked != true).ToList();
        var runs = await _store.LoadAsync();
        runs.Add(new ChecklistRun
        {
            PropertyId = p.Id, PropertyLabel = p.Name,
            ChecklistName = _templates[TemplateBox.SelectedIndex].Name,
            Date = DateTimeOffset.Now,
            DoneCount = _boxes.Count(b => b.IsChecked == true),
            TotalCount = _boxes.Count,
            MissedItems = string.Join("; ", missed),
            Note = RunNoteBox.Text.Trim(),
        });
        await _store.SaveAsync(runs);
        RunList.ItemsSource = runs.OrderByDescending(r => r.Date).ToList();
        ErrorText.Text = "Run saved with today's date.";
    }
}
