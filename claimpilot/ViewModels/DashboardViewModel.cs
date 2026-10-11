using System.Collections.ObjectModel;
using ClaimPilot.Models;
using ClaimPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.ViewModels;

/// <summary>Dashboard: module cards plus the disclaimer footer.</summary>
public partial class DashboardViewModel : ObservableObject
{
    public ObservableCollection<NavCard> Cards { get; } = new()
    {
        new NavCard("Violation Spotter", "Answer plain-English questions about collector conduct and see which federal laws MAY apply.", Symbol.Flag, "violations"),
        new NavCard("Letter Builder", "Build debt-validation, cease-communication, and credit-bureau dispute letters. Export TXT or PDF.", Symbol.Mail, "letters"),
        new NavCard("Lawsuit Worksheet", "Small-claims worksheet with a facts timeline, plus an educational federal-complaint checklist.", Symbol.Document, "worksheet"),
        new NavCard("Civil Rights", "Plain-English guides to six Section 1983 claim types, with honest notes on defenses and deadlines.", Symbol.People, "civilrights"),
        new NavCard("Court Finder", "All 94 U.S. federal district courts, searchable by state or territory, with official websites.", Symbol.Find, "courtfinder"),
        new NavCard("Bankruptcy", "Chapter 7 & 13 guides, document checklists, deadline presets, and the required-courses tracker. Bankruptcy Plus unlock.", Symbol.Bank, "bankruptcy"),
        new NavCard("Deadlines", "Track your deadlines on this PC with local toast reminders. Never miss a date.", Symbol.Calendar, "deadlines"),
        new NavCard("Evidence Vault", "Store evidence, build a date-sorted timeline, and export an organized evidence packet PDF.", Symbol.Folder, "vault"),
        new NavCard("About", "The full disclaimer, version info, and how your data is handled.", Symbol.Help, "about"),
    };

    public string DisclaimerText => LegalText.Disclaimer;

    public string Tagline => "On-device self-help legal toolkit. Your data never leaves this PC.";

    public void Navigate(string tag)
    {
        if (App.MainWindow is MainWindow window)
        {
            window.NavigateToTag(tag);
        }
    }
}
