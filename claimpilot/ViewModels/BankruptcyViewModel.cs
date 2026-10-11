using System.Collections.ObjectModel;
using ClaimPilot.Data;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Bankruptcy page: Chapter 7 and Chapter 13 guides, document checklists,
/// the required-courses tracker (credit counseling + debtor education),
/// bankruptcy deadline presets, and the Bankruptcy Plus unlock gate.
/// Everything on this page is general information — never legal advice.
/// </summary>
public partial class BankruptcyViewModel : ObservableObject
{
    public BankruptcyGuide Chapter7Guide { get; } = BankruptcyGuides.GetChapter7Guide();

    public BankruptcyGuide Chapter13Guide { get; } = BankruptcyGuides.GetChapter13Guide();

    public ObservableCollection<HonestNote> HonestNotes { get; } = new(BankruptcyGuides.GetHonestNotes());

    public ObservableCollection<ChecklistItem> Chapter7Checklist { get; } = new(
        BankruptcyGuides.GetChapter7Checklist().Select(t => new ChecklistItem(t)));

    public ObservableCollection<ChecklistItem> Chapter13Checklist { get; } = new(
        BankruptcyGuides.GetChapter13Checklist().Select(t => new ChecklistItem(t)));

    public ObservableCollection<CourseRecord> CourseRecords { get; } = new();

    public IEnumerable<CourseRecord> Chapter7Courses => CourseRecords.Where(c => c.Chapter == 7);

    public IEnumerable<CourseRecord> Chapter13Courses => CourseRecords.Where(c => c.Chapter == 13);

    [ObservableProperty]
    private bool _isBankruptcyPlusUnlocked;

    [ObservableProperty]
    private bool _isCheckingOwnership = true;

    [ObservableProperty]
    private bool _isPurchasing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _plannedFilingDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private DateTimeOffset _deadlineSeedFilingDate = DateTimeOffset.Now.Date;

    public Visibility LockedVisibility => IsBankruptcyPlusUnlocked ? Visibility.Collapsed : Visibility.Visible;

    public Visibility UnlockedVisibility => IsBankruptcyPlusUnlocked ? Visibility.Visible : Visibility.Collapsed;

    public string UnlockPriceDisplay => Pricing.BankruptcyPlusPriceDisplay;

    public string UnlockBlurb => Pricing.BankruptcyPlusBlurb;

    public string DisclaimerText => LegalText.Disclaimer + " " + LegalText.NeverFilesNote;

    public string IntroText =>
        "General information about Chapter 7 and Chapter 13 bankruptcy, document checklists, " +
        "and a tracker for the two courses the law requires. Nothing here is legal advice. " +
        "Bankruptcy has serious, long-lasting consequences — consider talking to a bankruptcy attorney before you file anything.";

    public string CourseHonestyText =>
        "IMPORTANT: ClaimPilot does NOT issue course certificates and cannot satisfy the course requirement. " +
        "Only a U.S. Trustee-approved provider can issue a valid certificate, generated through the Trustee's Certificate Generation System. " +
        "This tracker only records what YOU completed with a real provider. Use the official U.S. Trustee approved-provider lists below to find one.";

    public string CreditCounselingProvidersUrl =>
        "https://www.justice.gov/ust/list-credit-counseling-agencies-approved-pursuant-11-usc-111";

    public string DebtorEducationProvidersUrl =>
        "https://www.justice.gov/ust/list-approved-providers-personal-financial-management-instructional-courses-debtor-education";

    public string CounselingWindowText
    {
        get
        {
            var filing = PlannedFilingDate.Date;
            var earliest = filing.AddDays(-180);
            return $"If you plan to file on {filing:MMMM d, yyyy}, your pre-filing credit counseling must be completed " +
                   $"between {earliest:MMMM d, yyyy} and {filing:MMMM d, yyyy} (within 180 days before filing). " +
                   $"Complete it even one day too early and it does not count — the case can be dismissed.";
        }
    }

    public async Task LoadAsync()
    {
        await CheckOwnershipAsync();
        try
        {
            await DatabaseService.Instance.EnsureCourseRecordsSeededAsync();
            var records = await DatabaseService.Instance.GetCourseRecordsAsync();
            CourseRecords.Clear();
            foreach (var record in records)
            {
                CourseRecords.Add(record);
            }
            OnPropertyChanged(nameof(Chapter7Courses));
            OnPropertyChanged(nameof(Chapter13Courses));
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not load course records: " + ex.Message;
        }
    }

    partial void OnPlannedFilingDateChanged(DateTimeOffset value)
    {
        OnPropertyChanged(nameof(CounselingWindowText));
    }

    partial void OnIsBankruptcyPlusUnlockedChanged(bool value)
    {
        OnPropertyChanged(nameof(LockedVisibility));
        OnPropertyChanged(nameof(UnlockedVisibility));
    }

