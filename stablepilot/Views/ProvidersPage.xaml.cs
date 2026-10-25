using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class ProvidersPage : Page
{
    private readonly JsonListStore<Provider> _store = new("providers.json");
    private List<Provider> _providers = new();
    private Provider? _selected;

    public ProvidersPage()
    {
        this.InitializeComponent();
        TradeBox.ItemsSource = new[] { "Vet", "Farrier", "Dentist", "Chiropractor", "Transporter", "Other" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _providers = (await _store.LoadAsync()).OrderBy(p => p.Name).ToList();
        ProviderList.ItemsSource = _providers;
        if (_providers.Count > 0) ProviderList.SelectedIndex = 0;
    }

    private void ProviderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ProviderList.SelectedItem as Provider;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        TradeBox.SelectedItem = _selected.Trade;
        ContactBox.Text = _selected.Contact;
        HorsesBox.Text = _selected.HorsesNote;
        LastBox.Text = _selected.LastVisitNote;
        NextBox.Text = _selected.NextRoundNote;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { ErrorText.Text = "A provider name is required."; return; }
        var p = _selected ?? new Provider();
        p.Name = NameBox.Text.Trim();
        p.Trade = TradeBox.SelectedItem as string ?? p.Trade;
        p.Contact = ContactBox.Text.Trim();
        p.HorsesNote = HorsesBox.Text.Trim();
        p.LastVisitNote = LastBox.Text.Trim();
        p.NextRoundNote = NextBox.Text.Trim();
        if (_selected is null) _providers.Add(p);
        await _store.SaveAsync(_providers);
        _providers = _providers.OrderBy(x => x.Name).ToList();
        ProviderList.ItemsSource = _providers;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ProviderList.SelectedItem = null;
        NameBox.Text = ContactBox.Text = HorsesBox.Text = LastBox.Text = NextBox.Text = string.Empty;
        TradeBox.SelectedItem = "Vet";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _providers.Remove(_selected);
        await _store.SaveAsync(_providers);
        _selected = null;
        ProviderList.ItemsSource = _providers;
    }
}
