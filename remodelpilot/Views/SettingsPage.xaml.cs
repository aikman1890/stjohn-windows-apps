using RemodelPilot.Models;
using RemodelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RemodelPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly RemodelSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        NameBox.Text = s.BusinessName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        MarkupBox.Text = s.DefaultMarkupPercent.ToString("0.#");
        CategoriesBox.Text = string.Join(Environment.NewLine, s.Categories);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var categories = CategoriesBox.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim()).Where(c => c.Length > 0).Distinct().ToList();
        var s = new RemodelSettings
        {
            BusinessName = NameBox.Text.Trim(),
            Address = AddressBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            DefaultMarkupPercent = decimal.TryParse(MarkupBox.Text.Trim(), out var v) ? v : 20m,
            Categories = categories.Count > 0 ? categories : new RemodelSettings().Categories,
        };
        await _store.SaveAsync(s);
        StatusText.Text = "Settings saved.";
    }
}
