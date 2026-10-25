using System.Globalization;
using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GuardPilot.Views;

public sealed partial class SitesPage : Page
{
    private readonly JsonListStore<ClientSite> _store = new("sites.json");
    private readonly JsonListStore<Post> _postStore = new("posts.json");
    private List<ClientSite> _items = new();
    private ClientSite? _selected;

    public SitesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private async void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(s => s.Client).ToList();
        if (_selected is not null)
        {
            var posts = await _postStore.LoadAsync();
            PostsList.ItemsSource = posts.Where(p => p.SiteId == _selected.Id).ToList();
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ClientSite;
        if (_selected is null) return;
        ClientBox.Text = _selected.Client;
        SiteBox.Text = _selected.SiteName;
        AddressBox.Text = _selected.Address;
        ContractBox.Text = _selected.ContractRef;
        RateBox.Text = _selected.BillRatePerHour.ToString(CultureInfo.InvariantCulture);
        TermsBox.Text = _selected.BillingTerms;
        ContactBox.Text = _selected.SiteContact;
        NotesBox.Text = _selected.Notes;
        RefreshList();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new ClientSite();
        item.Client = ClientBox.Text.Trim();
        item.SiteName = SiteBox.Text.Trim();
        item.Address = AddressBox.Text.Trim();
        item.ContractRef = ContractBox.Text.Trim();
        item.BillRatePerHour = decimal.TryParse(RateBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : 0m;
        item.BillingTerms = TermsBox.Text.Trim();
        item.SiteContact = ContactBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(s => s.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
        PostsList.ItemsSource = null;
    }

    private void ClearForm()
    {
        ClientBox.Text = string.Empty;
        SiteBox.Text = string.Empty;
        AddressBox.Text = string.Empty;
        ContractBox.Text = string.Empty;
        RateBox.Text = string.Empty;
        TermsBox.Text = string.Empty;
        ContactBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        PostBox.Text = string.Empty;
    }

    private async void PostButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || string.IsNullOrWhiteSpace(PostBox.Text)) return;
        var posts = await _postStore.LoadAsync();
        posts.Add(new Post { SiteId = _selected.Id, SiteLabel = _selected.DisplayLine, Name = PostBox.Text.Trim() });
        await _postStore.SaveAsync(posts);
        PostBox.Text = string.Empty;
        RefreshList();
    }
}
