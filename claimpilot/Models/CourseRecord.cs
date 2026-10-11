using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.Models;

/// <summary>
/// One required bankruptcy course being tracked. Stored in SQLite; the
/// certificate file itself is copied into LocalFolder\certificates.
/// ClaimPilot never issues certificates — only U.S. Trustee-approved
/// providers can. This record only tracks YOUR progress.
/// </summary>
public partial class CourseRecord : ObservableObject
{
    public long Id { get; set; }

    /// <summary>7 or 13 — which chapter this course belongs to.</summary>
    [ObservableProperty]
    private int _chapter;

    /// <summary>"CreditCounseling" (pre-filing) or "DebtorEducation" (pre-discharge).</summary>
    [ObservableProperty]
    private string _courseType = string.Empty;

    /// <summary>"NotStarted", "Scheduled", or "Completed".</summary>
    [ObservableProperty]
    private string _status = "NotStarted";

    [ObservableProperty]
    private DateTime _scheduledDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _completedDate = DateTime.Today;

    [ObservableProperty]
    private string _providerName = string.Empty;

    /// <summary>File name of the certificate copy in LocalFolder\certificates.</summary>
    [ObservableProperty]
    private string _certificateFileName = string.Empty;

    public string DisplayName => CourseType == "CreditCounseling"
        ? "Pre-filing credit counseling"
        : "Pre-discharge debtor education";

    public string StatusDisplay => Status switch
    {
        "Completed" => "Completed",
        "Scheduled" => "Scheduled for " + ScheduledDate.ToString("MMMM d, yyyy"),
        _ => "Not started"
    };

    /// <summary>Friendly label for the status ComboBox (two-way).</summary>
    public string StatusLabel
    {
        get => Status switch
        {
            "Completed" => "Completed",
            "Scheduled" => "Scheduled",
            _ => "Not started"
        };
        set
        {
            Status = value == "Completed" ? "Completed" : value == "Scheduled" ? "Scheduled" : "NotStarted";
            RefreshDerived();
        }
    }

    /// <summary>DatePicker-friendly wrappers (DatePicker.Date is DateTimeOffset).</summary>
    public DateTimeOffset ScheduledDateOffset
    {
        get => new DateTimeOffset(ScheduledDate);
        set
        {
            ScheduledDate = value.Date;
            OnPropertyChanged(nameof(ScheduledDateOffset));
            OnPropertyChanged(nameof(StatusDisplay));
        }
    }

    public DateTimeOffset CompletedDateOffset
    {
        get => new DateTimeOffset(CompletedDate);
        set
        {
            CompletedDate = value.Date;
            OnPropertyChanged(nameof(CompletedDateOffset));
        }
    }

    public Microsoft.UI.Xaml.Visibility CertificateVisibility =>
        string.IsNullOrEmpty(CertificateFileName)
            ? Microsoft.UI.Xaml.Visibility.Collapsed
            : Microsoft.UI.Xaml.Visibility.Visible;

    /// <summary>Raises change notifications for every derived display property.</summary>
    public void RefreshDerived()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusLabel));
        OnPropertyChanged(nameof(StatusDisplay));
        OnPropertyChanged(nameof(CertificateFileName));
        OnPropertyChanged(nameof(CertificateVisibility));
    }
}
