using System.Net;
using System.Text;
using KennelPilot.Models;
using KennelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace KennelPilot.Views;

public sealed partial class CareLogPage : Page
{
    private readonly JsonListStore<CareEntry> _store = new("care.json");
    private readonly JsonListStore<Stay> _stayStore = new("stays.json");
    private readonly JsonListStore<Pet> _petStore = new("pets.json");
    private List<Stay> _stays = new();

    public CareLogPage()
    {
        this.InitializeComponent();
        KindCombo.ItemsSource = new[] { "Feeding", "Water", "Yard / exercise", "Medication", "Health note", "Incident", "Other" };
        KindCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _stays = (await _stayStore.LoadAsync()).Where(s => s.IsActive).OrderBy(s => s.CheckIn).ToList();
        StayPicker.ItemsSource = _stays.Select(s => s.DisplayLine).ToList();
        if (_stays.Count > 0) StayPicker.SelectedIndex = 0;
        await RefreshLogAsync();
    }

    private Stay? PickedStay =>
        StayPicker.SelectedIndex >= 0 && StayPicker.SelectedIndex < _stays.Count
            ? _stays[StayPicker.SelectedIndex]
            : null;

    private async void StayPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var stay = PickedStay;
        PetFileText.Text = string.Empty;
        if (stay is not null)
        {
            var pet = (await _petStore.LoadAsync()).FirstOrDefault(p => p.Id == stay.PetId);
            if (pet is not null)
            {
                PetFileText.Text =
                    $"Feeding: {pet.Feeding}" +
                    (string.IsNullOrWhiteSpace(pet.Medications) ? "" : $"{Environment.NewLine}Meds: {pet.Medications}") +
                    (string.IsNullOrWhiteSpace(pet.Behavior) ? "" : $"{Environment.NewLine}Behavior: {pet.Behavior}");
            }
        }
        await RefreshLogAsync();
    }

    private async Task RefreshLogAsync()
    {
        var all = await _store.LoadAsync();
        var stay = PickedStay;
        ItemsList.ItemsSource = (stay is null ? all : all.Where(c => c.StayId == stay.Id))
            .OrderByDescending(c => c.When).ToList();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var stay = PickedStay;
        if (stay is null || string.IsNullOrWhiteSpace(DetailBox.Text) || string.IsNullOrWhiteSpace(InitialsBox.Text))
        {
            StatusText.Text = "Pick a stay, write what happened, and sign with your initials.";
            return;
        }
        var all = await _store.LoadAsync();
        all.Add(new CareEntry
        {
            StayId = stay.Id,
            StayLabel = stay.DisplayLine,
            When = DateTimeOffset.Now,
            Kind = KindCombo.SelectedItem as string ?? "Other",
            Detail = DetailBox.Text.Trim(),
            StaffInitials = InitialsBox.Text.Trim(),
        });
        await _store.SaveAsync(all);
        DetailBox.Text = string.Empty;
        StatusText.Text = "Entry logged.";
        await RefreshLogAsync();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var stay = PickedStay;
        if (stay is null || App.WindowInstance is null) return;
        var entries = (await _store.LoadAsync()).Where(c => c.StayId == stay.Id).OrderBy(c => c.When).ToList();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"day-sheet-{stay.CheckIn:yyyy-MM-dd}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Day Sheet - {WebUtility.HtmlEncode(stay.PetLabel)}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}</style></head><body>");
        sb.Append($"<h1>Stay Day Sheet</h1><p><strong>{WebUtility.HtmlEncode(stay.PetLabel)}</strong><br>{WebUtility.HtmlEncode(stay.DisplayLine)}</p>");
        sb.Append("<table><tr><th>When</th><th>Kind</th><th>Detail</th><th>Staff</th></tr>");
        foreach (var c in entries)
            sb.Append($"<tr><td>{c.When:MMM d, h:mm tt}</td><td>{WebUtility.HtmlEncode(c.Kind)}</td><td>{WebUtility.HtmlEncode(c.Detail)}</td><td>{WebUtility.HtmlEncode(c.StaffInitials)}</td></tr>");
        sb.Append("</table></body></html>");
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Day sheet exported to {file.Path}";
    }
}
