using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class DarPage : Page
{
    private readonly JsonListStore<DarEntry> _store = new("dars.json");
    private readonly JsonListStore<Post> _postStore = new("posts.json");
    private List<DarEntry> _items = new();
    private List<Post> _posts = new();
    private DarEntry? _selected;

    public DarPage()
    {
        this.InitializeComponent();
        DatePickerCtl.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _posts = await _postStore.LoadAsync();
        PostPicker.ItemsSource = _posts.Select(p => p.DisplayLine).ToList();
        if (_posts.Count > 0) PostPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(d => d.Date).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as DarEntry;
        if (_selected is null) return;
        var idx = _posts.FindIndex(p => p.DisplayLine == _selected.PostLabel);
        if (idx >= 0) PostPicker.SelectedIndex = idx;
        GuardBox.Text = _selected.GuardLabel;
        DatePickerCtl.Date = _selected.Date;
        SummaryBox.Text = _selected.Summary;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var post = PostPicker.SelectedIndex >= 0 && PostPicker.SelectedIndex < _posts.Count
            ? _posts[PostPicker.SelectedIndex] : null;
        var item = _selected ?? new DarEntry();
        item.PostLabel = post?.DisplayLine ?? string.Empty;
        item.GuardLabel = GuardBox.Text.Trim();
        item.Date = DatePickerCtl.Date ?? DateTimeOffset.Now;
        item.Summary = SummaryBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(d => d.Id == _selected.Id);
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
        SummaryBox.Text = string.Empty;
    }
}
