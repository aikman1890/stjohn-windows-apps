using JobPilot.Models;
using JobPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace JobPilot.Views;

public sealed partial class ApplicationsPage : Page
{
    private static readonly string[] Statuses =
    {
        "Applied",
        "Recruiter screen",
        "Interview",
        "Offer",
        "Rejected",
        "Accepted",
        "Withdrawn",
    };

    private readonly JsonListStore<JobApplication> _store = new("applications.json");
    private List<JobApplication> _items = new();
    private JobApplication? _selected;

    public ApplicationsPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        AppliedDatePicker.Date = DateTimeOffset.Now;
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
        ItemsList.ItemsSource = _items.OrderByDescending(a => a.DateApplied).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as JobApplication;
        if (_selected is null) return;
        CompanyBox.Text = _selected.Company;
        RoleBox.Text = _selected.Role;
        RateBox.Text = _selected.Rate;
        StatusCombo.SelectedItem = _selected.Status;
        AppliedDatePicker.Date = _selected.DateApplied;
        ConfirmationBox.Text = _selected.Confirmation;
        ContactBox.Text = _selected.Contact;
        FollowUpCheck.IsChecked = _selected.FollowUpDate is not null;
        if (_selected.FollowUpDate is { } f) FollowUpPicker.Date = f;
        NotesBox.Text = _selected.Notes;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyBox.Text)) return;
        var item = _selected ?? new JobApplication();
        item.Company = CompanyBox.Text.Trim();
        item.Role = RoleBox.Text.Trim();
        item.Rate = RateBox.Text.Trim();
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.DateApplied = AppliedDatePicker.Date;
        item.Confirmation = ConfirmationBox.Text.Trim();
        item.Contact = ContactBox.Text.Trim();
        item.FollowUpDate = FollowUpCheck.IsChecked == true ? FollowUpPicker.Date : null;
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(a => a.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(a => a.Id == _selected.Id);
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
        RoleBox.Text = string.Empty;
        RateBox.Text = string.Empty;
        StatusCombo.SelectedIndex = 0;
        AppliedDatePicker.Date = DateTimeOffset.Now;
        ConfirmationBox.Text = string.Empty;
        ContactBox.Text = string.Empty;
        FollowUpCheck.IsChecked = false;
        FollowUpPicker.Date = DateTimeOffset.Now.AddDays(3);
        NotesBox.Text = string.Empty;
    }
}
