using System.Net;
using System.Text;
using StablePilot.Models;
using StablePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace StablePilot.Views;

public sealed partial class BoardPage : Page
{
    private List<Horse> _horses = new();

    public BoardPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _horses = (await new JsonListStore<Horse>("horses.json").LoadAsync())
            .OrderBy(h => h.Stall).ThenBy(h => h.Name).ToList();
        BoardList.ItemsSource = _horses.Select(h =>
            $"STALL {h.Stall} - {h.Name}\n  AM: {h.FeedAm}\n  PM: {h.FeedPm}" +
            (string.IsNullOrWhiteSpace(h.Supplements) ? string.Empty : $"\n  Supplements/meds: {h.Supplements}  [ ] given + initialed") +
            $"\n  Turnout: {h.TurnoutGroup} - Blanket: {h.BlanketRule}").ToList();
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = await new StableSettingsStore().LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Feed board</title>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;margin:24px;color:#111}h1{font-size:20px}table{border-collapse:collapse;width:100%}td,th{border:1px solid #999;padding:7px;text-align:left;vertical-align:top}.sign{width:70px}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.BarnName)} - Feed &amp; turnout board - {DateTimeOffset.Now:dddd, MMM d, yyyy}</h1>");
        sb.AppendLine("<table><tr><th>Stall / Horse</th><th>AM feed</th><th>PM feed</th><th>Supplements / meds</th><th>Turnout</th><th>Blanket</th><th class=\"sign\">Done + initials</th></tr>");
        foreach (var h in _horses)
            sb.AppendLine($"<tr><td>{Esc(h.Stall)} - {Esc(h.Name)}</td><td>{Esc(h.FeedAm)}</td><td>{Esc(h.FeedPm)}</td><td>{Esc(h.Supplements)}</td><td>{Esc(h.TurnoutGroup)}</td><td>{Esc(h.BlanketRule)}</td><td></td></tr>");
        sb.AppendLine("</table><p>Printed from StablePilot. Sign off supplements/meds per horse, every day.</p></body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = "feed-board.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Board saved to {file.Path}";
    }
}
