using ClientPilot.Models;
using ClientPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ClientPilot.Views;

public sealed partial class ProspectsPage : Page
{
    private static readonly string[] PitchTypes =
    {
        "Email pitch",
        "Web form pitch",
        "Phone intro",
        "Referral intro",
        "Other",
    };

    private static readonly string[] Statuses =
    {
        "Pitched",
        "Replied",
        "Call scheduled",
        "Proposal sent",
        "Won",
        "No reply",
        "Not a fit",
        "Closed",
    };

    private readonly JsonListStore<ClientProspect> _store = new("prospects.json");
    private List<ClientProspect> _items = new();
    private ClientProspect? _selected;

    public ProspectsPage()
    {
        this.InitializeComponent();
        PitchTypeCombo.ItemsSource = PitchTypes;
        PitchTypeCombo.SelectedIndex = 0;
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        PitchDatePicker.Date = DateTimeOffset.Now;
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(3);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(p => p.PitchDate).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as ClientProspect;
        if (_selected is null) return;
        CompanyBox.Text = _selected.Company;
        EmailBox.Text = _selected.ContactEmail;
        PitchTypeCombo.SelectedItem = _selected.PitchType;
        PitchDatePicker.Date = _selected.PitchDate;
        StatusCombo.SelectedItem = _selected.Status;
        FollowUpCheck.IsChecked = _selected.FollowUpDate is not null;
        if (_selected.FollowUpDate is { } f) FollowUpPicker.Date = f;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyBox.Text)) return;
        var item = _selected ?? new ClientProspect();
        item.Company = CompanyBox.Text.Trim();
        item.ContactEmail = EmailBox.Text.Trim();
        item.PitchType = PitchTypeCombo.SelectedItem as string ?? PitchTypes[0];
        item.PitchDate = PitchDatePicker.Date;
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.FollowUpDate = FollowUpCheck.IsChecked == true ? FollowUpPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(p => p.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(p => p.Id == _selected.Id);
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
    }

    private void ClearForm()
    {
        CompanyBox.Text = string.Empty;
        EmailBox.Text = string.Empty;
        PitchTypeCombo.SelectedIndex = 0;
        PitchDatePicker.Date = DateTimeOffset.Now;
        StatusCombo.SelectedIndex = 0;
        FollowUpCheck.IsChecked = false;
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(3);
        NotesBox.Text = string.Empty;
    }
}
