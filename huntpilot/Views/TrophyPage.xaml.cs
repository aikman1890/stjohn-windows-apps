using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class TrophyPage : Page
{
    private readonly JsonListStore<TrophyCase> _store = new("trophies.json");
    private List<TrophyCase> _cases = new();
    private TrophyCase? _selected;
    private int _norm = 30;

    public TrophyPage()
    {
        this.InitializeComponent();
        StageBox.ItemsSource = TrophyCase.Stages;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await new HuntSettingsStore().LoadAsync();
        _norm = settings.StageNormDays <= 0 ? 30 : settings.StageNormDays;
        NormText.Text = $"Every harvested animal as a case: field dressed, processor, meat, taxidermist, mount, crate, ship, delivered - each stage with its own clock. Norm is {_norm} days per stage (yours to set in Settings): past it orange, past twice it red.";
        _cases = await _store.LoadAsync();
        foreach (var c in _cases) c.NormDays = _norm;
        _cases = _cases.OrderBy(c => c.Closed).ThenByDescending(c => c.DaysInStage).ToList();
        CaseList.ItemsSource = _cases;
    }

    private void CaseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = CaseList.SelectedItem as TrophyCase;
        if (_selected is null) return;
        StageBox.SelectedItem = _selected.Stage;
        ShipBox.Text = _selected.ShipNote;
        ProcessorBox.Text = string.IsNullOrWhiteSpace(_selected.ProcessorTicket)
            ? _selected.ProcessorName : $"{_selected.ProcessorName} / {_selected.ProcessorTicket}";
        TaxiBox.Text = string.IsNullOrWhiteSpace(_selected.WorkOrder)
            ? _selected.TaxidermistName : $"{_selected.TaxidermistName} / {_selected.WorkOrder}";
        NotesBox.Text = _selected.Notes;
    }

    private static (string Name, string Ref) SplitRef(string text)
    {
        var parts = (text ?? string.Empty).Split('/', 2);
        return (parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : string.Empty);
    }

    private async void MoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the trophy case."; return; }
        var stage = StageBox.SelectedItem as string ?? _selected.Stage;
        if (stage != _selected.Stage)
        {
            _selected.Stage = stage;
            _selected.StageDate = DateTimeOffset.Now;
        }
        var (pName, pTicket) = SplitRef(ProcessorBox.Text);
        _selected.ProcessorName = pName;
        if (!string.IsNullOrWhiteSpace(pTicket)) _selected.ProcessorTicket = pTicket;
        var (tName, tOrder) = SplitRef(TaxiBox.Text);
        _selected.TaxidermistName = tName;
        if (!string.IsNullOrWhiteSpace(tOrder)) _selected.WorkOrder = tOrder;
        _selected.ShipNote = ShipBox.Text.Trim();
        _selected.Notes = NotesBox.Text.Trim();
        await _store.SaveAsync(_cases);
        CaseList.ItemsSource = null;
        CaseList.ItemsSource = _cases;
        CaseList.SelectedItem = _selected;
    }
}
