using EstatePilot.Models;
using EstatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EstatePilot.Views;

public sealed partial class PeoplePage : Page
{
    private readonly JsonListStore<StaffMember> _staffStore = new("staff.json");
    private readonly JsonListStore<Vendor> _vendorStore = new("vendors.json");
    private readonly JsonListStore<Property> _propertyStore = new("properties.json");
    private List<StaffMember> _staff = new();
    private List<Vendor> _vendors = new();
    private List<Property> _properties = new();
    private StaffMember? _member;
    private Vendor? _vendor;

    public PeoplePage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _properties = (await _propertyStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PropertyBox.ItemsSource = _properties.Select(p => p.Name).ToList();
        _staff = (await _staffStore.LoadAsync()).OrderBy(s => s.Name).ToList();
        StaffList.ItemsSource = _staff;
        _vendors = (await _vendorStore.LoadAsync()).OrderBy(v => v.Name).ToList();
        VendorList.ItemsSource = _vendors;
    }

    private void StaffList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _member = StaffList.SelectedItem as StaffMember;
        if (_member is null) return;
        NameBox.Text = _member.Name;
        RoleBox.Text = _member.Role;
        PropertyBox.SelectedItem = _member.PropertyLabel;
        ContactBox.Text = _member.Contact;
        ScheduleBox.Text = _member.SchedulePattern;
        StartPicker.Date = _member.StartDate;
        DocNoteBox.Text = _member.DocNote;
        DocPicker.Date = _member.DocExpiry;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A name is required."; return; }
        if (PropertyBox.SelectedIndex < 0) { ErrorText.Text = "Pick the property."; return; }
        var s = _member ?? new StaffMember();
        var p = _properties[PropertyBox.SelectedIndex];
        s.PropertyId = p.Id; s.PropertyLabel = p.Name;
        s.Name = NameBox.Text.Trim();
        s.Role = RoleBox.Text.Trim();
        s.Contact = ContactBox.Text.Trim();
        s.SchedulePattern = ScheduleBox.Text.Trim();
        s.StartDate = StartPicker.Date;
        s.DocNote = DocNoteBox.Text.Trim();
        s.DocExpiry = DocPicker.Date;
        if (_member is null) _staff.Add(s);
        await _staffStore.SaveAsync(_staff);
        _staff = _staff.OrderBy(x => x.Name).ToList();
        StaffList.ItemsSource = _staff;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _member = null;
        StaffList.SelectedItem = null;
        NameBox.Text = RoleBox.Text = ContactBox.Text = ScheduleBox.Text = DocNoteBox.Text = string.Empty;
        StartPicker.Date = DocPicker.Date = null;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_member is null) return;
        _staff.Remove(_member);
        await _staffStore.SaveAsync(_staff);
        _member = null;
        StaffList.ItemsSource = _staff;
    }

    private void VendorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vendor = VendorList.SelectedItem as Vendor;
        if (_vendor is null) return;
        VendorNameBox.Text = _vendor.Name;
        TradeBox.Text = _vendor.Trade;
        VendorContactBox.Text = _vendor.Contact;
        CoiPicker.Date = _vendor.CoiExpiry;
        LastJobBox.Text = _vendor.LastJobNote;
        RateBox.Text = _vendor.RateNote;
    }

    private async void SaveVendorButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(VendorNameBox.Text)) { ErrorText.Text = "A vendor name is required."; return; }
        var v = _vendor ?? new Vendor();
        v.Name = VendorNameBox.Text.Trim();
        v.Trade = TradeBox.Text.Trim();
        v.Contact = VendorContactBox.Text.Trim();
        v.CoiExpiry = CoiPicker.Date;
        v.LastJobNote = LastJobBox.Text.Trim();
        v.RateNote = RateBox.Text.Trim();
        if (_vendor is null) _vendors.Add(v);
        await _vendorStore.SaveAsync(_vendors);
        _vendors = _vendors.OrderBy(x => x.Name).ToList();
        VendorList.ItemsSource = _vendors;
    }

    private void NewVendorButton_Click(object sender, RoutedEventArgs e)
    {
        _vendor = null;
        VendorList.SelectedItem = null;
        VendorNameBox.Text = TradeBox.Text = VendorContactBox.Text = LastJobBox.Text = RateBox.Text = string.Empty;
        CoiPicker.Date = null;
    }

    private async void DeleteVendorButton_Click(object sender, RoutedEventArgs e)
    {
        if (_vendor is null) return;
        _vendors.Remove(_vendor);
        await _vendorStore.SaveAsync(_vendors);
        _vendor = null;
        VendorList.ItemsSource = _vendors;
    }
}
