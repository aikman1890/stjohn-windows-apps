using DenialPilot.Models;
using DenialPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DenialPilot.Views;

public sealed partial class ChecklistPage : Page
{
    private static readonly string[] StandardItems =
    {
        "Denial letter",
        "Explanation of Benefits (EOB)",
        "Policy excerpt (plan document pages that apply)",
        "Medical records for the date of service",
        "Physician letter of medical necessity",
        "Proof of submission (fax confirmation / certified mail receipt / portal note)",
    };

    private readonly JsonListStore<EvidenceItem> _store = new("checklist.json");
    private readonly JsonListStore<DenialCase> _caseStore = new("cases.json");
    private List<EvidenceItem> _items = new();
    private List<DenialCase> _cases = new();

    public ChecklistPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cases = await _caseStore.LoadAsync();
        CasePicker.ItemsSource = _cases.Select(c => c.DisplayLine).ToList();
        if (_cases.Count > 0) CasePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private DenialCase? PickedCase =>
        CasePicker.SelectedIndex >= 0 && CasePicker.SelectedIndex < _cases.Count
            ? _cases[CasePicker.SelectedIndex]
            : null;

    private void CasePicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

    private void RefreshList()
    {
        var picked = PickedCase;
        if (picked is null)
        {
            ItemsList.ItemsSource = null;
            ReadinessText.Text = "Add a case first.";
            return;
        }
        var mine = _items.Where(i => i.CaseId == picked.Id).ToList();
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = mine;
        ReadinessText.Text = mine.Count == 0
            ? "No checklist yet for this case - load the standard set."
            : $"{mine.Count(i => i.IsDone)} of {mine.Count} items gathered" +
              (mine.All(i => i.IsDone) ? " - ready to file." : ".");
    }

    private async void LoadStandardButton_Click(object sender, RoutedEventArgs e)
    {
        var picked = PickedCase;
        if (picked is null) return;
        foreach (var name in StandardItems)
        {
            if (!_items.Any(i => i.CaseId == picked.Id && i.ItemName == name))
            {
                _items.Add(new EvidenceItem { CaseId = picked.Id, CaseLabel = picked.DisplayLine, ItemName = name });
            }
        }
        await _store.SaveAsync(_items);
        RefreshList();
    }

    private async void AddCustomButton_Click(object sender, RoutedEventArgs e)
    {
        var picked = PickedCase;
        if (picked is null || string.IsNullOrWhiteSpace(CustomBox.Text)) return;
        _items.Add(new EvidenceItem
        {
            CaseId = picked.Id,
            CaseLabel = picked.DisplayLine,
            ItemName = CustomBox.Text.Trim(),
        });
        CustomBox.Text = string.Empty;
        await _store.SaveAsync(_items);
        RefreshList();
    }

    private async void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is EvidenceItem item)
        {
            _items.RemoveAll(i => i.Id == item.Id);
            await _store.SaveAsync(_items);
            RefreshList();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await _store.SaveAsync(_items);
        RefreshList();
    }
}
