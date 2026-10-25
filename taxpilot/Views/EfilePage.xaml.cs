using TaxPilot.Models;
using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class EfilePage : Page
{
    private readonly JsonListStore<EfileLog> _store = new("efile.json");
    private readonly JsonListStore<Engagement> _engStore = new("engagements.json");
    private readonly TaxSettingsStore _settingsStore = new();
    private List<EfileLog> _items = new();
    private List<Engagement> _engagements = new();
    private EfileLog? _selected;

    public EfilePage()
    {
        this.InitializeComponent();
        ResultCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _engagements = (await _engStore.LoadAsync()).Where(x => x.TaxYear == settings.CurrentTaxYear).ToList();
        EngagementPicker.ItemsSource = _engagements.Select(x => $"{x.ClientLabel} ({x.EntityType})").ToList();
        if (_engagements.Count > 0) EngagementPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(x => x.SubmittedAt).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as EfileLog;
        if (_selected is null) return;
        var idx = _engagements.FindIndex(x => x.Id == _selected.EngagementId);
        if (idx >= 0) EngagementPicker.SelectedIndex = idx;
        MethodBox.Text = _selected.Method;
        ResultCombo.SelectedItem = _selected.Result;
        CodeBox.Text = _selected.RejectCode;
        NoteBox.Text = _selected.Note;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (EngagementPicker.SelectedIndex < 0 || EngagementPicker.SelectedIndex >= _engagements.Count) return;
        var eng = _engagements[EngagementPicker.SelectedIndex];
        var item = _selected ?? new EfileLog { SubmittedAt = DateTimeOffset.Now };
        item.EngagementId = eng.Id;
        item.ClientLabel = eng.ClientLabel;
        item.Method = MethodBox.Text.Trim();
        item.Result = ResultCombo.SelectedItem as string ?? "Submitted";
        item.RejectCode = CodeBox.Text.Trim();
        item.Note = NoteBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);

        // Keep the engagement status in step with the latest e-file result.
        var all = await _engStore.LoadAsync();
        var target = all.FirstOrDefault(x => x.Id == eng.Id);
        if (target is not null)
        {
            target.Status = item.Result switch
            {
                "Accepted" => "Accepted",
                "Rejected" => "Rejected-fix",
                _ => "Filed",
            };
            await _engStore.SaveAsync(all);
        }
        _selected = item;
        RefreshList();
        StatusText.Text = $"Logged - {eng.ClientLabel} engagement status set to {item.Result switch { "Accepted" => "Accepted", "Rejected" => "Rejected-fix", _ => "Filed" }}.";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(x => x.Id == _selected.Id);
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
        MethodBox.Text = string.Empty;
        CodeBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
        StatusText.Text = string.Empty;
    }
}
