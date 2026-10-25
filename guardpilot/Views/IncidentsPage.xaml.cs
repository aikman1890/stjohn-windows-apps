using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class IncidentsPage : Page
{
    private readonly JsonListStore<Incident> _store = new("incidents.json");
    private readonly JsonListStore<Post> _postStore = new("posts.json");
    private readonly GuardSettingsStore _settingsStore = new();
    private List<Incident> _items = new();
    private List<Post> _posts = new();
    private Incident? _selected;
    private string _filter = string.Empty;

    public IncidentsPage()
    {
        this.InitializeComponent();
        DuePicker.Date = DateTimeOffset.Now.AddDays(2);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        TypeCombo.ItemsSource = settings.IncidentTypes;
        if (settings.IncidentTypes.Count > 0) TypeCombo.SelectedIndex = 0;
        _posts = await _postStore.LoadAsync();
        PostPicker.ItemsSource = _posts.Select(p => p.DisplayLine).ToList();
        if (_posts.Count > 0) PostPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        var view = _items.OrderByDescending(i => i.When).AsEnumerable();
        if (_filter.Length > 0)
        {
            bool Hit(string s) => s.Contains(_filter, StringComparison.OrdinalIgnoreCase);
            view = view.Where(i => Hit(i.SiteLabel) || Hit(i.PostLabel) || Hit(i.GuardLabel) || Hit(i.Type) || Hit(i.Narrative));
        }
        ItemsList.ItemsSource = view.ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Incident;
        if (_selected is null) return;
        var idx = _posts.FindIndex(p => p.DisplayLine == _selected.PostLabel);
        if (idx >= 0) PostPicker.SelectedIndex = idx;
        GuardBox.Text = _selected.GuardLabel;
        TypeCombo.Text = _selected.Type;
        NarrativeBox.Text = _selected.Narrative;
        FollowBox.Text = _selected.FollowUp;
        OwnerBox.Text = _selected.FollowUpOwner;
        if (_selected.FollowUpDue is { } d) DuePicker.Date = d;
        DoneCheck.IsChecked = _selected.FollowUpDone;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var post = PostPicker.SelectedIndex >= 0 && PostPicker.SelectedIndex < _posts.Count
            ? _posts[PostPicker.SelectedIndex] : null;
        var item = _selected ?? new Incident { When = DateTimeOffset.Now };
        item.PostLabel = post?.DisplayLine ?? string.Empty;
        item.SiteLabel = post?.SiteLabel ?? string.Empty;
        item.GuardLabel = GuardBox.Text.Trim();
        item.Type = TypeCombo.Text.Trim();
        item.Narrative = NarrativeBox.Text.Trim();
        item.FollowUp = FollowBox.Text.Trim();
        item.FollowUpOwner = OwnerBox.Text.Trim();
        item.FollowUpDue = string.IsNullOrWhiteSpace(item.FollowUp) ? null : DuePicker.Date;
        item.FollowUpDone = DoneCheck.IsChecked == true;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(i => i.Id == _selected.Id);
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
        GuardBox.Text = string.Empty;
        NarrativeBox.Text = string.Empty;
        FollowBox.Text = string.Empty;
        OwnerBox.Text = string.Empty;
        DoneCheck.IsChecked = false;
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        _filter = FilterBox.Text.Trim();
        RefreshList();
    }
}
