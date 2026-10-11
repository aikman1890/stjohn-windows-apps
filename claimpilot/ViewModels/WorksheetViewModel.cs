using System.Collections.ObjectModel;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Lawsuit Worksheet page view model: the small-claims worksheet (parties,
/// facts timeline, amount, petition checklist, fee note) and the educational
/// federal-complaint checklist tab.
/// </summary>
public partial class WorksheetViewModel : ObservableObject
{
    // ---------- Small-claims worksheet ----------

    [ObservableProperty]
    private string _plaintiffName = string.Empty;

    [ObservableProperty]
    private string _plaintiffAddress = string.Empty;

    [ObservableProperty]
    private string _defendantName = string.Empty;

    [ObservableProperty]
    private string _defendantAddress = string.Empty;

    [ObservableProperty]
    private string _amountSought = string.Empty;

    public ObservableCollection<FactRow> FactsRows { get; } = new();

    public string FilingFeeNote =>
        "Filing fee: check your court's website — fees change. ClaimPilot never hardcodes fee amounts.";

    public ObservableCollection<ChecklistItem> PetitionChecklist { get; } = new()
    {
        new ChecklistItem("Caption: the court name and the case title (your name v. the other party's name)"),
        new ChecklistItem("Your full name and mailing address"),
        new ChecklistItem("The defendant's full name and address where they can be served"),
        new ChecklistItem("A short statement of facts in numbered paragraphs: who, what, when, where"),
        new ChecklistItem("The amount you are asking for and how you calculated it"),
        new ChecklistItem("Your signature and the date"),
        new ChecklistItem("Copies for the court and for each defendant"),
        new ChecklistItem("The filing fee (check the court's website for the current amount)"),
        new ChecklistItem("Keep a copy of everything you file, stamped by the clerk if possible")
    };

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private void AddFactRow()
    {
        FactsRows.Add(new FactRow());
    }

    public void RemoveFactRow(FactRow row)
    {
        FactsRows.Remove(row);
    }

    [RelayCommand]
    private async Task ExportWorksheetPdfAsync()
    {
        var file = await FilePickerService.PickSaveFileAsync("LawsuitWorksheet", ".pdf", "PDF file");
        if (file is null)
        {
            StatusMessage = "Export canceled.";
            return;
        }

        try
        {
            var export = new WorksheetExport
            {
                PlaintiffName = PlaintiffName,
                PlaintiffAddress = PlaintiffAddress,
                DefendantName = DefendantName,
                DefendantAddress = DefendantAddress,
                AmountSought = AmountSought,
                Facts = FactsRows.Where(f => !string.IsNullOrWhiteSpace(f.Date) || !string.IsNullOrWhiteSpace(f.Description)).ToList(),
                ChecklistTexts = PetitionChecklist.Select(c => c.Text).ToList(),
                FilingFeeNote = FilingFeeNote
            };
            PdfService.GenerateWorksheetPdf(file.Path, export);
            StatusMessage = $"Worksheet exported to {file.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not export the PDF: " + ex.Message;
        }
    }

    // ---------- Federal complaint — educational checklist ----------

    public string FederalChecklistIntro =>
        "Educational checklist — this is not a filed document and not legal advice. " +
        "A federal complaint generally includes the following parts. Court rules vary; verify everything " +
        "against the local rules of the court where you would file, and consider talking to an attorney.";

    public ObservableCollection<ChecklistItem> FederalChecklist { get; } = new()
    {
        new ChecklistItem("Jurisdiction: why this case belongs in federal court (for example, a federal question under 28 U.S.C. 1331, or diversity of citizenship under 28 U.S.C. 1332)"),
        new ChecklistItem("Parties: the full name and address of each plaintiff and each defendant, and what each defendant did"),
        new ChecklistItem("Statement of facts: what happened, in numbered paragraphs, in chronological order — who, what, when, where"),
        new ChecklistItem("Counts / causes of action: each legal claim stated separately, with the law you believe was violated"),
        new ChecklistItem("Prayer for relief: exactly what you are asking the court to award"),
        new ChecklistItem("Signature block: your signature, printed name, address, phone, and email, with the date (see Fed. R. Civ. P. 11)"),
        new ChecklistItem("IFP application note: if you cannot afford the filing fee, ask the clerk about the in forma pauperis (fee waiver) application — the Pro Se Resources section explains it")
    };

    public string IfpNote =>
        "In forma pauperis ('in the character of a poor person') lets people who cannot afford court fees ask to proceed without prepaying them. " +
        "See the Pro Se Resources section for a plain-English explanation.";

    public string DisclaimerText => LegalText.Disclaimer + " " + LegalText.NeverFilesNote;
}
