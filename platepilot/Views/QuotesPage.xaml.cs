using System.Net;
using System.Text;
using PlatePilot.Models;
using PlatePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PlatePilot.Views;

public sealed partial class QuotesPage : Page
{
    private readonly JsonListStore<CateringQuote> _store = new("quotes.json");
    private readonly JsonListStore<Recipe> _recipeStore = new("recipes.json");
    private readonly JsonListStore<Ingredient> _ingredientStore = new("ingredients.json");
    private List<CateringQuote> _items = new();
    private List<Recipe> _recipes = new();
    private List<Ingredient> _ingredients = new();
    private CateringQuote? _selected;
    private List<QuoteLine> _editLines = new();

    public QuotesPage()
    {
        this.InitializeComponent();
        StatusCombo.ItemsSource = new[] { "Draft", "Sent", "Accepted", "Declined", "Done" };
        StatusCombo.SelectedIndex = 0;
        EventPicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _recipes = await _recipeStore.LoadAsync();
        _ingredients = await _ingredientStore.LoadAsync();
        RecipePicker.ItemsSource = _recipes.Select(r => r.Name).ToList();
        if (_recipes.Count > 0) RecipePicker.SelectedIndex = 0;
        _items = await _store.LoadAsync();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _items.OrderByDescending(q => q.EventDate).ToList();
    }

    private void RefreshLines()
    {
        LinesList.ItemsSource = null;
        LinesList.ItemsSource = _editLines.Select(l =>
            $"{l.RecipeName} - {l.Guests} guests @ {l.PricePerGuest:C} = {l.Amount:C}").ToList();
        var other = decimal.TryParse(OtherBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m;
        TotalText.Text = $"Quote total: {_editLines.Sum(l => l.Amount) + other:C}";
    }

    private void RecipePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RecipePicker.SelectedIndex < 0 || RecipePicker.SelectedIndex >= _recipes.Count) return;
        var recipe = _recipes[RecipePicker.SelectedIndex];
        var map = _ingredients.ToDictionary(i => i.Id, i => i.CostPerBaseUnit);
        var cost = recipe.CostWith(map);
        MarginHintText.Text = cost > 0 && recipe.TargetFoodCostPercent > 0
            ? $"{recipe.Name}: plate cost {cost:C}. At target {recipe.TargetFoodCostPercent:0.#}%, the per-guest price works out to {cost / (recipe.TargetFoodCostPercent / 100m):C} - edit freely."
            : $"{recipe.Name}: no costed lines yet.";
        PerGuestBox.Text = recipe.MenuPrice.ToString("0.##");
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (RecipePicker.SelectedIndex < 0 || RecipePicker.SelectedIndex >= _recipes.Count) return;
        _editLines.Add(new QuoteLine
        {
            RecipeName = _recipes[RecipePicker.SelectedIndex].Name,
            Guests = int.TryParse(GuestsBox.Text.Trim(), out var g) ? g : 0,
            PricePerGuest = decimal.TryParse(PerGuestBox.Text.Trim().TrimStart('$'), out var p) ? p : 0m,
        });
        RefreshLines();
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (LinesList.SelectedIndex >= 0 && LinesList.SelectedIndex < _editLines.Count)
        {
            _editLines.RemoveAt(LinesList.SelectedIndex);
            RefreshLines();
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ItemsList.SelectedItem as CateringQuote;
        if (_selected is null) return;
        ClientBox.Text = _selected.ClientName;
        EventPicker.Date = _selected.EventDate;
        StatusCombo.SelectedItem = _selected.Status;
        OtherBox.Text = _selected.OtherCharges.ToString("0.##");
        NotesBox.Text = _selected.Notes;
        _editLines = _selected.Lines.Select(l => new QuoteLine
        {
            RecipeName = l.RecipeName,
            Guests = l.Guests,
            PricePerGuest = l.PricePerGuest,
        }).ToList();
        RefreshLines();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var item = _selected ?? new CateringQuote
        {
            QuoteNumber = $"Q-{_items.Count + 1001}",
        };
        item.ClientName = ClientBox.Text.Trim();
        item.EventDate = EventPicker.Date;
        item.Status = StatusCombo.SelectedItem as string ?? "Draft";
        item.OtherCharges = decimal.TryParse(OtherBox.Text.Trim().TrimStart('$'), out var v) ? v : 0m;
        item.Notes = NotesBox.Text.Trim();
        item.Lines = _editLines;
        if (_selected is null) _items.Add(item);
        await _store.SaveAsync(_items);
        _selected = item;
        RefreshList();
        RefreshLines();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _items.RemoveAll(q => q.Id == _selected.Id);
        await _store.SaveAsync(_items);
        _selected = null;
        ClearForm();
        RefreshList();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ItemsList.SelectedItem = null;
        ClearForm();
    }

    private void ClearForm()
    {
        ClientBox.Text = string.Empty;
        OtherBox.Text = "0";
        NotesBox.Text = string.Empty;
        _editLines = new List<QuoteLine>();
        RefreshLines();
        TotalText.Text = string.Empty;
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null || App.WindowInstance is null) return;
        var settings = await new PlateSettingsStore().LoadAsync();
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"catering-quote-{_selected.QuoteNumber}",
        };
        picker.FileTypeChoices.Add("HTML document", new List<string> { ".html" });
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
        sb.Append($"<title>Catering Quote {_selected.QuoteNumber}</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:40px;color:#222}" +
                  "table{border-collapse:collapse;width:100%;margin-top:16px}" +
                  "th,td{border:1px solid #bbb;padding:8px;text-align:left}th{background:#f0f0f0}.r{text-align:right}.tot{font-weight:bold}</style></head><body>");
        if (!string.IsNullOrWhiteSpace(settings.RestaurantName))
            sb.Append($"<h1>{WebUtility.HtmlEncode(settings.RestaurantName)}</h1><p>{WebUtility.HtmlEncode(settings.Address)}<br>{WebUtility.HtmlEncode(settings.Phone)}</p>");
        sb.Append($"<h2>Catering Quote {_selected.QuoteNumber}</h2>");
        sb.Append($"<p><strong>Client:</strong> {WebUtility.HtmlEncode(_selected.ClientName)}<br><strong>Event date:</strong> {_selected.EventDate:MMMM d, yyyy}</p>");
        sb.Append("<table><tr><th>Dish</th><th class=\"r\">Guests</th><th class=\"r\">Per guest</th><th class=\"r\">Amount</th></tr>");
        foreach (var l in _selected.Lines)
            sb.Append($"<tr><td>{WebUtility.HtmlEncode(l.RecipeName)}</td><td class=\"r\">{l.Guests}</td><td class=\"r\">{l.PricePerGuest:C}</td><td class=\"r\">{l.Amount:C}</td></tr>");
        if (_selected.OtherCharges > 0)
            sb.Append($"<tr><td colspan=\"3\" class=\"r\">Other charges</td><td class=\"r\">{_selected.OtherCharges:C}</td></tr>");
        sb.Append($"<tr class=\"tot\"><td colspan=\"3\" class=\"r\">Total</td><td class=\"r\">{_selected.Total:C}</td></tr>");
        sb.Append("</table>");
        if (!string.IsNullOrWhiteSpace(_selected.Notes))
            sb.Append($"<p>{WebUtility.HtmlEncode(_selected.Notes)}</p>");
        sb.Append("</body></html>");
        await FileIO.WriteTextAsync(file, sb.ToString());
    }
}
