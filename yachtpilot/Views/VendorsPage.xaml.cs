using YachtPilot.Models;
using YachtPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace YachtPilot.Views;

public sealed partial class VendorsPage : Page
{
    private readonly JsonListStore<Vendor> _store = new("vendors.json");
    private List<Vendor> _vendors = new();
    private Vendor? _selected;

    public VendorsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _vendors = (await _store.LoadAsync()).OrderBy(v => v.Name).ToList();
        VendorList.ItemsSource = _vendors;
        if (_vendors.Count > 0) VendorList.SelectedIndex = 0;
    }

    private void VendorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = VendorList.SelectedItem as Vendor;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        TradeBox.Text = _selected.Trade;
        ContactBox.Text = _selected.Contact;
        CoiPicker.Date = _selected.CoiExpiry;
        LastJobBox.Text = _selected.LastJobNote;
        NextJobBox.Text = _selected.NextBookedNote;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A vendor name is required."; return; }
        var v = _selected ?? new Vendor();
        v.Name = NameBox.Text.Trim();
        v.Trade = TradeBox.Text.Trim();
        v.Contact = ContactBox.Text.Trim();
        v.CoiExpiry = CoiPicker.Date;
        v.LastJobNote = LastJobBox.Text.Trim();
        v.NextBookedNote = NextJobBox.Text.Trim();
        if (_selected is null) _vendors.Add(v);
        await _store.SaveAsync(_vendors);
        _vendors = _vendors.OrderBy(x => x.Name).ToList();
        VendorList.ItemsSource = _vendors;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        VendorList.SelectedItem = null;
        NameBox.Text = TradeBox.Text = ContactBox.Text = LastJobBox.Text = NextJobBox.Text = string.Empty;
        CoiPicker.Date = null;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _vendors.Remove(_selected);
        await _store.SaveAsync(_vendors);
        _selected = null;
        VendorList.ItemsSource = _vendors;
    }
}
