using System.Net;
using System.Text;
using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace EstatePilot.Views;

public sealed partial class ManualPage : Page
{
    private readonly JsonListStore<ManualSection> _store = new("manual.json");
    private readonly JsonListStore<Property> _propertyStore = new("properties.json");
    private List<ManualSection> _sections = new();
    private List<ManualSection> _filtered = new();
    private List<Property> _properties = new();
    private ManualSection? _selected;

    public ManualPage()
    {
        this.InitializeComponent();
        SectionKindBox.ItemsSource = new[] { "Room notes", "Standing instruction", "Principal preference", "Seasonal procedure" };
        PropertyFilterBox.SelectionChanged += (_, _) => ApplyFilter();
        SearchBox.TextChanged += (_, _) => ApplyFilter();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _properties = (await _propertyStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PropertyFilterBox.ItemsSource = _properties.Select(p => p.Name).ToList();
        _sections = (await _store.LoadAsync()).OrderBy(s => s.PropertyLabel).ThenBy(s => s.Title).ToList();
        if (_properties.Count > 0) PropertyFilterBox.SelectedIndex = 0;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var idx = PropertyFilterBox.SelectedIndex;
        var query = SearchBox.Text.Trim().ToLowerInvariant();
        _filtered = _sections.Where(s =>
            (idx < 0 || idx >= _properties.Count || s.PropertyId == _properties[idx].Id) &&
            (query.Length == 0 || s.Title.ToLowerInvariant().Contains(query) || s.Body.ToLowerInvariant().Contains(query)))
            .ToList();
        SectionList.ItemsSource = _filtered.Select(s => $"{s.PropertyLabel} - {s.Section} - {s.Title}").ToList();
    }

    private void SectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = SectionList.SelectedIndex;
        _selected = idx >= 0 && idx < _filtered.Count ? _filtered[idx] : null;
        if (_selected is null) return;
        SectionKindBox.SelectedItem = _selected.Section;
        TitleBox.Text = _selected.Title;
        BodyBox.Text = _selected.Body;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var idx = PropertyFilterBox.SelectedIndex;
        if (idx < 0 || idx >= _properties.Count) { ErrorText.Text = "Pick the property first (filter above)." ; return; }
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "A title is required."; return; }
        var s = _selected ?? new ManualSection();
        var p = _properties[idx];
        s.PropertyId = p.Id; s.PropertyLabel = p.Name;
        s.Section = SectionKindBox.SelectedItem as string ?? s.Section;
        s.Title = TitleBox.Text.Trim();
        s.Body = BodyBox.Text.Trim();
        if (_selected is null) _sections.Add(s);
        await _store.SaveAsync(_sections);
        ApplyFilter();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        SectionList.SelectedItem = null;
        TitleBox.Text = BodyBox.Text = string.Empty;
        SectionKindBox.SelectedItem = "Room notes";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _sections.Remove(_selected);
        await _store.SaveAsync(_sections);
        _selected = null;
        ApplyFilter();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var idx = PropertyFilterBox.SelectedIndex;
        if (idx < 0 || idx >= _properties.Count) { StatusText.Text = "Pick a property to export its manual."; return; }
        var p = _properties[idx];
        var mine = _sections.Where(s => s.PropertyId == p.Id).OrderBy(s => s.Section).ThenBy(s => s.Title).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>House manual</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}h2{font-size:15px;margin-top:18px;white-space:pre-wrap}</style></head><body>");
        sb.AppendLine($"<h1>House manual - {Esc(p.Name)}</h1>");
        foreach (var g in mine.GroupBy(s => s.Section))
        {
            sb.AppendLine($"<h2>{Esc(g.Key)}</h2>");
            foreach (var s in g)
                sb.AppendLine($"<h3>{Esc(s.Title)}</h3><p style=\"white-space:pre-wrap\">{Esc(s.Body)}</p>");
        }
        sb.AppendLine("<p>Exported from EstatePilot - the manager's local book. No access codes or credentials are recorded in this manual, by design.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = "house-manual.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Manual saved to {file.Path}";
    }
}
