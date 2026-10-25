using DenialPilot.Models;
using DenialPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace DenialPilot.Views;

public sealed partial class TemplatesPage : Page
{
    private static readonly string[] Methods =
    {
        "Fax",
        "Certified mail",
        "Portal upload",
        "Email",
        "Regular mail",
        "In person",
    };

    private static readonly (string Name, string Body)[] Templates =
    {
        ("Level 1 internal appeal",
         "Dear {Insurer} Appeals Department,\n\nI am writing to appeal the denial of claim {ClaimNumber} for {PatientName}, for services provided by {Provider} on {ServiceDate}. The amount denied is {AmountDenied}. The stated denial category was: {ReasonCategory}.\n\nI am requesting a full and fair review of this claim, including all medical records and documentation submitted with this appeal. Please identify the specific plan provision relied upon for the denial and the qualifications of the reviewer.\n\nPlease confirm receipt of this appeal in writing, and provide your decision within the time required by the plan and applicable law.\n\nSincerely,\n{PatientName}\n\n(General information template - not legal advice.)"),
        ("Level 2 appeal",
         "Dear {Insurer} Appeals Department,\n\nI am writing to file a second-level appeal of the denial of claim {ClaimNumber} for {PatientName} (services by {Provider} on {ServiceDate}; amount denied {AmountDenied}). The first-level denial was upheld, and I am exercising my right to the next level of internal review under the plan.\n\nPlease review the complete claim file, including all records submitted at every level, and provide the specific clinical or plan basis for any continued denial, together with copies of any guidelines or criteria relied upon.\n\nSincerely,\n{PatientName}\n\n(General information template - not legal advice.)"),
        ("External review request",
         "Dear External Review Organization / State Review Program,\n\nI am requesting an independent external review of a final adverse determination by {Insurer} ({Plan}) for claim {ClaimNumber}, patient {PatientName}, provider {Provider}, date of service {ServiceDate}, amount denied {AmountDenied}.\n\nThe internal appeal process has been completed (or the plan has failed to respond within the required time). Please find the denial letters, the Explanation of Benefits, and supporting medical records attached.\n\nSincerely,\n{PatientName}\n\n(General information template - not legal advice.)"),
        ("State DOI complaint cover letter",
         "Dear State Department of Insurance, Consumer Services,\n\nI am filing a complaint against {Insurer} regarding claim {ClaimNumber} for {PatientName} (plan: {Plan}; provider: {Provider}; date of service: {ServiceDate}; amount denied: {AmountDenied}).\n\nThe claim was denied on {DenialDate} under the category: {ReasonCategory}. I appealed through the plan's internal process; the denial was not resolved. A chronological proof ledger of every letter, call, and deadline for this case is attached.\n\nI am asking the Department to review the handling of this claim and the plan's compliance with applicable requirements.\n\nSincerely,\n{PatientName}\n\n(General information template - not legal advice.)"),
        ("Physician letter-of-medical-necessity request",
         "Dear {Provider},\n\nI am writing to ask for your help with an insurance denial. {Insurer} denied claim {ClaimNumber} for my care on {ServiceDate} (amount denied {AmountDenied}), under the category: {ReasonCategory}.\n\nWould you be willing to write a letter of medical necessity explaining why the treatment was medically necessary for me, referencing my diagnosis, prior treatments, and the clinical basis for your plan of care? Insurer appeals are often decided on this documentation.\n\nThank you for your care and your help.\n\nSincerely,\n{PatientName}\n\n(General information template - not legal advice.)"),
        ("Medical-bill dispute letter",
         "Dear {Provider} Billing Department,\n\nI am writing to dispute the bill connected to claim {ClaimNumber} for {PatientName}, date of service {ServiceDate}. The insurer, {Insurer}, denied {AmountDenied} of this claim, and I am actively appealing that denial.\n\nPlease place this account on hold and mark it as disputed pending the outcome of the insurance appeal, and please do not send this balance to collections while the appeal is pending. Please confirm the hold in writing.\n\nSincerely,\n{PatientName}\n\n(General information template - not legal advice.)"),
    };

    private readonly JsonListStore<DenialCase> _caseStore = new("cases.json");
    private readonly JsonListStore<LedgerEntry> _ledgerStore = new("ledger.json");
    private List<DenialCase> _cases = new();

    public TemplatesPage()
    {
        this.InitializeComponent();
        TemplatePicker.ItemsSource = Templates.Select(t => t.Name).ToList();
        TemplatePicker.SelectedIndex = 0;
        MethodCombo.ItemsSource = Methods;
        MethodCombo.SelectedIndex = 0;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cases = await _caseStore.LoadAsync();
        CasePicker.ItemsSource = _cases.Select(c => c.DisplayLine).ToList();
        if (_cases.Count > 0) CasePicker.SelectedIndex = 0;
        Merge();
    }

    private DenialCase? PickedCase =>
        CasePicker.SelectedIndex >= 0 && CasePicker.SelectedIndex < _cases.Count
            ? _cases[CasePicker.SelectedIndex]
            : null;

    private void TemplatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => Merge();

    private void CasePicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => Merge();

    private void MergeButton_Click(object sender, RoutedEventArgs e) => Merge();

    private void Merge()
    {
        if (TemplatePicker.SelectedIndex < 0) return;
        var body = Templates[TemplatePicker.SelectedIndex].Body;
        var c = PickedCase;
        if (c is not null)
        {
            body = body
                .Replace("{Insurer}", c.Insurer)
                .Replace("{Plan}", c.Plan)
                .Replace("{ClaimNumber}", c.ClaimNumber)
                .Replace("{PatientName}", c.PatientName)
                .Replace("{Provider}", c.Provider)
                .Replace("{ServiceDate}", $"{c.ServiceDate:MMMM d, yyyy}")
                .Replace("{DenialDate}", $"{c.DenialDate:MMMM d, yyyy}")
                .Replace("{AmountDenied}", c.AmountDenied)
                .Replace("{ReasonCategory}", c.ReasonCategory);
        }
        LetterBox.Text = body;
    }

    private async void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        var c = PickedCase;
        if (c is null)
        {
            StatusText.Text = "Pick a case first.";
            return;
        }
        var templateName = Templates[TemplatePicker.SelectedIndex].Name;
        var method = MethodCombo.SelectedItem as string ?? Methods[0];

        var ledger = await _ledgerStore.LoadAsync();
        ledger.Add(new LedgerEntry
        {
            CaseId = c.Id,
            CaseLabel = c.DisplayLine,
            Date = DateTimeOffset.Now,
            Kind = "Letter sent",
            Detail = templateName,
            Method = method,
        });
        await _ledgerStore.SaveAsync(ledger);

        if (templateName.Contains("appeal") || templateName.Contains("External review"))
        {
            if (c.AppealSentDate is null)
            {
                c.AppealSentDate = DateTimeOffset.Now;
                var cases = await _caseStore.LoadAsync();
                var stored = cases.FirstOrDefault(x => x.Id == c.Id);
                if (stored is not null) stored.AppealSentDate = c.AppealSentDate;
                await _caseStore.SaveAsync(cases);
            }
            StatusText.Text = $"Recorded: {templateName} sent via {method}. The response clock is running on the case.";
        }
        else
        {
            StatusText.Text = $"Recorded: {templateName} sent via {method}.";
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.WindowInstance is null) return;
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "appeal-letter",
        };
        picker.FileTypeChoices.Add("Text file", new List<string> { ".txt" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, LetterBox.Text);
        StatusText.Text = $"Letter exported to {file.Path}";
    }
}
