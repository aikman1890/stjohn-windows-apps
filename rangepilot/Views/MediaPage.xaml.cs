using System.Globalization;
using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class MediaPage : Page
{
    private readonly JsonListStore<MediaItem> _store = new("media.json");
    private List<MediaItem> _media = new();
    private MediaItem? _selected;

    public MediaPage()
    {
        this.InitializeComponent();
        KindBox.ItemsSource = new[] { "Photo package", "Video package", "Photo + video" };
        CapturedBox.ItemsSource = new[] { "Yes", "Not yet" };
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _media = (await _store.LoadAsync()).OrderBy(m => m.Delivered).ThenByDescending(m => m.EventDate).ToList();
        MediaList.ItemsSource = _media;
    }

    private void MediaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = MediaList.SelectedItem as MediaItem;
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GuestBox.Text)) { ErrorText.Text = "Name the guest/party."; return; }
        _media.Add(new MediaItem
        {
            GuestLabel = GuestBox.Text.Trim(),
            BookingLabel = GuestBox.Text.Trim(),
            Kind = KindBox.SelectedItem as string ?? "Video package",
            Price = decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m,
            EventDate = DateTimeOffset.Now,
            Captured = CapturedBox.SelectedItem as string == "Yes",
        });
        await _store.SaveAsync(_media);
        MediaList.ItemsSource = _media;
        GuestBox.Text = PriceBox.Text = string.Empty;
    }

    private async void DeliverButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the media item."; return; }
        _selected.DeliveredDate = DateTimeOffset.Now;
        await _store.SaveAsync(_media);
        MediaList.ItemsSource = null;
        MediaList.ItemsSource = _media;
    }
}
