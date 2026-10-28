using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Media.SpeechSynthesis;
using Wuse.Models;
using Wuse.Services;

namespace Wuse.ViewModels;

public partial class PreferencesViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    [ObservableProperty]
    private string assistantName = "wuse";

    [ObservableProperty]
    private string themeName = "System";

    [ObservableProperty]
    private bool rememberAboutMe = true;

    [ObservableProperty]
    private bool speakReplies = true;

    [ObservableProperty]
    private bool familyMode;

    /// <summary>
    /// VoiceStudio voice-profile name wuse speaks through (Shaun's cloned
    /// voice). Empty disables it in favor of the Windows system voice.
    /// </summary>
    [ObservableProperty]
    private string voiceStudioProfile = "Shaun";

    [ObservableProperty]
    private bool memoryBookEnabled = true;

    /// <summary>
    /// True when a family memory book file is installed on this PC. The
    /// toggle below is only interactive when this is true.
    /// </summary>
    public bool IsMemoryBookAvailable => MemoryBookService.Instance.IsAvailable;

    private void OnMemoryBookLoaded(object? sender, EventArgs e)
    {
        MemoryBookService.Instance.Loaded -= OnMemoryBookLoaded;
        OnPropertyChanged(nameof(IsMemoryBookAvailable));
    }

    [ObservableProperty]
    private string newFactText = string.Empty;

    public ObservableCollection<string> Facts { get; } = new();

    public IReadOnlyList<string> ThemeOptions { get; } = new[] { "System", "Light", "Dark" };

    /// <summary>All voices installed on this PC.</summary>
    public IReadOnlyList<VoiceInformation> Voices => SpeechService.Instance.AvailableVoices;

    /// <summary>The voice wuse speaks with; persisted by the speech service.</summary>
    public VoiceInformation? SelectedVoice
    {
        get => SpeechService.Instance.SelectedVoice;
        set
        {
            SpeechService.Instance.SelectedVoice = value;
            OnPropertyChanged();
        }
    }

    public PreferencesViewModel(SettingsService settings)
    {
        _settings = settings;
        var stored = settings.GetUserFacts();
        assistantName = stored.AssistantName;
        themeName = settings.ThemeName;
        rememberAboutMe = settings.RememberAboutMe;
        speakReplies = settings.VoiceReplies;
        familyMode = settings.FamilyMode;
        voiceStudioProfile = settings.VoiceStudioProfile;
        memoryBookEnabled = settings.MemoryBookEnabled;
        MemoryBookService.Instance.Loaded += OnMemoryBookLoaded;
        foreach (var fact in stored.Facts)
            Facts.Add(fact);
    }

    partial void OnAssistantNameChanged(string value)
    {
        var name = string.IsNullOrWhiteSpace(value) ? "wuse" : value.Trim();
        _settings.AssistantName = name;
        PersistFacts();
    }

    partial void OnThemeNameChanged(string value)
    {
        _settings.ThemeName = value;
        App.ApplyTheme(value);
    }

    partial void OnRememberAboutMeChanged(bool value)
    {
        _settings.RememberAboutMe = value;
    }

    partial void OnSpeakRepliesChanged(bool value)
    {
        _settings.VoiceReplies = value;
    }

    partial void OnFamilyModeChanged(bool value)
    {
        _settings.FamilyMode = value;
    }

    partial void OnVoiceStudioProfileChanged(string value)
    {
        _settings.VoiceStudioProfile = value ?? string.Empty;
    }

    partial void OnMemoryBookEnabledChanged(bool value)
    {
        _settings.MemoryBookEnabled = value;
    }

    [RelayCommand]
    private void AddFact()
    {
        var fact = NewFactText.Trim();
        if (fact.Length == 0) return;
        if (!Facts.Contains(fact))
        {
            Facts.Add(fact);
            PersistFacts();
        }
        NewFactText = string.Empty;
    }

    [RelayCommand]
    private void RemoveFact(string? fact)
    {
        if (!string.IsNullOrEmpty(fact) && Facts.Remove(fact))
            PersistFacts();
    }

    private void PersistFacts()
    {
        _settings.SaveUserFacts(new UserFacts
        {
            AssistantName = AssistantName,
            Facts = Facts.ToList(),
        });
    }
}
