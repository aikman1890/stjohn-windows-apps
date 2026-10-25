using FarewellPilot.Models;
using FarewellPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace FarewellPilot.Views;

public sealed partial class CasesPage : Page
{
    private readonly JsonListStore<FuneralCase> _store = new("cases.json");
    private readonly JsonListStore<AftercareTask> _aftercareStore = new("aftercare.json");
    private readonly FarewellSettingsStore _settingsStore = new();
    private List<FuneralCase> _items = new();
    private FuneralCase? _selected;
    private List<ChecklistItem> _editChecklist = new();
    private List<DocRef> _editDocs = new();

    public CasesPage()
    {
        this.InitializeComponent();
        TypeCombo.ItemsSource = new[] { "Burial", "Cremation", "Memorial", "Graveside" };
        TypeCombo.SelectedIndex = 0;
        StatusCombo.ItemsSource = new[]
        {
            "First call", "Arrangements", "Paperwork pending", "Service scheduled",
            "Service complete", "Aftercare", "Closed",
        };
        StatusCombo.SelectedIndex = 0;
        DodPicker.Date = DateTimeOffset.Now;
        ServicePicker.Date = DateTimeOffset.Now.AddDays(3);
        DcPicker.Date = DateTimeOffset.Now;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(c => c.FirstCallDate).ToList();
    }

    private void RefreshChecklist()
    {
        ChecklistList.ItemsSource = null;
        ChecklistList.ItemsSource = _editChecklist;
    }

    private void RefreshDocs()
    {
        DocsList.ItemsSource = null;
        DocsList.ItemsSource = _editDocs;
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as FuneralCase;
        if (_selected is null) return;
        DecedentBox.Text = _selected.DecedentName;
        DodCheck.IsChecked = _selected.DateOfDeath is not null;
        if (_selected.DateOfDeath is { } dod) DodPicker.Date = dod;
        KinBox.Text = _selected.NextOfKin;
        KinRelBox.Text = _selected.KinRelationship;
        KinPhoneBox.Text = _selected.KinPhone;
        KinEmailBox.Text = _selected.KinEmail;
        TypeCombo.SelectedItem = _selected.ServiceType;
        StatusCombo.SelectedItem = _selected.Status;
        ServiceCheck.IsChecked = _selected.ServiceDate is not null;
        if (_selected.ServiceDate is { } sd) ServicePicker.Date = sd;
        LocationBox.Text = _selected.ServiceLocation;
        VeteranCheck.IsChecked = _selected.IsVeteran;
        DcCheck.IsChecked = _selected.DcFiledDate is not null;
        if (_selected.DcFiledDate is { } dc) DcPicker.Date = dc;
        PermitBox.Text = _selected.PermitNote;
        NotesBox.Text = _selected.Notes;
        _editChecklist = _selected.Checklist.Select(i => new ChecklistItem
        {
            Label = i.Label, Done = i.Done, ConfirmedBy = i.ConfirmedBy, ConfirmedDate = i.ConfirmedDate,
        }).ToList();
        _editDocs = _selected.Documents.Select(d => new DocRef { Kind = d.Kind, Path = d.Path, Note = d.Note }).ToList();
        RefreshChecklist();
        RefreshDocs();
    }

