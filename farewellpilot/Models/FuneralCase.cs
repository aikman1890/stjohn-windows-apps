namespace FarewellPilot.Models;

public sealed class ChecklistItem
{
    public string Label { get; set; } = string.Empty;
    public bool Done { get; set; }
    public string ConfirmedBy { get; set; } = string.Empty;
    public DateTimeOffset? ConfirmedDate { get; set; }

    public string DisplayLine =>
        $"{(Done ? "[done]" : "[    ]")} {Label}" +
        (Done && !string.IsNullOrWhiteSpace(ConfirmedBy) ? $" - {ConfirmedBy} {ConfirmedDate:MMM d}" : string.Empty);
}

public sealed class DocRef
{
    public string Kind { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string DisplayLine => $"{Kind} - {Path}" + (string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}");
}

public sealed class FuneralCase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CaseNumber { get; set; } = string.Empty;
    public string DecedentName { get; set; } = string.Empty;
    public DateTimeOffset? DateOfDeath { get; set; }
    public string NextOfKin { get; set; } = string.Empty;
    public string KinRelationship { get; set; } = string.Empty;
    public string KinPhone { get; set; } = string.Empty;
    public string KinEmail { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Burial";
    public string Status { get; set; } = "First call";
    public DateTimeOffset FirstCallDate { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ServiceDate { get; set; }
    public string ServiceLocation { get; set; } = string.Empty;
    public bool IsVeteran { get; set; }
    public List<ChecklistItem> Checklist { get; set; } = new();
    public List<DocRef> Documents { get; set; } = new();
    public DateTimeOffset? DcFiledDate { get; set; }
    public string PermitNote { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayLine =>
        $"{CaseNumber} - {DecedentName} - {ServiceType} - {Status}" +
        (ServiceDate is { } sd ? $" - service {sd:MMM d}" : string.Empty);
}