    [RelayCommand]
    private async Task CheckOwnershipAsync()
    {
        IsCheckingOwnership = true;
        try
        {
            IsBankruptcyPlusUnlocked = await PurchaseService.IsBankruptcyPlusOwnedAsync();
        }
        finally
        {
            IsCheckingOwnership = false;
        }
    }

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        if (IsPurchasing)
        {
            return;
        }

        IsPurchasing = true;
        StatusMessage = "Opening the Microsoft Store purchase…";
        try
        {
            var (success, message) = await PurchaseService.PurchaseBankruptcyPlusAsync();
            StatusMessage = message;
            if (success)
            {
                IsBankruptcyPlusUnlocked = true;
            }
        }
        finally
        {
            IsPurchasing = false;
        }
    }

    public async Task SaveCourseAsync(CourseRecord record)
    {
        if (record is null)
        {
            return;
        }

        try
        {
            await DatabaseService.Instance.UpdateCourseRecordAsync(record);
            record.RefreshDerived();
            StatusMessage = "Course record saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the course record: " + ex.Message;
        }
    }

    public async Task ResetCourseAsync(CourseRecord record)
    {
        if (record is null)
        {
            return;
        }

        try
        {
            await DatabaseService.Instance.ResetCourseRecordAsync(record);
            record.Status = "NotStarted";
            record.ScheduledDate = DateTime.Today;
            record.CompletedDate = DateTime.Today;
            record.ProviderName = string.Empty;
            record.CertificateFileName = string.Empty;
            record.RefreshDerived();
            StatusMessage = "Course record reset.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not reset the course record: " + ex.Message;
        }
    }

    /// <summary>Copies the provider-issued certificate into the local vault.</summary>
    public async Task AttachCertificateAsync(CourseRecord record)
    {
        if (record is null || App.MainWindow is null)
        {
            return;
        }

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                StatusMessage = "Certificate attach canceled.";
                return;
            }

            var vault = await ApplicationData.Current.LocalFolder.CreateFolderAsync("certificates", CreationCollisionOption.OpenIfExists);
            var safeName = $"ch{record.Chapter}_{record.CourseType}_{DateTime.Now:yyyyMMddHHmmss}{file.FileType}";
            await file.CopyAsync(vault, safeName, NameCollisionOption.ReplaceExisting);

            if (!string.IsNullOrEmpty(record.CertificateFileName))
            {
                try
                {
                    var old = await vault.GetFileAsync(record.CertificateFileName);
                    await old.DeleteAsync();
                }
                catch
                {
                    // Old file already gone — nothing to clean up.
                }
            }

            record.CertificateFileName = safeName;
            record.Status = "Completed";
            record.CompletedDate = DateTime.Today;
            await DatabaseService.Instance.UpdateCourseRecordAsync(record);
            record.RefreshDerived();
            StatusMessage = $"Certificate saved to the vault: {safeName}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the certificate: " + ex.Message;
        }
    }

    public async Task OpenCertificateAsync(CourseRecord record)
    {
        if (record is null || string.IsNullOrEmpty(record.CertificateFileName))
        {
            return;
        }

        try
        {
            var vault = await ApplicationData.Current.LocalFolder.GetFolderAsync("certificates");
            var file = await vault.GetFileAsync(record.CertificateFileName);
            await Windows.System.Launcher.LaunchFileAsync(file);
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not open the certificate: " + ex.Message;
        }
    }

    /// <summary>Seeds the standard deadline presets for a chapter into the Deadlines tracker.</summary>
    [RelayCommand]
    private async Task AddChapterDeadlinesAsync(string chapter)
    {
        try
        {
            var filing = DeadlineSeedFilingDate.Date;
            var presets = chapter == "13"
                ? BankruptcyGuides.GetChapter13DeadlinePresets()
                : BankruptcyGuides.GetChapter7DeadlinePresets();

            var existing = await DatabaseService.Instance.GetDeadlinesAsync();
            var existingTitles = new HashSet<string>(existing.Select(d => d.Title));

            int added = 0;
            foreach (var (title, notes, dueFromFiling) in presets)
            {
                if (existingTitles.Contains(title))
                {
                    continue;
                }

                await DatabaseService.Instance.AddDeadlineAsync(new DeadlineItem
                {
                    Title = title,
                    Notes = notes + " Seeded from the Bankruptcy section on " + DateTime.Today.ToString("MMMM d, yyyy") + ". Verify every date against your case — presets are estimates.",
                    DueDate = dueFromFiling(filing),
                    Category = "Bankruptcy"
                });
                added++;
            }

            StatusMessage = added > 0
                ? $"{added} bankruptcy deadlines added to the Deadlines tracker. Adjust each date to match your actual case."
                : "Those bankruptcy deadlines are already in the tracker.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not add deadlines: " + ex.Message;
        }
    }
}
