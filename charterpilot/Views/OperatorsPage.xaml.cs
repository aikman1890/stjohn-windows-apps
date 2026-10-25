using CharterPilot.Models;
using CharterPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CharterPilot.Views;

public sealed partial class OperatorsPage : Page
{
    private readonly JsonListStore<Operator> _store = new("operators.json");
    private readonly JsonListStore<Tail> _tailStore = new("tails.json");
    private List<Operator> _operators = new();
    private Operator? _selected;

    public OperatorsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _operators = (await _store.LoadAsync()).OrderBy(o => o.Name).ToList();
        OperatorList.ItemsSource = _operators.Select(o => $"{o.Name} - {o.HomeBase}").ToList();
        await RefreshTailsAsync();
        if (_operators.Count > 0) OperatorList.SelectedIndex = 0;
    }

    private async Task RefreshTailsAsync()
    {
        var tails = await _tailStore.LoadAsync();
        TailList.ItemsSource = _selected is null ? tails : tails.Where(t => t.OperatorId == _selected.Id).ToList();
    }

    private void OperatorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = OperatorList.SelectedIndex;
        _selected = idx >= 0 && idx < _operators.Count ? _operators[idx] : null;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        ContactBox.Text = _selected.Contact;
        BaseBox.Text = _selected.HomeBase;
        TypesBox.Text = _selected.AircraftTypes;
        OpNotesBox.Text = _selected.Notes;
        _ = RefreshTailsAsync();
    }

    private async void SaveOpButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "An operator name is required."; return; }
        var o = _selected ?? new Operator();
        o.Name = NameBox.Text.Trim();
        o.Contact = ContactBox.Text.Trim();
        o.HomeBase = BaseBox.Text.Trim();
        o.AircraftTypes = TypesBox.Text.Trim();
        o.Notes = OpNotesBox.Text.Trim();
        if (_selected is null) _operators.Add(o);
        await _store.SaveAsync(_operators);
        _operators = _operators.OrderBy(x => x.Name).ToList();
        OperatorList.ItemsSource = _operators.Select(x => $"{x.Name} - {x.HomeBase}").ToList();
    }

    private void NewOpButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        OperatorList.SelectedItem = null;
        NameBox.Text = ContactBox.Text = BaseBox.Text = TypesBox.Text = OpNotesBox.Text = string.Empty;
    }

    private async void DeleteOpButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _operators.Remove(_selected);
        await _store.SaveAsync(_operators);
        _selected = null;
        OperatorList.ItemsSource = _operators.Select(x => $"{x.Name} - {x.HomeBase}").ToList();
    }

    private async void TailButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the operator first."; return; }
        if (string.IsNullOrWhiteSpace(TailNumBox.Text)) { ErrorText.Text = "Enter the tail number."; return; }
        var tails = await _tailStore.LoadAsync();
        tails.Add(new Tail
        {
            OperatorId = _selected.Id, OperatorLabel = _selected.Name,
            TailNumber = TailNumBox.Text.Trim(), AircraftType = TailTypeBox.Text.Trim(),
            HomeBase = TailBaseBox.Text.Trim(), Notes = TailNotesBox.Text.Trim(),
        });
        await _tailStore.SaveAsync(tails);
        TailNumBox.Text = TailTypeBox.Text = TailBaseBox.Text = TailNotesBox.Text = string.Empty;
        await RefreshTailsAsync();
    }
}
