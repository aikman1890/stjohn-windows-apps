using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Court Finder page: searchable federal district list.</summary>
public sealed partial class CourtFinderPage : Page
{
    public CourtFinderViewModel ViewModel { get; } = new();

    public CourtFinderPage()
    {
        InitializeComponent();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.SetSearchText(SearchBox.Text);
    }
}
