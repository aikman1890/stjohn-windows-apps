using DayPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DayPilot.Views;

public sealed partial class NotesPage : Page
{
    public NotesViewModel ViewModel { get; } = new();

    public NotesPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
        ViewModel.FocusTitleRequested += (_, _) => FocusTitleBox();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// QuickCapture: puts the cursor in the title box for a new note.
    /// </summary>
    public void FocusTitleBox()
    {
        TitleBox.Focus(FocusState.Programmatic);
    }
}
