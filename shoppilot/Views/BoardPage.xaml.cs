using ShopPilot.Models;
using ShopPilot.Services;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace ShopPilot.Views;

public sealed partial class BoardPage : Page
{
    private readonly JsonListStore<ShopJob> _jobStore = new("jobs.json");
    private readonly ShopSettingsStore _settingsStore = new();

    public BoardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        var jobs = (await _jobStore.LoadAsync())
            .Where(j => j.Status is "Open" or "In process" or "Complete").ToList();

        BoardPanel.Children.Clear();
        var centers = settings.WorkCenters
            .Concat(jobs.Select(j => j.WorkCenter)).Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct().ToList();
        foreach (var center in centers)
        {
            var mine = jobs.Where(j => j.WorkCenter == center)
                .OrderBy(j => j.Priority).ThenBy(j => j.DueDate).ToList();
            if (mine.Count == 0) continue;
            BoardPanel.Children.Add(new TextBlock
            {
                Text = $"{center} ({mine.Count})",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 12, 0, 2),
            });
            foreach (var job in mine)
            {
                BoardPanel.Children.Add(new TextBlock
                {
                    Text = $"P{job.Priority} - {job.DisplayLine}",
                    Foreground = job.DueBrush,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Microsoft.UI.Xaml.Thickness(12, 1, 0, 1),
                });
            }
        }
        if (BoardPanel.Children.Count == 0)
            BoardPanel.Children.Add(new TextBlock { Text = "No open jobs on the board." });
    }
}
