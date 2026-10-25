using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KennelPilot.Views;

public sealed partial class PetsPage : Page
{
    private readonly JsonListStore<Pet> _store = new("pets.json");
    private readonly JsonListStore<Owner> _ownerStore = new("owners.json");
    private List<Pet> _items = new();
    private List<Owner> _owners = new();
    private Pet? _selected;
    private List<Vaccination> _editVax = new();

    public PetsPage()
    {
        this.InitializeComponent();
        SpeciesCombo.ItemsSource = new[] { "Dog", "Cat", "Other" };
        SpeciesCombo.SelectedIndex = 0;
        VaxExpiryPicker.Date = DateTimeOffset.Now.AddYears(1);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _owners = await _ownerStore.LoadAsync();
        OwnerPicker.ItemsSource = _owners.Select(o => o.Name).ToList();
        if (_owners.Count > 0) OwnerPicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(p => p.Name).ToList();
    }

    private void RefreshVaxList()
    {
        VaxList.ItemsSource = null;
        VaxList.ItemsSource = _editVax.Select(v =>
            $"{v.Name} - expires {(v.Expiry is { } e ? e.ToString("MMM d, yyyy") : "no date")}" +
            (string.IsNullOrWhiteSpace(v.DocumentRef) ? string.Empty : $" - doc {v.DocumentRef}")).ToList();
    }

    private Owner? PickedOwner =>
        OwnerPicker.SelectedIndex >= 0 && OwnerPicker.SelectedIndex < _owners.Count
            ? _owners[OwnerPicker.SelectedIndex]
            : null;

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as Pet;
        if (_selected is null) return;
        var idx = _owners.FindIndex(o => o.Id == _selected.OwnerId);
        if (idx >= 0) OwnerPicker.SelectedIndex = idx;
        NameBox.Text = _selected.Name;
        SpeciesCombo.SelectedItem = _selected.Species;
        BreedBox.Text = _selected.Breed;
        WeightBox.Text = _selected.WeightLb > 0 ? _selected.WeightLb.ToString("0.#") : string.Empty;
        AgeBox.Text = _selected.AgeNote;
        SpayCheck.IsChecked = _selected.SpayedNeutered;
        FeedingBox.Text = _selected.Feeding;
        MedsBox.Text = _selected.Medications;
        BehaviorBox.Text = _selected.Behavior;
        NotesBox.Text = _selected.Notes;
        _editVax = _selected.Vaccinations.Select(v => new Vaccination
        {
            Name = v.Name,
            Expiry = v.Expiry,
            DocumentRef = v.DocumentRef,
        }).ToList();
        RefreshVaxList();
    }

    private void VaxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VaxList.SelectedIndex >= 0 && VaxList.SelectedIndex < _editVax.Count)
        {
            var v = _editVax[VaxList.SelectedIndex];
            VaxNameBox.Text = v.Name;
            VaxDocBox.Text = v.DocumentRef;
            if (v.Expiry is { } e) VaxExpiryPicker.Date = e;
        }
    }

    private void VaxAddButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(VaxNameBox.Text)) return;
        if (VaxList.SelectedIndex >= 0 && VaxList.SelectedIndex < _editVax.Count &&
            _editVax[VaxList.SelectedIndex].Name == VaxNameBox.Text.Trim())
        {
            var existing = _editVax[VaxList.SelectedIndex];
            existing.Expiry = VaxExpiryPicker.Date;
            existing.DocumentRef = VaxDocBox.Text.Trim();
        }
        else
        {
            _editVax.Add(new Vaccination
            {
                Name = VaxNameBox.Text.Trim(),
                Expiry = VaxExpiryPicker.Date,
                DocumentRef = VaxDocBox.Text.Trim(),
            });
        }
        VaxNameBox.Text = string.Empty;
        VaxDocBox.Text = string.Empty;
        RefreshVaxList();
    }

    private void VaxRemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (VaxList.SelectedIndex >= 0 && VaxList.SelectedIndex < _editVax.Count)
        {
            _editVax.RemoveAt(VaxList.SelectedIndex);
            RefreshVaxList();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var owner = PickedOwner;
        if (owner is null) return;
        var item = _selected ?? new Pet();
        item.OwnerId = owner.Id;
        item.OwnerLabel = owner.Name;
        item.Name = NameBox.Text.Trim();
        item.Species = SpeciesCombo.SelectedItem as string ?? "Dog";
        item.Breed = BreedBox.Text.Trim();
        item.WeightLb = decimal.TryParse(WeightBox.Text.Trim(), out var wt) ? wt : 0m;
        item.AgeNote = AgeBox.Text.Trim();
        item.SpayedNeutered = SpayCheck.IsChecked == true;
        item.Feeding = FeedingBox.Text.Trim();
        item.Medications = MedsBox.Text.Trim();
        item.Behavior = BehaviorBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        item.Vaccinations = _editVax;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(p => p.Id == _selected.Id);
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
        NameBox.Text = string.Empty;
        BreedBox.Text = string.Empty;
        WeightBox.Text = string.Empty;
        AgeBox.Text = string.Empty;
        SpayCheck.IsChecked = false;
        FeedingBox.Text = string.Empty;
        MedsBox.Text = string.Empty;
        BehaviorBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        _editVax = new List<Vaccination>();
        RefreshVaxList();
    }
}
