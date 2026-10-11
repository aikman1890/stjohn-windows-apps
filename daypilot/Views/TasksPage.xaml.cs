using DayPilot.Models;
using DayPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace DayPilot.Views;

public sealed partial class TasksPage : Page
{
    public TasksViewModel ViewModel { get; } = new();

    public TasksPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void NewTaskBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Enter in the add box submits, same as clicking Add task.
        if (e.Key == Windows.System.VirtualKey.Enter
            && ViewModel.AddTaskCommand.CanExecute(null))
        {
            ViewModel.AddTaskCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void TaskCheckBox_Click(object sender, RoutedEventArgs e)
    {
        // The TwoWay binding already wrote the new value into the model;
        // the command stamps CompletedAt and persists it to disk.
        if (sender is CheckBox box && box.DataContext is TodoItem item)
            ViewModel.ToggleDoneCommand.Execute(item);
    }

    private void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TodoItem item)
            ViewModel.DeleteTaskCommand.Execute(item);
    }
}
