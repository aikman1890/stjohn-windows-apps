using WheelPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WheelPilot.Views;

public sealed partial class SettingsPage : Page
{
    private readonly WheelSettingsStore _store = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        SchoolBox.Text = s.SchoolName;
        AddressBox.Text = s.Address;
        PhoneBox.Text = s.Phone;
        PackagesBox.Text = string.Join(Environment.NewLine, s.Packages);
        SkillsBox.Text = string.Join(Environment.NewLine, s.Skills);
        CoursesBox.Text = string.Join(Environment.NewLine, s.Courses);
    }

    private static List<string> Lines(string text) =>
        text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.SchoolName = SchoolBox.Text.Trim();
        s.Address = AddressBox.Text.Trim();
        s.Phone = PhoneBox.Text.Trim();
        s.Packages = Lines(PackagesBox.Text);
        s.Skills = Lines(SkillsBox.Text);
        s.Courses = Lines(CoursesBox.Text);
        await _store.SaveAsync(s);
        SavedText.Text = "Saved.";
    }
}
