using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using ClaimPilot.ViewModels;

namespace ClaimPilot.Views;

/// <summary>
/// Reusable pen/stylus/touch ink surface: named pages, ISF persistence,
/// undo/redo, and confirm dialogs for Clear and Delete.
/// </summary>
public sealed partial class InkNotesControl : UserControl
{
    public InkNotesViewModel ViewModel { get; } = new();

    // Undo/redo stacks for user-drawn strokes (most recent on top).
    private Stack<InkStroke> _undoStack = new();
    private Stack<InkStroke> _redoStack = new();

    // True while we change the stroke container programmatically so the
    // StrokesCollected/StrokesErased handlers don't record our own edits.
    private bool _suppressStrokeEvents;

    public InkNotesControl()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;

        var presenter = InkCanvasControl.InkPresenter;
        presenter.InputDeviceTypes =
            CoreInputDeviceTypes.Pen |
            CoreInputDeviceTypes.Touch |
            CoreInputDeviceTypes.Mouse;
        presenter.StrokesCollected += OnStrokesCollected;
        presenter.StrokesErased += OnStrokesErased;

        ViewModel.SelectedPageChanged += OnSelectedPageChanged;
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadPagesCommand.ExecuteAsync(null);
    }

    // ----- page switching -------------------------------------------------

    private async void OnSelectedPageChanged(object? sender, string? pageName)
    {
        var container = InkCanvasControl.InkPresenter.StrokeContainer;
        _suppressStrokeEvents = true;
        try
        {
            container.Clear();
            ClearUndoRedo();

            var isf = await ViewModel.LoadStrokesAsync();
            if (isf != null && isf.Length > 0)
            {
                using var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(isf);
                stream.Seek(0);
                await container.LoadAsync(stream);
            }
        }
        catch
        {
            // Corrupt ISF: leave a blank canvas rather than crash.
            container.Clear();
            ClearUndoRedo();
        }
        finally
        {
            _suppressStrokeEvents = false;
        }
    }

    // ----- save / clear / delete -------------------------------------------

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var container = InkCanvasControl.InkPresenter.StrokeContainer;
        try
        {
            using var stream = new InMemoryRandomAccessStream();
            await container.SaveAsync(stream);
            stream.Seek(0);
            var buffer = new Buffer((uint)stream.Size);
            await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
            await ViewModel.SaveStrokesAsync(buffer, container.GetStrokes().Count);
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    private async void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Clear ink canvas?",
            Content = "This erases every stroke on the current page. " +
                      "The saved copy is untouched until you press Save.",
            PrimaryButtonText = "Clear",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot,
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            InkCanvasControl.InkPresenter.StrokeContainer.Clear();
            ClearUndoRedo();
        }
    }

    private async void DeletePageButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ViewModel.SelectedPageName;
        if (name == null)
        {
            ViewModel.StatusMessage = "Select an ink page to delete.";
            return;
        }
        var dialog = new ContentDialog
        {
            Title = $"Delete \"{name}\"?",
            Content = "The saved ink page will be permanently deleted.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot,
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await ViewModel.DeleteSelectedPageAsync();
    }

    // ----- undo / redo ------------------------------------------------------

    private void OnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
    {
        if (_suppressStrokeEvents)
            return;
        foreach (var stroke in args.Strokes)
            _undoStack.Push(stroke);
        _redoStack.Clear();
    }

    private void OnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
    {
        if (_suppressStrokeEvents)
            return;
        // Drop erased strokes from the undo history so Undo never targets
        // a stroke that is already gone.
        var erased = args.Strokes.ToHashSet();
        _undoStack = new Stack<InkStroke>(_undoStack.Where(s => !erased.Contains(s)).Reverse());
        _redoStack.Clear();
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        if (_undoStack.Count == 0)
            return;
        var stroke = _undoStack.Pop();
        RemoveStroke(stroke);
        _redoStack.Push(stroke);
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        if (_redoStack.Count == 0)
            return;
        var stroke = _redoStack.Pop();
        _suppressStrokeEvents = true;
        try
        {
            InkCanvasControl.InkPresenter.StrokeContainer.AddStroke(stroke);
        }
        finally
        {
            _suppressStrokeEvents = false;
        }
        _undoStack.Push(stroke);
    }

    private void RemoveStroke(InkStroke stroke)
    {
        var container = InkCanvasControl.InkPresenter.StrokeContainer;
        var remaining = container.GetStrokes().Where(s => !ReferenceEquals(s, stroke)).ToList();
        _suppressStrokeEvents = true;
        try
        {
            container.Clear();
            foreach (var s in remaining)
                container.AddStroke(s);
        }
        finally
        {
            _suppressStrokeEvents = false;
        }
    }

    private void ClearUndoRedo()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
