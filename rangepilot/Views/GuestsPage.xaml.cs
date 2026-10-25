using System.Globalization;
using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class GuestsPage : Page
{
    private readonly JsonListStore<RangeGuest> _store = new("guests.json");
    private List<RangeGuest> _guests = new();
    private RangeGuest? _selected;

    public GuestsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _guests = (await _store.LoadAsync())
            .OrderByDescending(g => g.DaysSinceVisit ?? -1).ToList();
        GuestList.ItemsSource = _guests;
    }

    private void GuestList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = GuestList.SelectedItem as RangeGuest;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        ContactBox.Text = _selected.Contact;
        OccasionBox.Text = _selected.OccasionNote;
        ReferralBox.Text = _selected.ReferralNote;
        VisitsBox.Text = _selected.TotalVisits.ToString(CultureInfo.InvariantCulture);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "Name the guest."; return; }
        var g = _selected ?? new RangeGuest();
        g.Name = NameBox.Text.Trim();
        g.Contact = ContactBox.Text.Trim();
        g.OccasionNote = OccasionBox.Text.Trim();
        g.ReferralNote = ReferralBox.Text.Trim();
        if (int.TryParse(VisitsBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var visits)) g.TotalVisits = visits;
        if (_selected is null) _guests.Add(g);
        await _store.SaveAsync(_guests);
        GuestList.ItemsSource = _guests;
    }

    private async void VisitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the guest."; return; }
        _selected.TotalVisits += 1;
        _selected.LastVisitDate = DateTimeOffset.Now;
        await _store.SaveAsync(_guests);
        VisitsBox.Text = _selected.TotalVisits.ToString(CultureInfo.InvariantCulture);
        GuestList.ItemsSource = null;
        GuestList.ItemsSource = _guests;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        GuestList.SelectedItem = null;
        NameBox.Text = ContactBox.Text = OccasionBox.Text = ReferralBox.Text = VisitsBox.Text = string.Empty;
    }
}
