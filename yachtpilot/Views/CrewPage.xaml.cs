using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class CrewPage : Page
{
    private readonly JsonListStore<CrewMember> _crewStore = new("crew.json");
    private readonly JsonListStore<Certificate> _certStore = new("certificates.json");
    private readonly JsonListStore<Vessel> _vesselStore = new("vessels.json");
    private List<CrewMember> _crew = new();
    private List<Vessel> _vessels = new();
    private CrewMember? _selected;

    public CrewPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vessels = (await _vesselStore.LoadAsync()).OrderBy(v => v.Name).ToList();
        CrewVesselBox.ItemsSource = new[] { "(unassigned)" }.Concat(_vessels.Select(v => v.Name)).ToList();
        var settings = await new YachtSettingsStore().LoadAsync();
        CertTypeBox.ItemsSource = settings.CertTypes;
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _crew = (await _crewStore.LoadAsync()).OrderBy(c => c.Name).ToList();
        CrewList.ItemsSource = _crew.Select(c =>
            $"{c.Name} - {c.Role} - {(string.IsNullOrWhiteSpace(c.VesselLabel) ? "unassigned" : c.VesselLabel)}{(c.Active ? string.Empty : " - INACTIVE")}").ToList();
        var certs = await _certStore.LoadAsync();
        CertList.ItemsSource = certs.OrderBy(c => c.ExpiryDate).ToList();
    }

    private void CrewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var idx = CrewList.SelectedIndex;
        _selected = idx >= 0 && idx < _crew.Count ? _crew[idx] : null;
        if (_selected is null) return;
        CrewNameBox.Text = _selected.Name;
        CrewRoleBox.Text = _selected.Role;
        CrewVesselBox.SelectedItem = string.IsNullOrWhiteSpace(_selected.VesselLabel) ? "(unassigned)" : _selected.VesselLabel;
        CrewPhoneBox.Text = _selected.Phone;
        CrewEmailBox.Text = _selected.Email;
        CrewActiveCheck.IsChecked = _selected.Active;
    }

    private async void SaveCrewButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CrewNameBox.Text)) { ErrorText.Text = "A name is required."; return; }
        var c = _selected ?? new CrewMember();
        c.Name = CrewNameBox.Text.Trim();
        c.Role = CrewRoleBox.Text.Trim();
        var vi = CrewVesselBox.SelectedIndex;
        c.VesselId = vi > 0 && vi - 1 < _vessels.Count ? _vessels[vi - 1].Id : string.Empty;
        c.VesselLabel = vi > 0 && vi - 1 < _vessels.Count ? _vessels[vi - 1].Name : string.Empty;
        c.Phone = CrewPhoneBox.Text.Trim();
        c.Email = CrewEmailBox.Text.Trim();
        c.Active = CrewActiveCheck.IsChecked == true;
        if (_selected is null) _crew.Add(c);
        await _crewStore.SaveAsync(_crew);
        await ReloadAsync();
    }

    private void NewCrewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        CrewList.SelectedItem = null;
        CrewNameBox.Text = CrewRoleBox.Text = CrewPhoneBox.Text = CrewEmailBox.Text = string.Empty;
        CrewActiveCheck.IsChecked = true;
    }

    private async void DeleteCrewButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _crew.Remove(_selected);
        await _crewStore.SaveAsync(_crew);
        _selected = null;
        await ReloadAsync();
    }

    private async void CertButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the crew member first."; return; }
        var type = CertTypeBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(type) || CertExpiryPicker.Date is null)
        { ErrorText.Text = "Pick a certificate type and its expiry date."; return; }
        var certs = await _certStore.LoadAsync();
        certs.RemoveAll(c => c.CrewId == _selected.Id && c.Type == type);
        certs.Add(new Certificate
        {
            CrewId = _selected.Id, CrewLabel = _selected.Name, Type = type,
            ExpiryDate = CertExpiryPicker.Date.Value, Note = CertNoteBox.Text.Trim(),
        });
        await _certStore.SaveAsync(certs);
        var activity = await new JsonListStore<ActivityEntry>("activity.json").LoadAsync();
        activity.Add(new ActivityEntry
        {
            VesselId = _selected.VesselId, VesselLabel = _selected.VesselLabel,
            Kind = "Certificate renewed", Detail = $"{_selected.Name} - {type} now expires {CertExpiryPicker.Date:MMM d, yyyy}",
            MethodNote = "Recorded in office", Date = DateTimeOffset.Now,
        });
        await new JsonListStore<ActivityEntry>("activity.json").SaveAsync(activity);
        CertNoteBox.Text = string.Empty;
        await ReloadAsync();
    }
}
