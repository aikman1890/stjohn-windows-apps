using System.Net;
using System.Text;
using RallyPilot.Models;
using RallyPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace RallyPilot.Views;

public sealed partial class RunOfDayPage : Page
{
    private readonly JsonListStore<RunBlock> _store = new("runblocks.json");
    private readonly JsonListStore<RallyEvent> _eventStore = new("events.json");
    private List<RallyEvent> _events = new();

    public RunOfDayPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Activity", "Meal", "Buffer", "Briefing", "Transport" };
        EventBox.SelectionChanged += async (_, _) => await RefreshAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _events = (await _eventStore.LoadAsync()).OrderBy(ev => ev.EventDate).ToList();
        EventBox.ItemsSource = _events.Select(ev => $"{ev.EventDate:MMM d} - {ev.CompanyLabel}").ToList();
        if (_events.Count > 0) EventBox.SelectedIndex = 0;
        await RefreshAsync();
    }

    private RallyEvent? Current =>
        EventBox.SelectedIndex >= 0 && EventBox.SelectedIndex < _events.Count
            ? _events[EventBox.SelectedIndex] : null;

    private async Task<List<RunBlock>> MineAsync()
    {
        if (Current is null) return new List<RunBlock>();
        var all = await _store.LoadAsync();
        return all.Where(b => b.EventId == Current.Id).OrderBy(b => b.StartNote).ToList();
    }

    private async Task RefreshAsync()
    {
        BlockList.ItemsSource = await MineAsync();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) { ErrorText.Text = "Pick the event."; return; }
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) { ErrorText.Text = "Name the block."; return; }
        var all = await _store.LoadAsync();
        all.Add(new RunBlock
        {
            EventId = Current.Id, StartNote = StartBox.Text.Trim(), EndNote = EndBox.Text.Trim(),
            Title = TitleBox.Text.Trim(), Kind = KindBox.SelectedItem as string ?? "Activity",
            GroupLabel = GroupBox.Text.Trim(), StaffLabel = StaffBox.Text.Trim(),
            AssetStatusNote = AssetNoteBox.Text.Trim(),
        });
        await _store.SaveAsync(all);
        TitleBox.Text = GroupBox.Text = StaffBox.Text = string.Empty;
        await RefreshAsync();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (Current is null) { ErrorText.Text = "Pick the event."; return; }
        var settings = await new RallySettingsStore().LoadAsync();
        var blocks = await MineAsync();
        var roster = await new JsonListStore<RosterEntry>("roster.json").LoadAsync();
        var mine = roster.Where(r => r.EventId == Current.Id).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Day sheet</title>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:24px;color:#111}h1{font-size:20px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #999;padding:7px;text-align:left;vertical-align:top}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.BusinessName)} - Day sheet: {Esc(Current.CompanyLabel)} - {Current.EventDate:dddd, MMM d, yyyy}</h1>");
        sb.AppendLine($"<p>Organizer: {Esc(Current.OrganizerLabel)} {Esc(Current.OrganizerContact)} - heads confirmed {Current.ConfirmedHeads} of {Current.ExpectedHeads} expected - waivers signed {mine.Count(r => r.WaiverSigned)} of {mine.Count}.</p>");
        if (!string.IsNullOrWhiteSpace(Current.SpecialRequirements))
            sb.AppendLine($"<p><strong>Special requirements:</strong> {Esc(Current.SpecialRequirements)}</p>");
        sb.AppendLine("<table><tr><th>Time</th><th>Block</th><th>Kind</th><th>Group</th><th>Staff</th><th>Asset status</th></tr>");
        foreach (var b in blocks)
            sb.AppendLine($"<tr><td>{Esc(b.StartNote)}-{Esc(b.EndNote)}</td><td>{Esc(b.Title)}</td><td>{Esc(b.Kind)}</td><td>{Esc(b.GroupLabel)}</td><td>{Esc(b.StaffLabel)}</td><td>{Esc(b.AssetStatusNote)}</td></tr>");
        sb.AppendLine("</table><p>Printed from RallyPilot. Check asset status on the morning and update the board before the group arrives.</p></body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = "day-sheet.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Day sheet saved to {file.Path}";
    }
}
