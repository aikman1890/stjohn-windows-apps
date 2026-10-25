using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LienPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly FacilitySettingsStore _settingsStore = new();
    private readonly JsonListStore<StateProfile> _profileStore = new("profiles.json");
    private FacilitySettings _settings = new();
    private List<StateProfile> _profiles = new();
    private StateProfile? _selected;

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _settings = await _settingsStore.LoadAsync();
        FacilityBox.Text = _settings.FacilityName;
        AddressBox.Text = _settings.Address;
        PhoneBox.Text = _settings.Phone;
        LateFeeBox.Text = _settings.LateFee.ToString("0.##");
        LienFeeBox.Text = _settings.LienFee.ToString("0.##");
        SaleFeeBox.Text = _settings.SaleFee.ToString("0.##");
        LockFeeBox.Text = _settings.LockCutFee.ToString("0.##");
        _profiles = await _profileStore.LoadAsync();
        RefreshProfiles();
    }

    private void RefreshProfiles()
    {
        ProfilesList.ItemsSource = null;
        ProfilesList.ItemsSource = _profiles;
        var active = _profiles.FirstOrDefault(p => p.Id == _settings.ActiveProfileId);
        ActiveText.Text = active is null ? "No active profile." : $"ACTIVE: {active.Name}";
    }

    private static decimal ParseDec(string text) =>
        decimal.TryParse(text.Trim().TrimStart('$').Replace(",", ""), out var v) ? v : 0m;

    private async void SaveFacilityButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.FacilityName = FacilityBox.Text.Trim();
        _settings.Address = AddressBox.Text.Trim();
        _settings.Phone = PhoneBox.Text.Trim();
        _settings.LateFee = ParseDec(LateFeeBox.Text);
        _settings.LienFee = ParseDec(LienFeeBox.Text);
        _settings.SaleFee = ParseDec(SaleFeeBox.Text);
        _settings.LockCutFee = ParseDec(LockFeeBox.Text);
        await _settingsStore.SaveAsync(_settings);
        StatusText.Text = "Facility and fees saved.";
    }

    private void ProfilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ProfilesList.SelectedItem as StateProfile;
        if (_selected is null) return;
        ProfileNameBox.Text = _selected.Name;
        NoticeOffsetBox.Text = _selected.NoticeDayOffset.ToString();
        WaitBox.Text = _selected.WaitingDaysAfterNotice.ToString();
        PubCheck.IsChecked = _selected.PublicationRequired;
        PubDaysBox.Text = _selected.SaleDaysAfterFirstPublication.ToString();
        StepsBox.Text = string.Join(Environment.NewLine, _selected.RequiredSteps);
        ProfileNotesBox.Text = _selected.Notes;
        ConfirmCheck.IsChecked = _selected.Confirmed;
    }

    private async void SaveProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new StateProfile();
        item.Name = ProfileNameBox.Text.Trim();
        item.IsSample = false;
        item.NoticeDayOffset = int.TryParse(NoticeOffsetBox.Text.Trim(), out var n) ? n : 5;
        item.WaitingDaysAfterNotice = int.TryParse(WaitBox.Text.Trim(), out var wv) ? wv : 30;
        item.PublicationRequired = PubCheck.IsChecked == true;
        item.SaleDaysAfterFirstPublication = int.TryParse(PubDaysBox.Text.Trim(), out var pd) ? pd : 15;
        item.RequiredSteps = StepsBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        item.Notes = ProfileNotesBox.Text.Trim();
        var nowConfirmed = ConfirmCheck.IsChecked == true;
        if (nowConfirmed && !item.Confirmed) item.ConfirmedDate = DateTimeOffset.Now;
        item.Confirmed = nowConfirmed;
        if (_selected is null) _profiles.Add(item);
        await _profileStore.SaveAsync(_profiles);
        _selected = item;
        StatusText.Text = item.Confirmed
            ? $"Profile \"{item.Name}\" saved and confirmed."
            : "Profile saved but NOT confirmed - clocks will keep flagging that until you confirm it.";
        RefreshProfiles();
    }

    private async void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _profiles.RemoveAll(p => p.Id == _selected.Id);
        await _profileStore.SaveAsync(_profiles);
        _selected = null;
        RefreshProfiles();
    }

    private void ClearProfileButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ProfilesList.SelectedItem = null;
        ProfileNameBox.Text = string.Empty;
        NoticeOffsetBox.Text = "5";
        WaitBox.Text = "30";
        PubCheck.IsChecked = false;
        PubDaysBox.Text = "15";
        StepsBox.Text = string.Empty;
        ProfileNotesBox.Text = string.Empty;
        ConfirmCheck.IsChecked = false;
    }

    private async void ActivateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _settings.ActiveProfileId = _selected.Id;
        await _settingsStore.SaveAsync(_settings);
        RefreshProfiles();
        StatusText.Text = $"\"{_selected.Name}\" is now the active profile.";
    }
}
