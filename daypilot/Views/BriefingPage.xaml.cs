using DayPilot.Services;
using DayPilot.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DayPilot.Views;

public sealed partial class BriefingPage : Page
{
    public BriefingViewModel ViewModel { get; } = new();

    private readonly SpeechService _speech = SpeechService.Instance;
    private readonly VoiceCommandRouter _router = new();
    private CancellationTokenSource? _listeningCts;
    private bool _busy;

    public BriefingPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        // Never leave the mic hot or speech playing when leaving the page.
        _listeningCts?.Cancel();
        _speech.StopSpeaking();
    }

    private async void TalkButton_Click(object sender, RoutedEventArgs e)
    {
        if (_busy)
        {
            // Tap again while listening: stop the current turn.
            _listeningCts?.Cancel();
            _speech.StopSpeaking();
            return;
        }

        _busy = true;
        TalkPanel.Visibility = Visibility.Collapsed;
        try
        {
            TalkStatusText.Text = "Listening… speak now (tap again to stop).";
            _listeningCts = new CancellationTokenSource();

            var outcome = await _speech.ListenOnceAsync(_listeningCts.Token);

            if (outcome.WasCancelled)
            {
                TalkStatusText.Text = string.Empty;
                return;
            }

            if (!outcome.HeardSomething)
            {
                TalkStatusText.Text = string.Empty;
                ShowTalk("(nothing heard)", outcome.Message);
                await _speech.SpeakAsync(outcome.Message);
                return;
            }

            TalkStatusText.Text = "Thinking…";
            var reply = await _router.HandleAsync(outcome.Text);
            TalkStatusText.Text = string.Empty;
            ShowTalk(outcome.Text, reply.Display);
            await _speech.SpeakAsync(reply.Spoken);

            // Refresh the briefing numbers in case the turn changed data.
            await ViewModel.RefreshCommand.ExecuteAsync(null);
        }
        catch (Exception)
        {
            TalkStatusText.Text = string.Empty;
            ShowTalk(string.Empty, "Hmm, something hiccupped — mind trying again?");
        }
        finally
        {
            _listeningCts?.Dispose();
            _listeningCts = null;
            _busy = false;
        }
    }

    private void ShowTalk(string heard, string reply)
    {
        TalkHeardText.Text = heard;
        TalkReplyText.Text = reply;
        TalkPanel.Visibility = Visibility.Visible;
    }
}
