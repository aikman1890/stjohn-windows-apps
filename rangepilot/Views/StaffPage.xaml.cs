using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class StaffPage : Page
{
    private readonly JsonListStore<StaffMember> _store = new("staff.json");
    private List<StaffMember> _staff = new();
    private StaffMember? _selected;

    public StaffPage()
    {
        this.InitializeComponent();
        RoleBox.ItemsSource = new[] { "Safety officer", "Driver / instructor", "Guide", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _staff = (await _store.LoadAsync()).OrderBy(s => s.Name).ToList();
        StaffList.ItemsSource = _staff;
        if (_staff.Count > 0) StaffList.SelectedIndex = 0;
    }

    private void StaffList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = StaffList.SelectedItem as StaffMember;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        RoleBox.SelectedItem = _selected.Role;
        CertsBox.Text = _selected.Certifications;
        ExpiryPicker.Date = _selected.CertExpiry;
        ScheduleBox.Text = _selected.ScheduleNote;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A name is required."; return; }
        var s = _selected ?? new StaffMember();
        s.Name = NameBox.Text.Trim();
        s.Role = RoleBox.SelectedItem as string ?? s.Role;
        s.Certifications = CertsBox.Text.Trim();
        s.CertExpiry = ExpiryPicker.Date;
        s.ScheduleNote = ScheduleBox.Text.Trim();
        if (_selected is null) _staff.Add(s);
        await _store.SaveAsync(_staff);
        _staff = _staff.OrderBy(x => x.Name).ToList();
        StaffList.ItemsSource = _staff;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        StaffList.SelectedItem = null;
        NameBox.Text = CertsBox.Text = ScheduleBox.Text = string.Empty;
        ExpiryPicker.Date = null;
        RoleBox.SelectedItem = "Safety officer";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _staff.Remove(_selected);
        await _store.SaveAsync(_staff);
        _selected = null;
        StaffList.ItemsSource = _staff;
    }
}
