using ClaimPilot.Models;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClaimPilot.Views;

/// <summary>Lawsuit Worksheet page: small-claims tab and federal-complaint checklist tab.</summary>
public sealed partial class WorksheetPage : Page
{
    public WorksheetViewModel ViewModel { get; } = new();

    public WorksheetPage()
    {
        InitializeComponent();
    }

    private void RemoveFactRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is FactRow row)
        {
            ViewModel.RemoveFactRow(row);
        }
    }
}
