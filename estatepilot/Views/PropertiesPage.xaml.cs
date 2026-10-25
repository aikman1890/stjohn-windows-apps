using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EstatePilot.Views;

public sealed partial class PropertiesPage : Page
{
    private readonly JsonListStore<Property> _store = new("properties.json");
    private readonly JsonListStore<EstateDoc> _docStore = new("documents.json");
    private List<Property> _properties = new();
    private Property? _selected;

    public PropertiesPage()
    {
        this.InitializeComponent();
        TypeBox.ItemsSource = new[] { "Primary", "Seasonal" };
        DocKindBox.ItemsSource = new[] { "Manual", "Warranty", "Service contract", "Floor plan", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _properties = (await _store.LoadAsync()).OrderBy(p => p.Name).ToList();
        PropertyList.ItemsSource = _properties.Select(p => $"{p.Name} ({p.Type})").ToList();
        if (_properties.Count > 0) PropertyList.SelectedIndex = 0;
    }

    private async void PropertyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = PropertyList.SelectedIndex;
        _selected = idx >= 0 && idx < _properties.Count ? _properties[idx] : null;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        TypeBox.SelectedItem = _selected.Type;
        AddressBox.Text = _selected.Address;
        NotesBox.Text = _selected.Notes;
        SystemsBox.Text = _selected.SystemsNote;
        AccessBox.Text = _selected.AccessPointer;
        var docs = await _docStore.LoadAsync();
        DocList.ItemsSource = docs.Where(d => d.PropertyId == _selected.Id).Select(d => d.DisplayLine).ToList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A house label is required."; return; }
        var p = _selected ?? new Property();
        p.Name = NameBox.Text.Trim();
        p.Type = TypeBox.SelectedItem as string ?? p.Type;
        p.Address = AddressBox.Text.Trim();
        p.Notes = NotesBox.Text.Trim();
        p.SystemsNote = SystemsBox.Text.Trim();
        p.AccessPointer = AccessBox.Text.Trim();
        if (_selected is null) _properties.Add(p);
        await _store.SaveAsync(_properties);
        _properties = _properties.OrderBy(x => x.Name).ToList();
        PropertyList.ItemsSource = _properties.Select(x => $"{x.Name} ({x.Type})").ToList();
        PropertyList.SelectedIndex = _properties.FindIndex(x => x.Id == p.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _properties.Remove(_selected);
        await _store.SaveAsync(_properties);
        _selected = null;
        PropertyList.ItemsSource = _properties.Select(x => $"{x.Name} ({x.Type})").ToList();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        PropertyList.SelectedItem = null;
        NameBox.Text = AddressBox.Text = NotesBox.Text = SystemsBox.Text = AccessBox.Text = string.Empty;
        TypeBox.SelectedItem = "Primary";
        DocList.ItemsSource = null;
    }

    private async void DocButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the property first."; return; }
        var docs = await _docStore.LoadAsync();
        docs.Add(new EstateDoc
        {
            PropertyId = _selected.Id, PropertyLabel = _selected.Name,
            Kind = DocKindBox.SelectedItem as string ?? "Other",
            Title = DocTitleBox.Text.Trim(), LocalPath = DocPathBox.Text.Trim(),
        });
        await _docStore.SaveAsync(docs);
        DocTitleBox.Text = DocPathBox.Text = string.Empty;
        PropertyList_SelectionChanged(this, null!);
    }
}
