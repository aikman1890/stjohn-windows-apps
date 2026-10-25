using DenialPilot.Models;
using DenialPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;

namespace DenialPilot.Views;

public sealed partial class CasesPage : Page
{
    private static readonly string[] Reasons =
    {
        "Administrative",
        "Prior authorization",
        "Medical necessity",
        "Coding",
        "Eligibility",
        "Other",
    };

    private static readonly string[] Statuses =
    {
        "Denied",
        "Info gathered",
        "Level 1 appeal",
        "Level 2 appeal",
        "External review",
        "DOI complaint",
        "Resolved - won",
        "Resolved - lost",
        "Resolved - paid",
    };

    private static readonly string[] Clocks =
    {
        "180-day appeal window",
        "4-month external review window",
        "Custom days",
    };

    private readonly JsonListStore<DenialCase> _store = new("cases.json");
    private List<DenialCase> _items = new();
    private DenialCase? _selected;

    public CasesPage()
    {
        this.InitializeComponent();
        ReasonCombo.ItemsSource = Reasons;
        ReasonCombo.SelectedIndex = 0;
        StatusCombo.ItemsSource = Statuses;
        StatusCombo.SelectedIndex = 0;
        ClockCombo.ItemsSource = Clocks;
        ClockCombo.SelectedIndex = 0;
        ServiceDatePicker.Date = DateTimeOffset.Now;
        DenialDatePicker.Date = DateTimeOffset.Now;
        AppealSentPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await SampleSeed.EnsureAsync(_store);
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderBy(c => c.AppealDeadline).ToList();
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as DenialCase;
        if (_selected is null) return;
        PatientBox.Text = _selected.PatientName;
        InsurerBox.Text = _selected.Insurer;
        PlanBox.Text = _selected.Plan;
        ClaimBox.Text = _selected.ClaimNumber;
        ServiceDatePicker.Date = _selected.ServiceDate;
        ProviderBox.Text = _selected.Provider;
        AmountBox.Text = _selected.AmountDenied;
        DenialDatePicker.Date = _selected.DenialDate;
        ReasonCombo.SelectedItem = _selected.ReasonCategory;
        StatusCombo.SelectedItem = _selected.Status;
        ClockCombo.SelectedItem = _selected.ClockPreset;
        CustomDaysBox.Text = _selected.CustomDays.ToString();
        AppealSentCheck.IsChecked = _selected.AppealSentDate is not null;
        if (_selected.AppealSentDate is { } s) AppealSentPicker.Date = s;
        UrgentCheck.IsChecked = _selected.UrgentClock;
        DocumentsBox.Text = _selected.Documents;
        NotesBox.Text = _selected.Notes;
    }

    private async void AddDocButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.WindowInstance is null) return;
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        DocumentsBox.Text = string.IsNullOrWhiteSpace(DocumentsBox.Text)
            ? file.Path
            : DocumentsBox.Text.TrimEnd() + Environment.NewLine + file.Path;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InsurerBox.Text)) return;
        var item = _selected ?? new DenialCase();
        item.PatientName = PatientBox.Text.Trim();
        item.Insurer = InsurerBox.Text.Trim();
        item.Plan = PlanBox.Text.Trim();
        item.ClaimNumber = ClaimBox.Text.Trim();
        item.ServiceDate = ServiceDatePicker.Date;
        item.Provider = ProviderBox.Text.Trim();
        item.AmountDenied = AmountBox.Text.Trim();
        item.DenialDate = DenialDatePicker.Date;
        item.ReasonCategory = ReasonCombo.SelectedItem as string ?? Reasons[0];
        item.Status = StatusCombo.SelectedItem as string ?? Statuses[0];
        item.ClockPreset = ClockCombo.SelectedItem as string ?? Clocks[0];
        item.CustomDays = int.TryParse(CustomDaysBox.Text.Trim(), out var days) && days > 0 ? days : 180;
        item.AppealSentDate = AppealSentCheck.IsChecked == true ? AppealSentPicker.Date : null;
        item.UrgentClock = UrgentCheck.IsChecked == true;
        item.Documents = DocumentsBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        ItemsList.SelectedItem = _items.FirstOrDefault(c => c.Id == item.Id);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(c => c.Id == _selected.Id);
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
        PatientBox.Text = string.Empty;
        InsurerBox.Text = string.Empty;
        PlanBox.Text = string.Empty;
        ClaimBox.Text = string.Empty;
        ServiceDatePicker.Date = DateTimeOffset.Now;
        ProviderBox.Text = string.Empty;
        AmountBox.Text = string.Empty;
        DenialDatePicker.Date = DateTimeOffset.Now;
        ReasonCombo.SelectedIndex = 0;
        StatusCombo.SelectedIndex = 0;
        ClockCombo.SelectedIndex = 0;
        CustomDaysBox.Text = "180";
        AppealSentCheck.IsChecked = false;
        AppealSentPicker.Date = DateTimeOffset.Now;
        UrgentCheck.IsChecked = false;
        DocumentsBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
    }
}
