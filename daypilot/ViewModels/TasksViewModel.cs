using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DayPilot.Models;
using DayPilot.Services;

namespace DayPilot.ViewModels;

/// <summary>
/// Task list: quick add, check off, delete. Backed by TodoStore (todos.json).
/// The voice assistant's todo commands ("add a todo", "mark ... done")
/// operate on the same store, so this page always reflects voice changes.
/// </summary>
public partial class TasksViewModel : ObservableObject
{
    private readonly TodoStore _store = new();

    public ObservableCollection<TodoItem> Tasks { get; } = new();

    [ObservableProperty]
    private string _newTaskTitle = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Tasks.Clear();
        foreach (var todo in (await _store.GetAllAsync())
                     .OrderBy(t => t.IsDone)
                     .ThenByDescending(t => t.CreatedAt))
            Tasks.Add(todo);
    }

    partial void OnNewTaskTitleChanged(string value)
    {
        AddTaskCommand.NotifyCanExecuteChanged();
    }

    private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskTitle);

    [RelayCommand(CanExecute = nameof(CanAddTask))]
    private async Task AddTaskAsync()
    {
        var title = NewTaskTitle.Trim();
        var item = new TodoItem { Title = title };
        await _store.AddAsync(item);
        Tasks.Insert(0, item);
        NewTaskTitle = string.Empty;
        NotificationService.Instance.ShowToast("Task added", title);
    }

    [RelayCommand]
    private async Task ToggleDoneAsync(TodoItem? item)
    {
        if (item == null)
            return;
        item.CompletedAt = item.IsDone ? DateTime.Now : null;
        await _store.UpdateAsync(item);
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TodoItem? item)
    {
        if (item == null)
            return;
        await _store.DeleteAsync(item.Id);
        Tasks.Remove(item);
    }
}
