namespace TaxPilot.Models;

public sealed class TaxSettings
{
    public string OfficeName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int CurrentTaxYear { get; set; } = DateTime.Now.Year - 1;
    /// <summary>As-of label for the deadline tables - the preparer confirms the dates.</summary>
    public string AsOfLabel { get; set; } = "Federal defaults - CONFIRM before relying (edit in Deadlines)";
    /// <summary>Deadline table lines: "Entity|Due MM-DD|Extension MM-DD" (filing year = tax year + 1).</summary>
    public List<string> DeadlineTable { get; set; } = new()
    {
        "1040|04-15|10-15",
        "1120|04-15|10-15",
        "1120-S|03-15|09-15",
        "1065|03-15|09-15",
        "1120-H|04-15|10-15",
        "990|05-15|11-15",
        "1041|04-15|09-30",
    };
    /// <summary>Document checklist templates: "Entity|Doc|Doc|...".</summary>
    public List<string> DocTemplates { get; set; } = new()
    {
        "1040|W-2|1099s|Prior-year return|ID|Bank letter for direct deposit",
        "1120|Business books|Prior-year return|Bank statements|Payroll reports",
        "1120-S|Business books|K-1 data|Prior-year return|Payroll reports",
        "1065|Partnership books|Partner list|Prior-year return",
        "1120-H|Association books|Prior-year return|Reserve study",
        "990|Organization books|Prior-year return|Board list",
        "1041|Trust/estate documents|1099s|Prior-year return",
    };
    /// <summary>State jurisdictions the preparer maintains personally - free-text lines, no shipped authority.</summary>
    public List<string> StateNotes { get; set; } = new();
    public List<string> EntityTypes { get; set; } = new()
    {
        "1040", "1120", "1120-S", "1065", "1120-H", "990", "1041",
    };
}
