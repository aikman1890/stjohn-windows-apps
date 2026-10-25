using HuntPilot.Models;
using HuntPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace HuntPilot.Views;

public sealed partial class HuntersPage : Page
{
    private readonly JsonListStore<Hunter> _store = new("hunters.json");
    private List<Hunter> _hunters = new();
    private Hunter? _selected;

    public HuntersPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _hunters = (await _store.LoadAsync()).OrderBy(h => h.Name).ToList();
        HunterList.ItemsSource = _hunters;
        if (_hunters.Count > 0) HunterList.SelectedIndex = 0;
    }

    private async void HunterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = HunterList.SelectedItem as Hunter;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        ContactBox.Text = _selected.Contact;
        LicenseBox.Text = _selected.LicenseNumber;
        StateBox.Text = _selected.LicenseState;
        ExpiryPicker.Date = _selected.LicenseExpiry;
        PartyBox.Text = _selected.PartyNote;
        PrefsBox.Text = _selected.Preferences;
        NotesBox.Text = _selected.Notes;
        var hunts = await new JsonListStore<HuntBooking>("hunts.json").LoadAsync();
        var mine = hunts.Where(h => h.HunterId == _selected.Id).OrderByDescending(h => h.StartDate).ToList();
        var harvests = await new JsonListStore<Harvest>("harvests.json").LoadAsync();
        var myHarvests = harvests.Where(h => h.HunterLabel == _selected.Name).ToList();
        HistoryText.Text = mine.Count == 0 ? "No hunts on file yet."
            : $"Lifetime value {mine.Sum(h => h.TotalCharges):0.00} across {mine.Count} hunt(s); {myHarvests.Count} harvest(s) logged.\n" +
              string.Join("\n", mine.Select(h => $"{h.StartDate:MMM yyyy} - {h.SpeciesTargeted} - {h.TotalCharges:0.00} {h.Currency} - {h.Status}"));
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A name is required."; return; }
        var h = _selected ?? new Hunter();
        h.Name = NameBox.Text.Trim();
        h.Contact = ContactBox.Text.Trim();
        h.LicenseNumber = LicenseBox.Text.Trim();
        h.LicenseState = StateBox.Text.Trim();
        h.LicenseExpiry = ExpiryPicker.Date;
        h.PartyNote = PartyBox.Text.Trim();
        h.Preferences = PrefsBox.Text.Trim();
        h.Notes = NotesBox.Text.Trim();
        if (_selected is null) _hunters.Add(h);
        await _store.SaveAsync(_hunters);
        _hunters = _hunters.OrderBy(x => x.Name).ToList();
        HunterList.ItemsSource = _hunters;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        HunterList.SelectedItem = null;
        NameBox.Text = ContactBox.Text = LicenseBox.Text = StateBox.Text = PartyBox.Text =
            PrefsBox.Text = NotesBox.Text = string.Empty;
        ExpiryPicker.Date = null;
        HistoryText.Text = string.Empty;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _hunters.Remove(_selected);
        await _store.SaveAsync(_hunters);
        _selected = null;
        HunterList.ItemsSource = _hunters;
    }
}
