using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DayPilot.Models;
using DayPilot.Services;

namespace DayPilot.ViewModels;

/// <summary>
/// Master-detail notes editor with on-device AI actions (summarize, rewrite,
/// extract action items). AI commands stay disabled until the model is ready.
/// </summary>
public partial class NotesViewModel : ObservableObject
{
    private readonly NoteStore _store = new();
    private readonly AiService _ai = AiService.Instance;

    public AiService Ai => _ai;

    public ObservableCollection<Note> Notes { get; } = new();

    [ObservableProperty]
    private Note? _selectedNote;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private string _tags = string.Empty;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private bool _aiBusy;

    public event EventHandler? FocusTitleRequested;

    public NotesViewModel()
    {
        _ai.PropertyChanged += OnAiPropertyChanged;
    }

    private void OnAiPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AiService.IsAvailable))
            RefreshCommandStates();
    }

    partial void OnSelectedNoteChanged(Note? value)
    {
        if (value == null)
        {
            Title = string.Empty;
            Body = string.Empty;
            Tags = string.Empty;
        }
        else
        {
            Title = value.Title;
            Body = value.Body;
            Tags = value.Tags;
        }
        ResultText = string.Empty;
        RefreshCommandStates();
    }

    partial void OnTitleChanged(string value) => RefreshCommandStates();

    partial void OnBodyChanged(string value) => RefreshCommandStates();

    partial void OnAiBusyChanged(bool value) => RefreshCommandStates();

    private void RefreshCommandStates()
    {
        SaveNoteCommand.NotifyCanExecuteChanged();
        DeleteNoteCommand.NotifyCanExecuteChanged();
        SummarizeCommand.NotifyCanExecuteChanged();
        RewriteCommand.NotifyCanExecuteChanged();
        ActionItemsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Notes.Clear();
        foreach (var note in await _store.GetAllAsync())
            Notes.Add(note);
    }

    [RelayCommand]
    private void NewNote()
    {
        SelectedNote = null;
        ResultText = string.Empty;
        FocusTitleRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanSaveNote() => !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Body);

    [RelayCommand(CanExecute = nameof(CanSaveNote))]
    private async Task SaveNoteAsync()
    {
        string title = string.IsNullOrWhiteSpace(Title) ? "Untitled" : Title.Trim();
        var selected = SelectedNote;
        if (selected == null)
        {
            var note = new Note { Title = title, Body = Body, Tags = Tags };
            await _store.AddAsync(note);
            Notes.Insert(0, note);
            SelectedNote = note;
        }
        else
        {
            selected.Title = title;
            selected.Body = Body;
            selected.Tags = Tags;
            selected.UpdatedAt = DateTime.Now;
            await _store.UpdateAsync(selected);
            Notes.Remove(selected);
            Notes.Insert(0, selected);
        }
        RefreshCommandStates();
    }

    private bool CanDeleteNote() => SelectedNote != null;

    [RelayCommand(CanExecute = nameof(CanDeleteNote))]
    private async Task DeleteNoteAsync()
    {
        if (SelectedNote == null)
            return;
        await _store.DeleteAsync(SelectedNote.Id);
        Notes.Remove(SelectedNote);
        SelectedNote = null;
    }

    private bool CanRunAi() => Ai.IsAvailable && !AiBusy && !string.IsNullOrWhiteSpace(Body);

    [RelayCommand(CanExecute = nameof(CanRunAi))]
    private async Task SummarizeAsync()
    {
        await RunAiAsync(() => _ai.SummarizeAsync(Body), result => ResultText = result);
    }

    [RelayCommand(CanExecute = nameof(CanRunAi))]
    private async Task RewriteAsync()
    {
        await RunAiAsync(() => _ai.RewriteAsync(Body), result =>
        {
            Body = result;
            ResultText = "Note rewritten with on-device AI.";
        });
    }

    [RelayCommand(CanExecute = nameof(CanRunAi))]
    private async Task ActionItemsAsync()
    {
        await RunAiAsync(() => _ai.ExtractActionItemsAsync(Body), result => ResultText = result);
    }

    private async Task RunAiAsync(Func<Task<string>> action, Action<string> apply)
    {
        if (AiBusy)
            return;
        AiBusy = true;
        try
        {
            apply(await action());
        }
        finally
        {
            AiBusy = false;
        }
    }
}
