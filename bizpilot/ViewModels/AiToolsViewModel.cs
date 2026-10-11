using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BizPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace BizPilot.ViewModels;

public partial class AiToolsViewModel : ObservableObject
{
    private readonly AiService _ai;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateEmailCommand))]
    private string emailBullets = string.Empty;

    [ObservableProperty]
    private string emailTone = "Professional";

    [ObservableProperty]
    private ObservableCollection<string> tones = new() { "Professional", "Friendly", "Formal", "Concise" };

    [ObservableProperty]
    private string generatedEmail = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SummarizeCommand))]
    private string meetingNotes = string.Empty;

    [ObservableProperty]
    private string meetingSummary = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateEmailCommand))]
    [NotifyCanExecuteChangedFor(nameof(SummarizeCommand))]
    private bool isBusy;

    [ObservableProperty]
    private bool isAiAvailable;

    [ObservableProperty]
    private string aiStatusMessage = string.Empty;

    public AiToolsViewModel()
    {
        _ai = ((App)Application.Current).Ai;
        RefreshAiStatus();
    }

    public void RefreshAiStatus()
    {
        IsAiAvailable = _ai.IsAvailable;
        AiStatusMessage = _ai.StatusMessage;
        GenerateEmailCommand.NotifyCanExecuteChanged();
        SummarizeCommand.NotifyCanExecuteChanged();
    }

    private bool CanGenerateEmail() =>
        IsAiAvailable && !IsBusy && !string.IsNullOrWhiteSpace(EmailBullets);

    private bool CanSummarize() =>
        IsAiAvailable && !IsBusy && !string.IsNullOrWhiteSpace(MeetingNotes);

    [RelayCommand(CanExecute = nameof(CanGenerateEmail))]
    private async Task GenerateEmailAsync()
    {
        IsBusy = true;
        try
        {
            GeneratedEmail = await _ai.DraftClientEmailAsync(EmailBullets, EmailTone);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSummarize))]
    private async Task SummarizeAsync()
    {
        IsBusy = true;
        try
        {
            MeetingSummary = await _ai.SummarizeMeetingNotesAsync(MeetingNotes);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
