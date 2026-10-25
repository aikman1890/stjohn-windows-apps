using System.Globalization;
using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class VesselsPage : Page
{
    private readonly JsonListStore<Vessel> _store = new("vessels.json");
    private readonly JsonListStore<YachtDoc> _docStore = new("documents.json");
    private List<Vessel> _vessels = new();
    private Vessel? _selected;

    public VesselsPage()
    {
        this.InitializeComponent();
        DocKindBox.ItemsSource = new[] { "Certificate", "Survey", "Manual", "Insurance", "Registration", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vessels = (await _store.LoadAsync()).OrderBy(v => v.Name).ToList();
        VesselList.ItemsSource = _vessels.Select(v => v.Name).ToList();
        if (_vessels.Count > 0) VesselList.SelectedIndex = 0;
    }

    private async void VesselList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = VesselList.SelectedIndex;
        _selected = idx >= 0 && idx < _vessels.Count ? _vessels[idx] : null;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        FlagBox.Text = _selected.Flag;
        BerthBox.Text = _selected.Berth;
        DimsBox.Text = _selected.Dimensions;
        OwnerBox.Text = _selected.OwnerName;
        OwnerContactBox.Text = _selected.OwnerContact;
        InsPicker.Date = _selected.InsuranceExpiry;
        RegPicker.Date = _selected.RegistrationExpiry;
        HoursBox.Text = _selected.CurrentEngineHours.ToString(CultureInfo.InvariantCulture);
        NotesBox.Text = _selected.Notes;
        OverrideBox.Text = _selected.ReadyOverrideNote;
        ErrorText.Text = string.Empty;

        var crew = await new JsonListStore<CrewMember>("crew.json").LoadAsync();
        var certs = await new JsonListStore<Certificate>("certificates.json").LoadAsync();
        var maintenance = await new JsonListStore<MaintenanceItem>("maintenance.json").LoadAsync();
        var blockers = Readiness.Blockers(_selected, crew, certs, maintenance);
        ReadyText.Text = blockers.Count == 0
            ? "CHARTER-READY: no expired assigned certificates, no overdue maintenance."
            : $"NOT CHARTER-READY ({blockers.Count} blocker(s)):\n" + string.Join("\n", blockers) +
              (_selected.ReadyOverrideDate is { } od ? $"\nOverride on file: {od:MMM d, yyyy} - {_selected.ReadyOverrideNote}" : string.Empty);

        var docs = await _docStore.LoadAsync();
        DocList.ItemsSource = docs.Where(d => d.VesselId == _selected.Id).Select(d => d.DisplayLine).ToList();
    }

    private void ReadForm(Vessel v)
    {
        v.Name = NameBox.Text.Trim();
        v.Flag = FlagBox.Text.Trim();
        v.Berth = BerthBox.Text.Trim();
        v.Dimensions = DimsBox.Text.Trim();
        v.OwnerName = OwnerBox.Text.Trim();
        v.OwnerContact = OwnerContactBox.Text.Trim();
        v.InsuranceExpiry = InsPicker.Date;
        v.RegistrationExpiry = RegPicker.Date;
        if (decimal.TryParse(HoursBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var h)) v.CurrentEngineHours = h;
        v.Notes = NotesBox.Text.Trim();
        v.ReadyOverrideNote = OverrideBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(v.ReadyOverrideNote)) v.ReadyOverrideDate = null;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A vessel name is required."; return; }
        var v = _selected ?? new Vessel();
        ReadForm(v);
        if (_selected is null) _vessels.Add(v);
        await _store.SaveAsync(_vessels);
        _vessels = _vessels.OrderBy(x => x.Name).ToList();
        VesselList.ItemsSource = _vessels.Select(x => x.Name).ToList();
        VesselList.SelectedIndex = _vessels.FindIndex(x => x.Id == v.Id);
    }

    private async void OverrideButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        if (string.IsNullOrWhiteSpace(OverrideBox.Text))
        { ErrorText.Text = "An override needs a note - it is never silent."; return; }
        _selected.ReadyOverrideNote = OverrideBox.Text.Trim();
        _selected.ReadyOverrideDate = DateTimeOffset.Now;
        await _store.SaveAsync(_vessels);
        VesselList_SelectionChanged(this, null!);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _vessels.Remove(_selected);
        await _store.SaveAsync(_vessels);
        _selected = null;
        VesselList.ItemsSource = _vessels.Select(x => x.Name).ToList();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        VesselList.SelectedItem = null;
        NameBox.Text = FlagBox.Text = BerthBox.Text = DimsBox.Text = OwnerBox.Text =
            OwnerContactBox.Text = NotesBox.Text = OverrideBox.Text = string.Empty;
        HoursBox.Text = "0";
        InsPicker.Date = RegPicker.Date = null;
        ReadyText.Text = string.Empty;
        DocList.ItemsSource = null;
    }

    private async void DocButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select a vessel first."; return; }
        var docs = await _docStore.LoadAsync();
        docs.Add(new YachtDoc
        {
            VesselId = _selected.Id, VesselLabel = _selected.Name,
            Kind = DocKindBox.SelectedItem as string ?? "Other",
            Title = DocTitleBox.Text.Trim(), LocalPath = DocPathBox.Text.Trim(),
        });
        await _docStore.SaveAsync(docs);
        DocTitleBox.Text = DocPathBox.Text = string.Empty;
        VesselList_SelectionChanged(this, null!);
    }
}
