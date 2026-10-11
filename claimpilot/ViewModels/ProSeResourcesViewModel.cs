using System.Collections.ObjectModel;
using ClaimPilot.Data;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.ViewModels;

/// <summary>
/// Pro Se Resources: plain-English IFP explanation, per-district court links,
/// and the standing rule that filing fees are never hardcoded.
/// </summary>
public partial class ProSeResourcesViewModel : ObservableObject
{
    public string IfpTitle => "In forma pauperis (IFP) — fee waivers, in plain English";

    public string IfpBody =>
        "“In forma pauperis” is Latin for “in the character of a poor person.” In federal court, it is the process " +
        "that lets someone who cannot afford the filing fee ask the court to let them proceed without prepaying it.\n\n" +
        "How it generally works: you fill out a financial affidavit — often on AO Form 239, the court's Application to Proceed " +
        "in District Court Without Prepaying Fees or Costs — listing your income, expenses, assets, and debts, and you file it " +
        "with your complaint. A judge reviews it and may grant it, deny it, or grant it in part.\n\n" +
        "What it does NOT do: being granted IFP does not mean your case has merit, does not appoint you a lawyer in a civil case, " +
        "and does not excuse you from following every court rule and deadline. Some courts still require partial payments over time.\n\n" +
        "Every court has its own local rules and many publish a pro se handbook or guide for people representing themselves. " +
        "Use the district picker below to open a court's official website and look for its pro se resources.\n\n" +
        "Filing fees change — always check the court's website for the current fee schedule. ClaimPilot never hardcodes fee amounts.";

    public ObservableCollection<DistrictInfo> Districts { get; } = new(Data.Districts.All);

    [ObservableProperty]
    private DistrictInfo? _selectedDistrict;

    [ObservableProperty]
    private Uri? _selectedCourtUrl;

    partial void OnSelectedDistrictChanged(DistrictInfo? value)
    {
        SelectedCourtUrl = value?.WebsiteUrl;
    }

    public Uri CourtLocatorUrl { get; } = new("https://www.uscourts.gov/court-locator");

    public string FeeNote =>
        "Filing fees change. Check the court's website for the current fee schedule — this app never lists fee amounts.";

    public string LocalRulesNote =>
        "Every federal district court has its own local rules, and many publish a pro se handbook. " +
        "Before drafting or filing anything, read the local rules of the court where you would file. " +
        "When in doubt, call the clerk's office — clerks can explain procedures (not legal advice).";

    public string DisclaimerText => LegalText.Disclaimer + " " + LegalText.NeverFilesNote;
}
