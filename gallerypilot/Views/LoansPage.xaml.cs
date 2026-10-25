using GalleryPilot.Models;
using GalleryPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GalleryPilot.Views;

public sealed partial class LoansPage : Page
{
    private readonly JsonListStore<Loan> _store = new("loans.json");
    private readonly JsonListStore<Exhibition> _exStore = new("exhibitions.json");
    private readonly JsonListStore<Artwork> _workStore = new("artworks.json");
    private List<Loan> _loans = new();
    private List<Artwork> _works = new();
    private Loan? _selected;

    public LoansPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _works = (await _workStore.LoadAsync()).OrderBy(w => w.Artist).ToList();
        WorkBox.ItemsSource = _works.Select(w => $"{w.Artist} - \"{w.Title}\"").ToList();
        _loans = (await _store.LoadAsync()).OrderBy(l => l.ReturnDue).ToList();
        LoanList.ItemsSource = _loans;
        ExhibitionList.ItemsSource = (await _exStore.LoadAsync()).OrderBy(x => x.StartDate).ToList();
        if (_loans.Count > 0) LoanList.SelectedIndex = 0;
    }

    private void LoanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = LoanList.SelectedItem as Loan;
        if (_selected is null) return;
        WorkBox.SelectedItem = _works.Select(w => $"{w.Artist} - \"{w.Title}\"").FirstOrDefault(x => _selected.ArtworkLabel.Contains(x.Split(" - ")[^1].Trim('"')));
        BorrowerBox.Text = _selected.Borrower;
        VenueBox.Text = _selected.Venue;
        StartPicker.Date = _selected.StartDate;
        DuePicker.Date = _selected.ReturnDue;
        CondOutCheck.IsChecked = _selected.ConditionOutLogged;
        CondInCheck.IsChecked = _selected.ConditionInLogged;
        LoanNotesBox.Text = _selected.Notes;
    }

    private async void SaveLoanButton_Click(object sender, RoutedEventArgs e)
    {
        var wi = WorkBox.SelectedIndex;
        if (wi < 0 || wi >= _works.Count) { ErrorText.Text = "Pick the work."; return; }
        var loan = _selected ?? new Loan();
        var work = _works[wi];
        loan.ArtworkId = work.Id;
        loan.ArtworkLabel = $"{work.Artist} - \"{work.Title}\"";
        loan.Borrower = BorrowerBox.Text.Trim();
        loan.Venue = VenueBox.Text.Trim();
        loan.StartDate = StartPicker.Date ?? loan.StartDate;
        loan.ReturnDue = DuePicker.Date ?? loan.ReturnDue;
        loan.ConditionOutLogged = CondOutCheck.IsChecked == true;
        loan.ConditionInLogged = CondInCheck.IsChecked == true;
        loan.Notes = LoanNotesBox.Text.Trim();
        if (_selected is null) _loans.Add(loan);
        await _store.SaveAsync(_loans);
        _loans = _loans.OrderBy(l => l.ReturnDue).ToList();
        LoanList.ItemsSource = _loans;
    }

    private async void CloseLoanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { ErrorText.Text = "Select the loan to close."; return; }
        if (!(CondOutCheck.IsChecked == true && CondInCheck.IsChecked == true))
        {
            ErrorText.Text = "Not closed: both condition entries (out AND in) must be logged first. Write them on the work's record under Inventory, then tick both boxes.";
            return;
        }
        _selected.ConditionOutLogged = true;
        _selected.ConditionInLogged = true;
        _selected.ReturnedDate = DateTimeOffset.Now;
        await _store.SaveAsync(_loans);
        var works = await _workStore.LoadAsync();
        var work = works.FirstOrDefault(w => w.Id == _selected.ArtworkId);
        if (work is not null && work.Location == "On loan") { work.Location = "Gallery"; await _workStore.SaveAsync(works); }
        LoanList.ItemsSource = null;
        LoanList.ItemsSource = _loans;
        LoanList.SelectedItem = _selected;
    }

    private void NewLoanButton_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        LoanList.SelectedItem = null;
        BorrowerBox.Text = VenueBox.Text = LoanNotesBox.Text = string.Empty;
        CondOutCheck.IsChecked = CondInCheck.IsChecked = false;
        StartPicker.Date = DateTimeOffset.Now;
        DuePicker.Date = DateTimeOffset.Now.AddMonths(2);
    }

    private async void DeleteLoanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        _loans.Remove(_selected);
        await _store.SaveAsync(_loans);
        _selected = null;
        LoanList.ItemsSource = _loans;
    }

    private async void ExButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ExTitleBox.Text)) { ErrorText.Text = "Name the exhibition."; return; }
        var exhibitions = await _exStore.LoadAsync();
        exhibitions.Add(new Exhibition
        {
            Title = ExTitleBox.Text.Trim(), Venue = ExVenueBox.Text.Trim(),
            WorksNote = ExWorksBox.Text.Trim(),
            StartDate = DateTimeOffset.Now, EndDate = DateTimeOffset.Now.AddDays(42),
        });
        await _exStore.SaveAsync(exhibitions);
        ExhibitionList.ItemsSource = exhibitions.OrderBy(x => x.StartDate).ToList();
        ExTitleBox.Text = ExVenueBox.Text = ExWorksBox.Text = string.Empty;
    }
}
