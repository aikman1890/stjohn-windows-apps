using ClaimPilot.Models;
using ClaimPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ClaimPilot.Views;

/// <summary>Bankruptcy page: guides, checklists, course tracker, deadline presets, unlock gate.</summary>
public sealed partial class BankruptcyPage : Page
{
    public BankruptcyViewModel ViewModel { get; } = new();

    public BankruptcyPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = ViewModel.LoadAsync();
    }

    private static CourseRecord? RecordFrom(object sender) =>
        (sender as FrameworkElement)?.DataContext as CourseRecord;

    private async void CourseStatus_Changed(object sender, SelectionChangedEventArgs e)
    {
        var record = RecordFrom(sender);
        if (record is not null)
        {
            await ViewModel.SaveCourseAsync(record);
        }
    }

    private async void CourseDate_Changed(object sender, DatePickerValueChangedEventArgs e)
    {
        var record = RecordFrom(sender);
        if (record is not null)
        {
            await ViewModel.SaveCourseAsync(record);
        }
    }

    private async void CourseText_LostFocus(object sender, RoutedEventArgs e)
    {
        var record = RecordFrom(sender);
        if (record is not null)
        {
            await ViewModel.SaveCourseAsync(record);
        }
    }

    private async void AttachCertificate_Click(object sender, RoutedEventArgs e)
    {
        var record = RecordFrom(sender);
        if (record is not null)
        {
            await ViewModel.AttachCertificateAsync(record);
        }
    }

    private async void OpenCertificate_Click(object sender, RoutedEventArgs e)
    {
        var record = RecordFrom(sender);
        if (record is not null)
        {
            await ViewModel.OpenCertificateAsync(record);
        }
    }

    private async void ResetCourse_Click(object sender, RoutedEventArgs e)
    {
        var record = RecordFrom(sender);
        if (record is not null)
        {
            await ViewModel.ResetCourseAsync(record);
        }
    }

    private async void CreditCounselingLink_Click(object sender, RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(ViewModel.CreditCounselingProvidersUrl));
    }

    private async void DebtorEducationLink_Click(object sender, RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(ViewModel.DebtorEducationProvidersUrl));
    }
}
