using VenuePilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace VenuePilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly VenueSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        VenueBox.Text = s.VenueName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        SpacesBox.Text = string.Join(Environment.NewLine, s.Spaces);
        PackagesBox.Text = string.Join(Environment.NewLine, s.Packages);
        TypesBox.Text = string.Join(Environment.NewLine, s.EventTypes);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.VenueName = VenueBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        List<string> Lines(string text) => text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        s.Spaces = Lines(SpacesBox.Text);
        s.Packages = Lines(PackagesBox.Text);
        s.EventTypes = Lines(TypesBox.Text);
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
