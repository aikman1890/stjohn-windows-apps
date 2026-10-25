using TaxPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace TaxPilot.Views;

public sealed partial class DeadlinesPage : Page
{
    private readonly TaxSettingsStore _store = new();

    public DeadlinesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var s = await _store.LoadAsync();
        AsOfBox.Text = s.AsOfLabel;
        TableBox.Text = string.Join(Environment.NewLine, s.DeadlineTable);
        TemplatesBox.Text = string.Join(Environment.NewLine, s.DocTemplates);
        StateBox.Text = string.Join(Environment.NewLine, s.StateNotes);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var s = await _store.LoadAsync();
        s.AsOfLabel = AsOfBox.Text.Trim();
        List<string> Lines(string text) => text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        s.DeadlineTable = Lines(TableBox.Text);
        s.DocTemplates = Lines(TemplatesBox.Text);
        s.StateNotes = Lines(StateBox.Text);
        await _store.SaveAsync(s);
        SavedText.Text = "Saved. Deadlines across the app now use these tables.";
    }
}
