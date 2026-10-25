using System.Globalization;
using RangePilot.Models;
using RangePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RangePilot.Views;

public sealed partial class BookingsPage : Page
{
    private readonly JsonListStore<Booking> _store = new("bookings.json");
    private readonly JsonListStore<ExperiencePackage> _pkgStore = new("packages.json");
    private readonly JsonListStore<GiftCard> _cardStore = new("giftcards.json");
    private List<Booking> _bookings = new();
    private List<ExperiencePackage> _packages = new();
    private Booking? _selected;

    public BookingsPage()
    {
        this.InitializeComponent();
        SourceBox.ItemsSource = new[] { "Direct", "Phone", "Groupon", "LivingSocial", "Auction", "Gift card" };
        StatusBox.ItemsSource = new[] { "Booked", "Completed", "Cancelled", "No-show" };
        SlotPicker.Date = DateTimeOffset.Now;
        TimeBox.Text = "10:00";
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _packages = (await _pkgStore.LoadAsync()).OrderBy(p => p.Name).ToList();
        PackageBox.ItemsSource = _packages.Select(p => p.Name).ToList();
        PackageList.ItemsSource = _packages;
        _bookings = (await _store.LoadAsync()).OrderByDescending(b => b.SlotDate).ToList();
        BookingList.ItemsSource = _bookings;
        CardList.ItemsSource = await _cardStore.LoadAsync();
        await WarnDownAssetsAsync();
    }

    private async Task WarnDownAssetsAsync()
    {
        var assets = await new JsonListStore<AssetItem>("assets.json").LoadAsync();
        var down = assets.Where(a => a.Status != "Up").ToList();
        var hits = _bookings.Where(b => b.Status == "Booked" &&
            down.Any(d => b.AssetLabel.Contains(d.Name, StringComparison.OrdinalIgnoreCase))).ToList();
        DownWarningText.Text = hits.Count == 0 ? string.Empty
            : "ASSET WARNING: " + string.Join(" ", hits.Select(b =>
                $"{b.LeadGuest} ({b.SlotDate:MMM d} {b.SlotTimeNote}) is booked against a down/maintenance asset ({b.AssetLabel}) - swap it or call ahead."));
    }

    private void PackageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PackageBox.SelectedIndex < 0 || PackageBox.SelectedIndex >= _packages.Count) return;
        var pkg = _packages[PackageBox.SelectedIndex];
        TotalBox.Text = pkg.Price.ToString(CultureInfo.InvariantCulture);
        AssetBox.Text = pkg.AssetsRequired;
    }

    private void BookingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = BookingList.SelectedItem as Booking;
        if (_selected is null) return;
        PackageBox.SelectedItem = _selected.PackageLabel;
        GuestBox.Text = _selected.LeadGuest;
        SlotPicker.Date = _selected.SlotDate;
        TimeBox.Text = _selected.SlotTimeNote;
        PartyBox.Text = _selected.PartySize.ToString(CultureInfo.InvariantCulture);
        SourceBox.SelectedItem = _selected.Source;
        TotalBox.Text = _selected.TotalPrice.ToString(CultureInfo.InvariantCulture);
        DepositBox.Text = _selected.DepositPaid.ToString(CultureInfo.InvariantCulture);
        AssetBox.Text = _selected.AssetLabel;
        StaffBox.Text = _selected.StaffNote;
        StatusBox.SelectedItem = _selected.Status;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GuestBox.Text)) { ErrorText.Text = "Name the lead guest."; return; }
        var assets = await new JsonListStore<AssetItem>("assets.json").LoadAsync();
        if (!string.IsNullOrWhiteSpace(AssetBox.Text) && (_selected is null || _selected.Status != StatusBox.SelectedItem as string || true))
        {
            var downHit = assets.FirstOrDefault(a => a.Status == "Down" &&
                AssetBox.Text.Contains(a.Name, StringComparison.OrdinalIgnoreCase));
            if (downHit is not null && StatusBox.SelectedItem as string == "Booked")
            {
                ErrorText.Text = $"BLOCKED: {downHit.Name} is DOWN. A down asset blocks new bookings against it - pick an up asset or log the swap first.";
                return;
            }
        }
        var b = _selected ?? new Booking();
        b.PackageLabel = PackageBox.SelectedItem as string ?? b.PackageLabel;
        b.LeadGuest = GuestBox.Text.Trim();
        b.SlotDate = SlotPicker.Date ?? b.SlotDate;
        b.SlotTimeNote = TimeBox.Text.Trim();
        if (int.TryParse(PartyBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var size)) b.PartySize = size;
        b.Source = SourceBox.SelectedItem as string ?? b.Source;
        if (decimal.TryParse(TotalBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var total)) b.TotalPrice = total;
        if (decimal.TryParse(DepositBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var dep)) b.DepositPaid = dep;
        b.AssetLabel = AssetBox.Text.Trim();
        b.StaffNote = StaffBox.Text.Trim();
        b.Status = StatusBox.SelectedItem as string ?? b.Status;
        if (_selected is null) _bookings.Add(b);
        await _store.SaveAsync(_bookings);
        _bookings = _bookings.OrderByDescending(x => x.SlotDate).ToList();
        BookingList.ItemsSource = _bookings;
        await WarnDownAssetsAsync();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        BookingList.SelectedItem = null;
        GuestBox.Text = DepositBox.Text = StaffBox.Text = string.Empty;
        SlotPicker.Date = DateTimeOffset.Now;
        TimeBox.Text = "10:00";
        StatusBox.SelectedItem = "Booked";
        SourceBox.SelectedItem = "Direct";
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _bookings.Remove(_selected);
        await _store.SaveAsync(_bookings);
        _selected = null;
        BookingList.ItemsSource = _bookings;
    }

    private async void PkgButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PkgNameBox.Text)) { ErrorText.Text = "Name the package."; return; }
        _packages.Add(new ExperiencePackage
        {
            Name = PkgNameBox.Text.Trim(),
            Price = decimal.TryParse(PkgPriceBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m,
            Capacity = int.TryParse(PkgCapBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cap) ? cap : 6,
            AssetsRequired = PkgAssetsBox.Text.Trim(),
        });
        await _pkgStore.SaveAsync(_packages);
        PackageBox.ItemsSource = _packages.Select(p => p.Name).ToList();
        PackageList.ItemsSource = _packages;
        PkgNameBox.Text = PkgPriceBox.Text = PkgCapBox.Text = PkgAssetsBox.Text = string.Empty;
    }

    private async void CardButton_Click(object sender, RoutedEventArgs e)
    {
        var cards = await _cardStore.LoadAsync();
        var existing = cards.FirstOrDefault(c => c.Code == CardCodeBox.Text.Trim());
        if (existing is not null)
        {
            if (decimal.TryParse(CardRedeemBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var redeem))
                existing.Redeemed += redeem;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(CardCodeBox.Text)) { ErrorText.Text = "A card code is required."; return; }
            cards.Add(new GiftCard
            {
                Code = CardCodeBox.Text.Trim(), IssuedTo = CardToBox.Text.Trim(),
                Amount = decimal.TryParse(CardAmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0m,
                IssuedDate = DateTimeOffset.Now,
            });
        }
        await _cardStore.SaveAsync(cards);
        CardList.ItemsSource = cards;
        CardRedeemBox.Text = string.Empty;
    }
}
