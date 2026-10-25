using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FarewellPilot.Views;

public sealed partial class AftercarePage : Page
{
    private readonly JsonListStore<AftercareTask> _store = new("aftercare.json");
    private readonly JsonListStore<FuneralCase> _caseStore = new("cases.json");
    private List<AftercareTask> _items = new();
    private List<FuneralCase> _cases = new();

    public AftercarePage()
    {
        this.InitializeComponent();
        DuePicker.Date = DateTimeOffset.Now.AddDays(7);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cases = await _caseStore.LoadAsync();
        CasePicker.ItemsSource = _cases.Select(c => c.DisplayLine).ToList();
        if (_cases.Count > 0) CasePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshAll();
    }

    private void RefreshAll()
    {
        var weekEnd = DateTimeOffset.Now.Date.AddDays(7);
        DueList.ItemsSource = _items.Where(t => !t.Done && t.DueDate.Date <= weekEnd).OrderBy(t => t.DueDate).ToList();
        AllList.ItemsSource = _items.OrderBy(t => t.Done).ThenBy(t => t.DueDate).ToList();
    }

    private AftercareTask? PickedTask =>
        (DueList.SelectedItem as AftercareTask) ?? (AllList.SelectedItem as AftercareTask);

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (CasePicker.SelectedIndex < 0 || CasePicker.SelectedIndex >= _cases.Count) return;
        var c = _cases[CasePicker.SelectedIndex];
        _items.Add(new AftercareTask
        {
            CaseId = c.Id,
            CaseLabel = c.DisplayLine,
            Kind = string.IsNullOrWhiteSpace(KindBox.Text) ? "Custom follow-up" : KindBox.Text.Trim(),
            DueDate = DuePicker.Date,
            Note = NoteBox.Text.Trim(),
        });
        await _store.SaveAsync(_items);
        KindBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
        RefreshAll();
    }

    private async void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        if (PickedTask is not { } task) return;
        task.Done = true;
        task.DoneDate = DateTimeOffset.Now;
        await _store.SaveAsync(_items);
        RefreshAll();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (PickedTask is not { } task) return;
        _items.RemoveAll(t => t.Id == task.Id);
        await _store.SaveAsync(_items);
        RefreshAll();
    }
}
