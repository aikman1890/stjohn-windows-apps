using System.Collections.ObjectModel;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Letter Builder: fills three complete letter templates from user fields,
/// previews the result, and saves TXT / exports PDF / copies to clipboard.
/// </summary>
public partial class LetterBuilderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _userAddress = string.Empty;

    [ObservableProperty]
    private string _recipientName = string.Empty;

    [ObservableProperty]
    private string _recipientAddress = string.Empty;

    [ObservableProperty]
    private string _accountNumber = string.Empty;

    public ObservableCollection<string> TemplateNames { get; } = new()
    {
        "Debt Validation Request (15 U.S.C. 1692g)",
        "Cease Communication (15 U.S.C. 1692c(c))",
        "Credit Bureau Dispute (15 U.S.C. 1681i)"
    };

    [ObservableProperty]
    private int _selectedTemplateIndex;

    [ObservableProperty]
    private string _generatedLetter = string.Empty;

    [ObservableProperty]
    private bool _hasLetter;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string DisclaimerText => LegalText.Disclaimer;

    public string IntroText =>
        "Fill in the fields, pick a template, and generate a complete letter. " +
        "For the credit bureau dispute, enter the bureau's name and address in the recipient fields. " +
        "This is general information, not legal advice.";

    [RelayCommand]
    private void GenerateLetter()
    {
        var name = string.IsNullOrWhiteSpace(UserName) ? "[Your Name]" : UserName.Trim();
        var address = string.IsNullOrWhiteSpace(UserAddress) ? "[Your Address]" : UserAddress.Trim();
        var recipient = string.IsNullOrWhiteSpace(RecipientName) ? "[Recipient Name]" : RecipientName.Trim();
        var recipientAddr = string.IsNullOrWhiteSpace(RecipientAddress) ? "[Recipient Address]" : RecipientAddress.Trim();
        var account = string.IsNullOrWhiteSpace(AccountNumber) ? "[Account Number]" : AccountNumber.Trim();

        GeneratedLetter = SelectedTemplateIndex switch
        {
            1 => LetterTemplates.BuildCeaseCommunicationLetter(name, address, recipient, recipientAddr, account),
            2 => LetterTemplates.BuildBureauDisputeLetter(name, address, recipient, recipientAddr, account),
            _ => LetterTemplates.BuildDebtValidationLetter(name, address, recipient, recipientAddr, account)
        };

        HasLetter = true;
        StatusMessage = "Letter generated. Review it carefully before sending — consider talking to an attorney.";
    }

    [RelayCommand]
    private async Task SaveTxtAsync()
    {
        if (!HasLetter || string.IsNullOrWhiteSpace(GeneratedLetter))
        {
            StatusMessage = "Generate a letter first.";
            return;
        }

        var file = await FilePickerService.PickSaveFileAsync("ClaimPilotLetter", ".txt", "Text file");
        if (file is null)
        {
            StatusMessage = "Save canceled.";
            return;
        }

        try
        {
            await FileIO.WriteTextAsync(file, GeneratedLetter);
            StatusMessage = $"Saved to {file.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not save the file: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (!HasLetter || string.IsNullOrWhiteSpace(GeneratedLetter))
        {
            StatusMessage = "Generate a letter first.";
            return;
        }

        var file = await FilePickerService.PickSaveFileAsync("ClaimPilotLetter", ".pdf", "PDF file");
        if (file is null)
        {
            StatusMessage = "Export canceled.";
            return;
        }

        try
        {
            var title = TemplateNames[SelectedTemplateIndex];
            PdfService.GenerateLetterPdf(file.Path, title, GeneratedLetter);
            StatusMessage = $"Exported PDF to {file.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not export the PDF: " + ex.Message;
        }
    }

    [RelayCommand]
    private void CopyLetter()
    {
        if (!HasLetter || string.IsNullOrWhiteSpace(GeneratedLetter))
        {
            StatusMessage = "Generate a letter first.";
            return;
        }

        try
        {
            var package = new DataPackage();
            package.SetText(GeneratedLetter);
            Clipboard.SetContent(package);
            StatusMessage = "Letter copied to the clipboard.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Could not copy: " + ex.Message;
        }
    }
}
