namespace ClaimPilot.Models;

/// <summary>Data passed to the PDF service when exporting the lawsuit worksheet.</summary>
public sealed class WorksheetExport
{
    public string PlaintiffName { get; set; } = string.Empty;
    public string PlaintiffAddress { get; set; } = string.Empty;
    public string DefendantName { get; set; } = string.Empty;
    public string DefendantAddress { get; set; } = string.Empty;
    public string AmountSought { get; set; } = string.Empty;
    public List<FactRow> Facts { get; set; } = new();
    public List<string> ChecklistTexts { get; set; } = new();
    public string FilingFeeNote { get; set; } = string.Empty;
}
