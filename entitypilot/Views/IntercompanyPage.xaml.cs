using System.Globalization;
using EntityPilot.Models;
using EntityPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EntityPilot.Views;

public sealed partial class IntercompanyPage : Page
{
    private readonly JsonListStore<IntercompanyEntry> _store = new("intercompany.json");
    private readonly JsonListStore<EntityRecord> _entityStore = new("entities.json");
    private List<IntercompanyEntry> _entries = new();
    private List<EntityRecord> _entities = new();
    private IntercompanyEntry? _selected;

    public IntercompanyPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Loan", "Transfer", "Repayment" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _entities = (await _entityStore.LoadAsync()).OrderBy(x => x.LegalName).ToList();
        FromBox.ItemsSource = _entities.Select(x => x.LegalName).ToList();
        ToBox.ItemsSource = _entities.Select(x => x.LegalName).ToList();
        _entries = (await _store.LoadAsync()).OrderByDescending(x => x.Date).ToList();
        Refresh();
    }

    private void Refresh()
    {
        EntryList.ItemsSource = _entries;
        // Net per unordered pair: loans/transfers add in their direction; repayments run opposite.
        var pairs = new Dictionary<string, decimal>();
        var labels = new Dictionary<string, string>();
        foreach (var x in _entries.OrderBy(x => x.Date))
        {
            var key = string.Compare(x.FromEntityId, x.ToEntityId, StringComparison.Ordinal) < 0
                ? $"{x.FromEntityId}|{x.ToEntityId}" : $"{x.ToEntityId}|{x.FromEntityId}";
            var forward = key.StartsWith(x.FromEntityId, StringComparison.Ordinal);
            var delta = x.Kind == "Repayment" ? -x.Amount : x.Amount;
            if (!forward) delta = -delta;
            pairs[key] = pairs.GetValueOrDefault(key) + delta;
            var first = key.Split('|')[0];
            labels[key] = _entities.FirstOrDefault(en => en.Id == first)?.LegalName ?? first;
        }
        BalanceList.ItemsSource = pairs.Select(kv =>
        {
            var other = kv.Key.Split('|')[1];
            var otherLabel = _entities.FirstOrDefault(en => en.Id == other)?.LegalName ?? other;
            return kv.Value >= 0
                ? $"{labels[kv.Key]} is owed {kv.Value:C} by {otherLabel}"
                : $"{otherLabel} is owed {-kv.Value:C} by {labels[kv.Key]}";
        }).ToList();
    }

    private void EntryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = EntryList.SelectedItem as IntercompanyEntry;
        if (_selected is null) return;
        FromBox.SelectedItem = _selected.FromLabel;
        ToBox.SelectedItem = _selected.ToLabel;
        KindBox.SelectedItem = _selected.Kind;
        AmountBox.Text = _selected.Amount.ToString(CultureInfo.InvariantCulture);
        DatePicker.Date = _selected.Date;
        TermsBox.Text = _selected.TermsNote;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var fi = FromBox.SelectedIndex; var ti = ToBox.SelectedIndex;
        if (fi < 0 || ti < 0 || fi == ti) { ErrorText.Text = "Pick two different entities."; return; }
        if (!decimal.TryParse(AmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        { ErrorText.Text = "Enter the amount."; return; }
        var x = _selected ?? new IntercompanyEntry();
        x.FromEntityId = _entities[fi].Id; x.FromLabel = _entities[fi].LegalName;
        x.ToEntityId = _entities[ti].Id; x.ToLabel = _entities[ti].LegalName;
        x.Kind = KindBox.SelectedItem as string ?? x.Kind;
        x.Amount = amount;
        x.Date = DatePicker.Date ?? x.Date;
        x.TermsNote = TermsBox.Text.Trim();
        if (_selected is null) _entries.Add(x);
        await _store.SaveAsync(_entries);
        _entries = _entries.OrderByDescending(en => en.Date).ToList();
        Refresh();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _entries.Remove(_selected);
        await _store.SaveAsync(_entries);
        _selected = null;
        Refresh();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        EntryList.SelectedItem = null;
        AmountBox.Text = TermsBox.Text = string.Empty;
        DatePicker.Date = DateTimeOffset.Now;
        KindBox.SelectedItem = "Loan";
    }
}
