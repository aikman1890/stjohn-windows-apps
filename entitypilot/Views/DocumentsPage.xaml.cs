using EntityPilot.Models;
using EntityPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace EntityPilot.Views;

public sealed partial class DocumentsPage : Page
{
    private readonly JsonListStore<DocRef> _store = new("documents.json");
    private readonly JsonListStore<EntityRecord> _entityStore = new("entities.json");
    private List<DocRef> _docs = new();
    private List<EntityRecord> _entities = new();
    private DocRef? _selected;

    public DocumentsPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Formation", "Operating agreement or bylaws", "EIN letter", "Annual filing", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _entities = (await _entityStore.LoadAsync()).OrderBy(x => x.LegalName).ToList();
        EntityBox.ItemsSource = _entities.Select(x => x.LegalName).ToList();
        _docs = (await _store.LoadAsync()).OrderBy(d => d.EntityLabel).ThenBy(d => d.Kind).ToList();
        DocList.ItemsSource = _docs;
    }

    private void DocList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = DocList.SelectedItem as DocRef;
        if (_selected is null) return;
        EntityBox.SelectedItem = _selected.EntityLabel;
        KindBox.SelectedItem = _selected.Kind;
        TitleBox.Text = _selected.Title;
        FiledPicker.Date = _selected.FiledDate;
        PathBox.Text = _selected.LocalPath;
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeFilter.Add("*");
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSingleFileAsync();
        if (file is not null) PathBox.Text = file.Path;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (EntityBox.SelectedIndex < 0) { ErrorText.Text = "Pick the entity."; return; }
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "A title is required."; return; }
        var d = _selected ?? new DocRef();
        var ent = _entities[EntityBox.SelectedIndex];
        d.EntityId = ent.Id; d.EntityLabel = ent.LegalName;
        d.Kind = KindBox.SelectedItem as string ?? d.Kind;
        d.Title = TitleBox.Text.Trim();
        d.FiledDate = FiledPicker.Date;
        d.LocalPath = PathBox.Text.Trim();
        if (_selected is null) _docs.Add(d);
        await _store.SaveAsync(_docs);
        _docs = _docs.OrderBy(x => x.EntityLabel).ThenBy(x => x.Kind).ToList();
        DocList.ItemsSource = _docs;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _docs.Remove(_selected);
        await _store.SaveAsync(_docs);
        _selected = null;
        DocList.ItemsSource = _docs;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        DocList.SelectedItem = null;
        TitleBox.Text = PathBox.Text = string.Empty;
        FiledPicker.Date = null;
    }
}
