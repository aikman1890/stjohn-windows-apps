using System.Text.Json;
using LienPilot.Models;
using LienPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace LienPilot.Views;

public sealed partial class TemplatesPage : Page
{
    private sealed record TemplateDef(string Name, string LedgerStep, string Body);

    private static readonly TemplateDef[] Templates =
    {
        new("First notice of default", "Default notice",
            "Dear [Tenant Name],\n\nOur records show rent for Unit [Unit Number] at [Facility Name], [Facility Address] is past due. Amount owed: [Total Claim] (rent and fees itemized in the attached statement), paid through [Paid Through].\n\nPlease pay in full or contact us at [Facility Phone] to discuss the account. If the balance remains unpaid, we will proceed under our state's self-storage lien law and your rental agreement, which may include a lien on the stored property and its sale.\n\n[FACILITY NAME]\n[DATE]"),
        new("Lien notice", "Lien notice",
            "NOTICE OF LIEN - [Facility Name], [Facility Address]\n\nTenant: [Tenant Name], [Tenant Address]\nUnit: [Unit Number]\n\nYou are in default. Total claim: [Total Claim], itemized in the attached statement, for rent and fees through [Paid Through]. A lien is claimed on the property stored in the unit under state law and your rental agreement.\n\nUnless the claim is paid in full, the property will be advertised for sale and sold after [Sale Date]. You may stop the sale by paying the full claim before the sale date. Contact [Facility Phone].\n\n[FACILITY NAME]\n[DATE]"),
        new("Sale advertisement", "Advertisement week 1",
            "PUBLIC SALE - [Facility Name], [Facility Address]\n\nTo satisfy a landlord's lien, the contents of the following unit(s) will be sold at public sale on [Sale Date] at [Facility Address]:\n\nUnit [Unit Number] - [Tenant Name] - general household goods and personal property (general description per facility records).\n\nTerms: cash, sold as-is, facility reserves the right to bid and to cancel any sale on payment in full. [Facility Phone]."),
    };

    private readonly JsonListStore<StorageUnit> _unitStore = new("units.json");
    private readonly JsonListStore<LienLedgerEntry> _ledgerStore = new("ledger.json");
    private readonly JsonListStore<ClaimLine> _lineStore = new("claimlines.json");
    private readonly FacilitySettingsStore _settingsStore = new();
    private List<StorageUnit> _units = new();
    private Dictionary<string, string> _overrides = new();

    public TemplatesPage()
    {
        this.InitializeComponent();
        MethodCombo.ItemsSource = new[] { "Verified mail", "Certified mail", "Email", "Hand delivery", "Publication", "Posting", "Other" };
        MethodCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _units = await _unitStore.LoadAsync();
        UnitPicker.ItemsSource = _units.Select(u => u.DisplayLine).ToList();
        if (_units.Count > 0) UnitPicker.SelectedIndex = 0;
        TemplatePicker.ItemsSource = Templates.Select(t => t.Name).ToList();
        TemplatePicker.SelectedIndex = 0;
        await LoadOverridesAsync();
        RefreshBody();
    }

    private static string OverrideFile => "template-overrides.json";

    private async Task LoadOverridesAsync()
    {
        try
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(OverrideFile);
            if (item is StorageFile file)
            {
                var json = await FileIO.ReadTextAsync(file);
                _overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }
        catch { _overrides = new(); }
    }

    private async Task SaveOverridesAsync()
    {
        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            OverrideFile, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(_overrides));
    }

    private TemplateDef? Current =>
        TemplatePicker.SelectedIndex >= 0 && TemplatePicker.SelectedIndex < Templates.Length
            ? Templates[TemplatePicker.SelectedIndex]
            : null;

    private StorageUnit? PickedUnit =>
        UnitPicker.SelectedIndex >= 0 && UnitPicker.SelectedIndex < _units.Count
            ? _units[UnitPicker.SelectedIndex]
            : null;

    private void Pickers_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshBody();

    private async void RefreshBody()
    {
        if (Current is not { } t) return;
        var raw = _overrides.TryGetValue(t.Name, out var o) ? o : t.Body;
        var unit = PickedUnit;
        var settings = await _settingsStore.LoadAsync();
        var lines = await _lineStore.LoadAsync();
        var claim = unit is null ? 0m : unit.Balance + lines.Where(l => l.UnitId == unit.Id).Sum(l => l.Amount);
        BodyBox.Text = raw
            .Replace("[Tenant Name]", unit?.TenantName ?? "[Tenant Name]")
            .Replace("[Tenant Address]", unit?.TenantAddress ?? "[Tenant Address]")
            .Replace("[Unit Number]", unit?.UnitNumber ?? "[Unit Number]")
            .Replace("[Facility Name]", string.IsNullOrWhiteSpace(settings.FacilityName) ? "[Facility Name]" : settings.FacilityName)
            .Replace("[Facility Address]", string.IsNullOrWhiteSpace(settings.Address) ? "[Facility Address]" : settings.Address)
            .Replace("[Facility Phone]", string.IsNullOrWhiteSpace(settings.Phone) ? "[Facility Phone]" : settings.Phone)
            .Replace("[Total Claim]", claim.ToString("C"))
            .Replace("[Paid Through]", unit?.PaidThrough is { } pt ? pt.ToString("MMMM d, yyyy") : "[Paid Through]")
            .Replace("[Sale Date]", unit?.SaleDate is { } sd ? sd.ToString("MMMM d, yyyy") : "[Sale Date]")
            .Replace("[DATE]", DateTime.Now.ToString("MMMM d, yyyy"));
    }

    private async void SaveTextButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } t) return;
        // Note: saves the merged text for this unit context as the template override.
        // Users typically edit tokens intact; the raw token text is stored.
        _overrides[t.Name] = BodyBox.Text;
        await SaveOverridesAsync();
        StatusText.Text = $"Saved as the working text for \"{t.Name}\". Tokens already merged for this unit stay merged - reset to restore the token version.";
    }

    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } t) return;
        _overrides.Remove(t.Name);
        await SaveOverridesAsync();
        RefreshBody();
        StatusText.Text = "Template reset to the built-in draft.";
    }

    private async void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } t || PickedUnit is not { } unit) return;
        var ledger = await _ledgerStore.LoadAsync();
        ledger.Add(new LienLedgerEntry
        {
            UnitId = unit.Id,
            UnitLabel = unit.DisplayLine,
            StepType = t.LedgerStep,
            Date = DateTimeOffset.Now,
            Method = MethodCombo.SelectedItem as string ?? "Other",
            Tracking = TrackingBox.Text.Trim(),
            Note = $"Sent from template \"{t.Name}\"",
        });
        await _ledgerStore.SaveAsync(ledger);
        StatusText.Text = $"Recorded in the proof ledger: {t.LedgerStep} for unit {unit.UnitNumber}, {DateTime.Now:MMM d, yyyy}.";
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } t || App.WindowInstance is null) return;
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = t.Name.Replace(' ', '-').ToLowerInvariant(),
        };
        picker.FileTypeChoices.Add("Text document", new List<string> { ".txt" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, BodyBox.Text);
        StatusText.Text = $"Exported to {file.Path}";
    }
}
