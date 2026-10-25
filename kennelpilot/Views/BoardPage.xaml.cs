using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KennelPilot.Views;

public sealed partial class BoardPage : Page
{
    private readonly JsonListStore<Stay> _stayStore = new("stays.json");
    private readonly JsonListStore<Pet> _petStore = new("pets.json");
    private readonly KennelSettingsStore _settingsStore = new();
    private List<Stay> _stays = new();
    private List<Pet> _pets = new();
    private Stay? _selected;
    private bool _gateOverrideArmed;

    public BoardPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = new[] { "Reserved", "Checked in", "Checked out", "Cancelled", "No-show" };
        StatusCombo.SelectedIndex = 0;
        InPicker.Date = DateTimeOffset.Now;
        OutPicker.Date = DateTimeOffset.Now.AddDays(3);
        InPicker.DateChanged += (_, _) => UpdateGate();
        OutPicker.DateChanged += (_, _) => UpdateGate();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _pets = await _petStore.LoadAsync();
        PetPicker.ItemsSource = _pets.Select(p => p.FullLabel).ToList();
        if (_pets.Count > 0) PetPicker.SelectedIndex = 0;
        _stays = await _stayStore.LoadAsync();
        await RefreshListsAsync();
        UpdateGate();
    }

    private async Task RefreshListsAsync()
    {
        var today = DateTimeOffset.Now.Date;
        var settings = await _settingsStore.LoadAsync();
        var inHouse = _stays.Where(s => s.IsInHouse).OrderBy(s => s.ExpectedOut).ToList();
        CapacityText.Text = settings.Capacity > 0
            ? $"In house: {inHouse.Count} of {settings.Capacity} run(s)."
            : $"In house: {inHouse.Count} pet(s).";
        InHouseList.ItemsSource = inHouse;
        UpcomingList.ItemsSource = _stays
            .Where(s => s.Status == "Reserved" && s.CheckIn.Date >= today)
            .OrderBy(s => s.CheckIn).ToList();
        ArrivalsList.ItemsSource = _stays
            .Where(s => s.Status == "Reserved" && s.CheckIn.Date == today).ToList();
        DeparturesList.ItemsSource = _stays
            .Where(s => s.IsInHouse && s.ExpectedOut.Date == today).ToList();
    }

    private Pet? PickedPet =>
        PetPicker.SelectedIndex >= 0 && PetPicker.SelectedIndex < _pets.Count
            ? _pets[PetPicker.SelectedIndex]
            : null;

    private void UpdateGate()
    {
        _gateOverrideArmed = false;
        var pet = PickedPet;
        if (pet is null) { GateText.Text = string.Empty; return; }
        var (state, detail) = pet.VaccineStateOn(InPicker.Date);
        GateText.Text = state switch
        {
            "expired" => $"VACCINE GATE: {pet.Name} - {detail}. Saving is blocked until the pet file is fixed or you press Save twice to override with a reason in the stay notes.",
            "unknown" => $"VACCINE GATE: {pet.Name} has {detail}. Saving is blocked until records are entered or you press Save twice to override with a reason in the stay notes.",
            _ => $"{pet.Name}: vaccinations {detail} for a {InPicker.Date:MMM d} check-in.",
        };
    }

    private void PetPickerChanged(object sender, SelectionChangedEventArgs e) => UpdateGate();

    private void StayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var stay = (sender as ListView)?.SelectedItem as Stay;
        if (stay is null) return;
        _selected = stay;
        var idx = _pets.FindIndex(p => p.Id == stay.PetId);
        if (idx >= 0) PetPicker.SelectedIndex = idx;
        RunBox.Text = stay.RunLabel;
        StatusCombo.SelectedItem = stay.Status;
        InPicker.Date = stay.CheckIn;
        OutPicker.Date = stay.ExpectedOut;
        NotesBox.Text = stay.Notes;
        UpdateGate();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var pet = PickedPet;
        if (pet is null) return;
        var (state, detail) = pet.VaccineStateOn(InPicker.Date);
        var gated = state is "expired" or "unknown";
        if (gated && !_gateOverrideArmed)
        {
            _gateOverrideArmed = true;
            GateText.Text = $"GATE HELD: {detail}. If you have verified the records outside the app, put the reason in the stay notes and press Save again to override - the override and reason stay on the stay record.";
            return;
        }
        if (gated && string.IsNullOrWhiteSpace(NotesBox.Text))
        {
            GateText.Text = "Override needs a reason in the stay notes (for example: records verified by phone with the vet, with date).";
            return;
        }

        var item = _selected ?? new Stay();
        item.PetId = pet.Id;
        item.PetLabel = pet.FullLabel;
        item.OwnerId = pet.OwnerId;
        item.OwnerLabel = pet.OwnerLabel;
        item.RunLabel = RunBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? "Reserved";
        item.CheckIn = InPicker.Date;
        item.ExpectedOut = OutPicker.Date;
        if (item.Status == "Checked out" && item.ActualOut is null) item.ActualOut = DateTimeOffset.Now;
        item.Notes = NotesBox.Text.Trim();
        item.VaccineSnapshot = gated
            ? $"OVERRIDE at save: {detail}. Reason: {item.Notes}"
            : $"Gate passed: {detail}";
        if (_selected is null) _stays.Add(item);
        await _stayStore.SaveAsync(_stays);
        _selected = item;
        _gateOverrideArmed = false;
        await RefreshListsAsync();
        UpdateGate();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _stays.RemoveAll(s => s.Id == _selected.Id);
        await _stayStore.SaveAsync(_stays);
        _selected = null;
        ClearForm();
        await RefreshListsAsync();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        InHouseList.SelectedItem = null;
        UpcomingList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        RunBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        InPicker.Date = DateTimeOffset.Now;
        OutPicker.Date = DateTimeOffset.Now.AddDays(3);
        NotesBox.Text = string.Empty;
        UpdateGate();
    }
}
