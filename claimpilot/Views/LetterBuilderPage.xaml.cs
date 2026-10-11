using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Letter Builder page.</summary>
public sealed partial class LetterBuilderPage : Page
{
    public LetterBuilderViewModel ViewModel { get; } = new();

    public LetterBuilderPage()
    {
        InitializeComponent();
    }
}
