using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private string newFactText = string.Empty;

    public ObservableCollection<string> Facts { get; } = new();

    public IReadOnlyList<string> ThemeOptions { get; } = new[] { "System", "Light", "Dark" };

    public PreferencesViewModel(SettingsService settings)
    {
        _settings = settings;
        var stored = settings.GetUserFacts();
        assistantName = stored.AssistantName;
        themeName = settings.ThemeName;
        rememberAboutMe = settings.RememberAboutMe;
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
