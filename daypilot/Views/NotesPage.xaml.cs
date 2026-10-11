using DayPilot.Services;
using DayPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DayPilot.Views;

public sealed partial class NotesPage : Page
{
    public NotesViewModel ViewModel { get; } = new();

    private readonly SpeechService _speech = SpeechService.Instance;
    private CancellationTokenSource? _dictationCts;
    private bool _dictating;

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

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _dictationCts?.Cancel();
    }

    /// <summary>
    /// QuickCapture: puts the cursor in the title box for a new note.
    /// </summary>
    public void FocusTitleBox()
    {
        NotesPivot.SelectedIndex = 0; // QuickCapture always targets the text tab.
        TitleBox.Focus(FocusState.Programmatic);
    }

    private async void DictateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_dictating)
        {
            // Tap again: stop listening.
            _dictationCts?.Cancel();
            return;
        }

        _dictating = true;
        try
        {
            SetDictationStatus("Listening… speak now (tap Dictate again to stop).");
            _dictationCts = new CancellationTokenSource();

            var outcome = await _speech.ListenOnceAsync(_dictationCts.Token);

            if (outcome.WasCancelled)
            {
                SetDictationStatus(null);
                return;
            }

            if (!outcome.HeardSomething)
            {
                SetDictationStatus(outcome.Message);
                return;
            }

            InsertAtCursor(outcome.Text);
            SetDictationStatus(null);
        }
        catch (Exception)
        {
            SetDictationStatus("The microphone isn't cooperating right now. Please try again in a moment.");
        }
        finally
        {
            _dictationCts?.Dispose();
            _dictationCts = null;
            _dictating = false;
        }
    }

    private void InsertAtCursor(string text)
    {
        string body = ViewModel.Body ?? string.Empty;
        int pos = Math.Clamp(BodyBox.SelectionStart, 0, body.Length);

        string insert = text;
        if (pos > 0 && !char.IsWhiteSpace(body[pos - 1]) && !char.IsWhiteSpace(insert[0]))
            insert = " " + insert;

        ViewModel.Body = body.Insert(pos, insert);

        // Put the caret right after the inserted text.
        BodyBox.Focus(FocusState.Programmatic);
        BodyBox.SelectionStart = pos + insert.Length;
        BodyBox.SelectionLength = 0;
    }

    private void SetDictationStatus(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            DictationStatusText.Visibility = Visibility.Collapsed;
            DictationStatusText.Text = string.Empty;
        }
        else
        {
            DictationStatusText.Text = message;
            DictationStatusText.Visibility = Visibility.Visible;
        }
    }
}
