using RallyPilot.Models;
using RallyPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace RallyPilot.Views;

public sealed partial class OrganizersPage : Page
{
    private readonly JsonListStore<Organizer> _store = new("organizers.json");
    private List<Organizer> _orgs = new();
    private Organizer? _selected;

    public OrganizersPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _orgs = (await _store.LoadAsync()).OrderBy(o => o.Company).ToList();
        OrgList.ItemsSource = _orgs;
        if (_orgs.Count > 0) OrgList.SelectedIndex = 0;
    }

    private async void OrgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = OrgList.SelectedItem as Organizer;
        if (_selected is null) return;
        CompanyBox.Text = _selected.Company;
        ContactNameBox.Text = _selected.ContactName;
        ContactBox.Text = _selected.Contact;
        SizeBox.Text = _selected.TypicalSize;
        AnnualBox.Text = _selected.BooksAnnually;
        NotesBox.Text = _selected.Notes;
        var events = await new JsonListStore<RallyEvent>("events.json").LoadAsync();
        var mine = events.Where(ev => ev.CompanyLabel == _selected.Company).ToList();
        var sales = await new JsonListStore<AddOnSale>("addonsales.json").LoadAsync();
        decimal ltv = 0m;
        foreach (var ev in mine)
            ltv += ev.QuotedTotal + sales.Where(s => s.EventId == ev.Id).Sum(s => s.LineTotal);
        LtvText.Text = mine.Count == 0 ? "No events on file for this organizer yet."
            : $"Lifetime value {ltv:0.00} across {mine.Count} event(s)." +
              (mine.Any(ev => ev.RebookPromptDate is not null)
                ? $" Next rebook prompt: {mine.Where(ev => ev.RebookPromptDate is not null).Min(ev => ev.RebookPromptDate):MMM d, yyyy}." : string.Empty);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyBox.Text)) { ErrorText.Text = "Name the company."; return; }
        var o = _selected ?? new Organizer();
        o.Company = CompanyBox.Text.Trim();
        o.ContactName = ContactNameBox.Text.Trim();
        o.Contact = ContactBox.Text.Trim();
        o.TypicalSize = SizeBox.Text.Trim();
        o.BooksAnnually = AnnualBox.Text.Trim();
        o.Notes = NotesBox.Text.Trim();
        if (_selected is null) _orgs.Add(o);
        await _store.SaveAsync(_orgs);
        _orgs = _orgs.OrderBy(x => x.Company).ToList();
        OrgList.ItemsSource = _orgs;
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        OrgList.SelectedItem = null;
        CompanyBox.Text = ContactNameBox.Text = ContactBox.Text = SizeBox.Text = AnnualBox.Text = NotesBox.Text = string.Empty;
        LtvText.Text = string.Empty;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _orgs.Remove(_selected);
        await _store.SaveAsync(_orgs);
        _selected = null;
        OrgList.ItemsSource = _orgs;
    }
}