    private void ToggleItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (ChecklistList.SelectedItem is not ChecklistItem item) return;
        item.Done = !item.Done;
        item.ConfirmedBy = item.Done ? InitialsBox.Text.Trim() : string.Empty;
        item.ConfirmedDate = item.Done ? DateTimeOffset.Now : null;
        RefreshChecklist();
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomItemBox.Text)) return;
        _editChecklist.Add(new ChecklistItem { Label = CustomItemBox.Text.Trim() });
        CustomItemBox.Text = string.Empty;
        RefreshChecklist();
    }

    private async void PickDocButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.WindowInstance is null) return;
        var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file is not null) DocPathBox.Text = file.Path;
    }

    private void AddDocButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DocPathBox.Text)) return;
        _editDocs.Add(new DocRef
        {
            Kind = string.IsNullOrWhiteSpace(DocKindBox.Text) ? "Document" : DocKindBox.Text.Trim(),
            Path = DocPathBox.Text.Trim(),
        });
        DocKindBox.Text = string.Empty;
        DocPathBox.Text = string.Empty;
        RefreshDocs();
    }

    private void RemoveDocButton_Click(object sender, RoutedEventArgs e)
    {
        if (DocsList.SelectedItem is DocRef doc)
        {
            _editDocs.Remove(doc);
            RefreshDocs();
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var isNew = _selected is null;
        var item = _selected ?? new FuneralCase
        {
            CaseNumber = $"{DateTime.Now.Year}-{_items.Count + 1:000}",
            FirstCallDate = DateTimeOffset.Now,
        };
        var oldStatus = item.Status;
        item.DecedentName = DecedentBox.Text.Trim();
        item.DateOfDeath = DodCheck.IsChecked == true ? DodPicker.Date : null;
        item.NextOfKin = KinBox.Text.Trim();
        item.KinRelationship = KinRelBox.Text.Trim();
        item.KinPhone = KinPhoneBox.Text.Trim();
        item.KinEmail = KinEmailBox.Text.Trim();
        item.ServiceType = TypeCombo.SelectedItem as string ?? "Burial";
        item.Status = StatusCombo.SelectedItem as string ?? "First call";
        item.ServiceDate = ServiceCheck.IsChecked == true ? ServicePicker.Date : null;
        item.ServiceLocation = LocationBox.Text.Trim();
        item.IsVeteran = VeteranCheck.IsChecked == true;
        item.DcFiledDate = DcCheck.IsChecked == true ? DcPicker.Date : null;
        item.PermitNote = PermitBox.Text.Trim();
        item.Notes = NotesBox.Text.Trim();
        if (isNew && _editChecklist.Count == 0)
            _editChecklist = CaseMath.BuildChecklist(settings, item.ServiceType);
        item.Checklist = _editChecklist;
        item.Documents = _editDocs;
        if (isNew) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;

        // Aftercare cadence: schedule follow-ups when a case completes its service.
        if (item.Status is "Service complete" or "Aftercare" or "Closed" && oldStatus != item.Status)
        {
            var tasks = await _aftercareStore.LoadAsync();
            var anchor = item.ServiceDate ?? DateTimeOffset.Now;
            void AddTask(string kind, DateTimeOffset due)
            {
                if (!tasks.Any(t => t.CaseId == item.Id && t.Kind == kind))
                    tasks.Add(new AftercareTask { CaseId = item.Id, CaseLabel = item.DisplayLine, Kind = kind, DueDate = due });
            }
            AddTask("1 week follow-up", anchor.AddDays(7));
            AddTask("1 month follow-up", anchor.AddMonths(1));
            AddTask("Anniversary follow-up", anchor.AddYears(1));
            await _aftercareStore.SaveAsync(tasks);
        }
        RefreshList();
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
        DecedentBox.Text = string.Empty;
        KinBox.Text = string.Empty;
        KinRelBox.Text = string.Empty;
        KinPhoneBox.Text = string.Empty;
        KinEmailBox.Text = string.Empty;
        TypeCombo.SelectedIndex = 0;
        StatusCombo.SelectedIndex = 0;
        ServiceCheck.IsChecked = false;
        LocationBox.Text = string.Empty;
        VeteranCheck.IsChecked = false;
        DcCheck.IsChecked = false;
        DodCheck.IsChecked = false;
        PermitBox.Text = string.Empty;
        NotesBox.Text = string.Empty;
        _editChecklist = new List<ChecklistItem>();
        _editDocs = new List<DocRef>();
        RefreshChecklist();
        RefreshDocs();
    }
}
