using System.Globalization;
using EntityPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EntityPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly EntitySettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        OwnerBox.Text = s.OwnerName;
        OrangeBox.Text = s.OrangeDays.ToString(CultureInfo.InvariantCulture);
        TemplatesBox.Text = string.Join(Environment.NewLine, s.StateTemplates);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.OwnerName = OwnerBox.Text.Trim();
        if (int.TryParse(OrangeBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days)) s.OrangeDays = days;
        s.StateTemplates = TemplatesBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
