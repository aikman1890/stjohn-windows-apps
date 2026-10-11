using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClaimPilot.ViewModels;

/// <summary>About page: full disclaimer, version, and data-handling statement.</summary>
public partial class AboutViewModel : ObservableObject
{
    public string AppName => "ClaimPilot";

    public string VersionText => "Version 1.0.0";

    public string PublisherText => "Publisher: St. John Pictures";

    public string DisclaimerText => LegalText.Disclaimer;

    public string DataStaysText =>
        "All data stays on this PC. ClaimPilot stores your deadlines, evidence records, and timeline in a local " +
        "SQLite database under this app's local folder, and copies attached files into a local Vault folder. " +
        "ClaimPilot makes no cloud calls and has no account system. The only network activity is opening a court's " +
        "official website in your browser when you click a link.";

    public string WhatItDoesText =>
        "ClaimPilot provides general legal information and document tools: a debt-collector violation spotter, " +
        "letter templates, a lawsuit worksheet, plain-English civil-rights claim guides, a federal court finder, " +
        "deadline tracking with local reminders, and an evidence vault.";

    public string WhatItDoesNotDoText =>
        "ClaimPilot is not a lawyer and does not give legal advice. It never concludes that anyone “has a winning case.” " +
        "It never files anything with any court. Verify everything with your court's rules or an attorney.";
}
