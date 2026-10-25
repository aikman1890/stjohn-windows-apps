using VenuePilot.Models;
using VenuePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace VenuePilot.Views;

public sealed class BookLine
{
    public string Text { get; set; } = string.Empty;
    public Brush Brush { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.Black);
}

public sealed partial class DateBookPage : Page
{
    private readonly JsonListStore<VenueEvent> _store = new("events.json");
    private readonly VenueSettingsStore _settingsStore = new();
    private List<VenueEvent> _events = new();
    private List<string> _spaces = new();
    private DateTimeOffset _month = new(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, 1, 0, 0, 0, TimeSpan.Zero);

    public DateBookPage()
    {
        this.InitializeComponent();
        MonthPicker.Date = _month;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _spaces = settings.Spaces;
        _events = await _store.LoadAsync();
        Render();
    }

    private void Render()
    {
        MonthText.Text = _month.ToString("MMMM yyyy");
        var lines = new List<BookLine>();
        var days = DateTime.DaysInMonth(_month.Year, _month.Month);
        for (var d = 1; d <= days; d++)
        {
            var date = new DateTimeOffset(_month.Year, _month.Month, d, 0, 0, 0, TimeSpan.Zero);
            var dayEvents = _events.Where(x => x.EventDate.Date == date.Date && x.Status != "Cancelled")
                .OrderBy(x => x.Space).ToList();
            if (dayEvents.Count == 0) continue;
            foreach (var ev in dayEvents)
            {
                lines.Add(new BookLine
                {
                    Text = $"{date:ddd, MMM d} - {ev.Space}: {ev.ClientNames} ({ev.EventType}) - {ev.Status}" +
                           (ev.HoldExpired ? $" - HOLD EXPIRED {ev.HoldExpiry:MMM d}" : ev.Status == "Hold" ? $" - hold to {ev.HoldExpiry:MMM d}" : string.Empty),
                    Brush = ev.StatusBrush,
                });
            }
        }
        var bookedSpaces = string.Join(", ", _spaces.Select(s =>
            $"{s}: {_events.Count(x => x.Space == s && x.Status == "Contracted" && x.EventDate.Year == _month.Year && x.EventDate.Month == _month.Month)} contracted"));
        lines.Insert(0, new BookLine { Text = $"This month - {bookedSpaces}" });
        if (lines.Count == 1) lines.Add(new BookLine { Text = "No events on the book this month." });
        BookList.ItemsSource = lines;
    }

    private void MonthPicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
    {
        if (e.NewDate is { } d)
        {
            _month = new DateTimeOffset(d.Year, d.Month, 1, 0, 0, 0, TimeSpan.Zero);
            Render();
        }
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        _month = _month.AddMonths(-1);
        MonthPicker.Date = _month;
        Render();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        _month = _month.AddMonths(1);
        MonthPicker.Date = _month;
        Render();
    }
}
