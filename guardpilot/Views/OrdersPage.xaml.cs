using System.Net;
using System.Text;
using GuardPilot.Models;
using GuardPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GuardPilot.Views;

public sealed partial class OrdersPage : Page
{
    private readonly JsonListStore<PostOrderRevision> _store = new("orders.json");
    private readonly JsonListStore<Post> _postStore = new("posts.json");
    private readonly GuardSettingsStore _settingsStore = new();
    private List<PostOrderRevision> _items = new();
    private List<Post> _posts = new();

    public OrdersPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _posts = await _postStore.LoadAsync();
        PostPicker.ItemsSource = _posts.Select(p => p.DisplayLine).ToList();
        _items = await _store.LoadAsync();
        if (_posts.Count > 0) PostPicker.SelectedIndex = 0;
        RefreshHistory();
    }

    private Post? CurrentPost =>
        PostPicker.SelectedIndex >= 0 && PostPicker.SelectedIndex < _posts.Count
            ? _posts[PostPicker.SelectedIndex] : null;

    private void RefreshHistory()
    {
        if (CurrentPost is null) { HistoryList.ItemsSource = null; return; }
        var mine = _items.Where(r => r.PostId == CurrentPost.Id).OrderByDescending(r => r.Version).ToList();
        HistoryList.ItemsSource = mine;
        var current = mine.FirstOrDefault(r => r.Active);
        if (current is not null)
        {
            OrdersBox.Text = current.OrdersText;
            InstructionsBox.Text = current.SiteInstructions;
            EmergencyBox.Text = current.EmergencyContacts;
            EscalationBox.Text = current.EscalationChain;
            DetailText.Text = $"In force: v{current.Version}, effective {current.EffectiveDate:MMM d, yyyy}. Edit the text and publish to create v{current.Version + 1}.";
        }
        else
        {
            DetailText.Text = "No orders published for this post yet.";
        }
    }

    private void PostPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshHistory();

    private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HistoryList.SelectedItem is not PostOrderRevision rev) return;
        DetailText.Text =
            $"v{rev.Version} - effective {rev.EffectiveDate:MMM d, yyyy}{(rev.Active ? " - IN FORCE" : " - superseded")}" +
            $"{Environment.NewLine}{Environment.NewLine}ORDERS: {rev.OrdersText}" +
            $"{Environment.NewLine}{Environment.NewLine}SITE: {rev.SiteInstructions}" +
            $"{Environment.NewLine}{Environment.NewLine}EMERGENCY: {rev.EmergencyContacts}" +
            $"{Environment.NewLine}{Environment.NewLine}ESCALATION: {rev.EscalationChain}";
    }

    private async void PublishButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPost is null) return;
        var mine = _items.Where(r => r.PostId == CurrentPost.Id).ToList();
        foreach (var r in mine) r.Active = false;
        var version = mine.Count > 0 ? mine.Max(r => r.Version) + 1 : 1;
        _items.Add(new PostOrderRevision
        {
            PostId = CurrentPost.Id,
            PostLabel = CurrentPost.DisplayLine,
            Version = version,
            EffectiveDate = DateTimeOffset.Now,
            OrdersText = OrdersBox.Text.Trim(),
            SiteInstructions = InstructionsBox.Text.Trim(),
            EmergencyContacts = EmergencyBox.Text.Trim(),
            EscalationChain = EscalationBox.Text.Trim(),
            Active = true,
        });
        await _store.SaveAsync(_items);
        RefreshHistory();
        StatusText.Text = $"Revision v{version} is now in force for {CurrentPost.DisplayLine}.";
    }

    private static string Esc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

    private async void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPost is null) return;
        var current = _items.Where(r => r.PostId == CurrentPost.Id && r.Active)
            .OrderByDescending(r => r.Version).FirstOrDefault();
        if (current is null) { StatusText.Text = "Publish orders first."; return; }
        var settings = await _settingsStore.LoadAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Post orders</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}h1{font-size:21px}h2{font-size:15px;margin-top:18px}.muted{color:#666}</style></head><body>");
        sb.AppendLine($"<h1>{Esc(settings.CompanyName)} - Post orders</h1>");
        sb.AppendLine($"<p><strong>Post:</strong> {Esc(current.PostLabel)}<br/><strong>Revision:</strong> v{current.Version}, effective {current.EffectiveDate:MMM d, yyyy}</p>");
        sb.AppendLine($"<h2>Standing orders</h2><p>{Esc(current.OrdersText)}</p>");
        sb.AppendLine($"<h2>Site instructions</h2><p>{Esc(current.SiteInstructions)}</p>");
        sb.AppendLine($"<h2>Emergency contacts</h2><p>{Esc(current.EmergencyContacts)}</p>");
        sb.AppendLine($"<h2>Escalation chain</h2><p>{Esc(current.EscalationChain)}</p>");
        sb.AppendLine("<p class=\"muted\">Print for the site binder. This sheet is generated from the office system of record; the revision in force is the one worked.</p>");
        sb.AppendLine("</body></html>");
        var picker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        picker.SuggestedFileName = $"post-orders-v{current.Version}.html";
        if (App.WindowInstance is not null)
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance));
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteTextAsync(file, sb.ToString());
        StatusText.Text = $"Orders sheet saved to {file.Path}";
    }
}
