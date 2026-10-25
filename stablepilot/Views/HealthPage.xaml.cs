using System.Globalization;
using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace StablePilot.Views;

public sealed partial class HealthPage : Page
{
    private readonly JsonListStore<HealthEntry> _healthStore = new("health.json");
    private readonly JsonListStore<SessionEntry> _sessionStore = new("sessions.json");
    private readonly JsonListStore<Horse> _horseStore = new("horses.json");
    private List<Horse> _horses = new();

    public HealthPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Vet visit", "Injury", "Treatment", "Lameness", "Note" };
        SessionKindBox.ItemsSource = new[] { "Training session", "Lesson", "Exercise ride" };
        HorseFilterBox.SelectionChanged += async (_, _) => await RefreshAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _horses = (await _horseStore.LoadAsync()).OrderBy(h => h.Name).ToList();
        HorseFilterBox.ItemsSource = _horses.Select(h => h.Name).ToList();
        if (_horses.Count > 0) HorseFilterBox.SelectedIndex = 0;
        await RefreshAsync();
    }

    private Horse? Current =>
        HorseFilterBox.SelectedIndex >= 0 && HorseFilterBox.SelectedIndex < _horses.Count
            ? _horses[HorseFilterBox.SelectedIndex] : null;

    private async Task RefreshAsync()
    {
        if (Current is null) { HealthList.ItemsSource = null; SessionList.ItemsSource = null; return; }
        var health = await _healthStore.LoadAsync();
        HealthList.ItemsSource = health.Where(h => h.HorseId == Current.Id)
            .OrderByDescending(h => h.Date).ToList();
        var sessions = await _sessionStore.LoadAsync();
        SessionList.ItemsSource = sessions.Where(s => s.HorseId == Current.Id)
            .OrderByDescending(s => s.Date).ToList();
    }

    private async void AddHealthButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) { ErrorText.Text = "Pick the horse."; return; }
        if (string.IsNullOrWhiteSpace(DetailBox.Text)) { ErrorText.Text = "Write the entry."; return; }
        var health = await _healthStore.LoadAsync();
        health.Add(new HealthEntry
        {
            HorseId = Current.Id, HorseLabel = Current.Name,
            Kind = KindBox.SelectedItem as string ?? "Note",
            Detail = DetailBox.Text.Trim(), Provider = ProviderBox.Text.Trim(),
            Date = DateTimeOffset.Now,
        });
        await _healthStore.SaveAsync(health);
        DetailBox.Text = ProviderBox.Text = string.Empty;
        await RefreshAsync();
    }

    private async void AddSessionButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) { ErrorText.Text = "Pick the horse."; return; }
        var sessions = await _sessionStore.LoadAsync();
        sessions.Add(new SessionEntry
        {
            HorseId = Current.Id, HorseLabel = Current.Name,
            Trainer = TrainerBox.Text.Trim(),
            Kind = SessionKindBox.SelectedItem as string ?? "Training session",
            Fee = decimal.TryParse(FeeBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var fee) ? fee : 0m,
            Note = SessionNoteBox.Text.Trim(), Date = DateTimeOffset.Now,
        });
        await _sessionStore.SaveAsync(sessions);
        SessionNoteBox.Text = string.Empty;
        await RefreshAsync();
    }
}
