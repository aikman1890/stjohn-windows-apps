using DayPilot.Models;
using DayPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DayPilot.Views;

public sealed partial class RemindersPage : Page
{
    public RemindersViewModel ViewModel { get; } = new();

    public RemindersPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void ReminderCheckBox_Click(object sender, RoutedEventArgs e)
    {
        // The TwoWay binding already wrote the new value into the model;
        // the command persists it to disk.
        if (sender is CheckBox box && box.DataContext is ReminderItem item)
            ViewModel.ToggleDoneCommand.Execute(item);
    }

    private void DeleteReminder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ReminderItem item)
            ViewModel.DeleteReminderCommand.Execute(item);
    }
}
