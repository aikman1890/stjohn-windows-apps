using System.Globalization;
using PlotPilot.Models;
using PlotPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PlotPilot.Views;

public sealed partial class RegisterPage : Page
{
    private readonly JsonListStore<PlotSpace> _store = new("spaces.json");
    private readonly JsonListStore<ChainEntry> _chainStore = new("chain.json");
    private List<PlotSpace> _items = new();
    private List<ChainEntry> _chain = new();
    private PlotSpace? _selected;

    public RegisterPage()
    {
        this.InitializeComponent();
        StatusCombo.SelectedIndex = 0;
        ChainKindCombo.SelectedIndex = 0;
        PurchasePicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        _chain = await _chainStore.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(s => s.Address).ToList();
        ChainList.ItemsSource = _selected is null
            ? null
            : _chain.Where(c => c.SpaceId == _selected.Id).OrderBy(c => c.Date).ToList();
    }

    private void PurchaseCheck_Changed(object sender, RoutedEventArgs e)
    {
        PurchasePicker.IsEnabled = PurchaseCheck.IsChecked == true;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as PlotSpace;
        if (_selected is null) return;
        SectionBox.Text = _selected.Section;
        BlockBox.Text = _selected.Block;
        LotBox.Text = _selected.Lot;
        SpaceBox.Text = _selected.Space;
        StatusCombo.SelectedItem = _selected.Status;
        OwnerBox.Text = _selected.OwnerName;
        DeedBox.Text = _selected.DeedRef;
        PriceBox.Text = _selected.PurchasePrice.ToString(CultureInfo.InvariantCulture);
        CareBox.Text = _selected.CareContribution.ToString(CultureInfo.InvariantCulture);
        PurchaseCheck.IsChecked = _selected.PurchaseDate is not null;
        if (_selected.PurchaseDate is { } p) PurchasePicker.Date = p;
        SourceBox.Text = _selected.Source;
        NotesBox.Text = _selected.Notes;
        RefreshList();
    }

    private decimal ParseMoney(string text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new PlotSpace();
        item.Section = SectionBox.Text.Trim();
        item.Block = BlockBox.Text.Trim();
        item.Lot = LotBox.Text.Trim();
        item.Space = SpaceBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? "Available";
        item.OwnerName = OwnerBox.Text.Trim();
        item.DeedRef = DeedBox.Text.Trim();
        item.PurchasePrice = ParseMoney(PriceBox.Text);
        item.CareContribution = ParseMoney(CareBox.Text);
        item.PurchaseDate = PurchaseCheck.IsChecked == true ? PurchasePicker.Date : null;
        item.Source = SourceBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(s => s.Id == _selected.Id);
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
        RefreshList();
    }

    private void ClearForm()
    {
        SectionBox.Text = string.Empty;
        BlockBox.Text = string.Empty;
        LotBox.Text = string.Empty;
        SpaceBox.Text = string.Empty;
        OwnerBox.Text = string.Empty;
        DeedBox.Text = string.Empty;
        PriceBox.Text = string.Empty;
        CareBox.Text = string.Empty;
        PurchaseCheck.IsChecked = false;
        SourceBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        FromBox.Text = string.Empty;
        ToBox.Text = string.Empty;
        StatusText.Text = string.Empty;
    }

    private async void ChainButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { StatusText.Text = "Select a space first."; return; }
        var kind = ChainKindCombo.SelectedItem as string ?? "Note";
        var entry = new ChainEntry
        {
            SpaceId = _selected.Id,
            SpaceLabel = _selected.Address,
            Date = DateTimeOffset.Now,
            Kind = kind,
            FromName = FromBox.Text.Trim(),
            ToName = ToBox.Text.Trim(),
            DeedRef = _selected.DeedRef,
            Source = "Staff entry",
        };
        _chain.Add(entry);
        // Sales/transfers/inheritances move the owner of record.
        if (kind is "Sale" or "Transfer" or "Inheritance" && !string.IsNullOrWhiteSpace(entry.ToName))
        {
            _selected.OwnerName = entry.ToName;
            OwnerBox.Text = entry.ToName;
            await _store.SaveAsync(_items);
        }
        await _chainStore.SaveAsync(_chain);
        FromBox.Text = string.Empty;
        ToBox.Text = string.Empty;
        RefreshList();
        StatusText.Text = "Chain entry appended.";
    }
}
