using ShopPilot.Models;
using ShopPilot.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ShopPilot.Views;

public sealed partial class HistoryPage : Page
{
    private readonly JsonListStore<ShopCustomer> _customerStore = new("customers.json");
    private readonly JsonListStore<ShopQuote> _quoteStore = new("quotes.json");
    private readonly JsonListStore<ShopJob> _jobStore = new("jobs.json");
    private readonly JsonListStore<JobOperation> _opStore = new("operations.json");
    private readonly JsonListStore<OutsidePO> _poStore = new("outsidepos.json");
    private readonly ShopSettingsStore _settingsStore = new();
    private List<ShopCustomer> _customers = new();
    private decimal _laborRate = 85m;

    public HistoryPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var settings = await _settingsStore.LoadAsync();
        _laborRate = settings.LaborRate;
        await ReloadCustomersAsync();
    }

    private async Task ReloadCustomersAsync()
    {
        _customers = await _customerStore.LoadAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.Name).ToList();
    }

    private decimal JobMargin(ShopJob job, List<JobOperation> ops, List<OutsidePO> pos)
    {
        var labor = ops.Where(o => o.JobId == job.Id).Sum(o => o.ActualHours) * _laborRate;
        var outside = pos.Where(p => p.JobId == job.Id).Sum(p => p.Cost);
        return job.QuotedPrice - (job.MaterialCost + labor + outside);
    }

    private async Task ShowCustomerAsync(ShopCustomer customer)
    {
        var quotes = await _quoteStore.LoadAsync();
        var jobs = await _jobStore.LoadAsync();
        var ops = await _opStore.LoadAsync();
        var pos = await _poStore.LoadAsync();
        var lines = new List<string> { $"CUSTOMER: {customer.Name} - {customer.Contact} - {customer.Email} {customer.Phone}" };
        var decided = quotes.Where(q => q.CustomerId == customer.Id && q.Status is "Won" or "Lost").ToList();
        if (decided.Count > 0)
            lines.Add($"Win rate with this customer: {100.0 * decided.Count(q => q.Status == "Won") / decided.Count:0}% ({decided.Count} decided)");
        lines.AddRange(quotes.Where(q => q.CustomerId == customer.Id).OrderByDescending(q => q.Date)
            .Select(q => $"QUOTE {q.DisplayLine}"));
        lines.AddRange(jobs.Where(j => j.CustomerId == customer.Id).OrderByDescending(j => j.DueDate)
            .Select(j => $"JOB {j.DisplayLine} - margin so far {JobMargin(j, ops, pos):C}"));
        HistoryList.ItemsSource = lines;
    }

    private async void CustomerPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count) return;
        var customer = _customers[CustomerPicker.SelectedIndex];
        NameBox.Text = customer.Name;
        ContactBox.Text = customer.Contact;
        EmailBox.Text = customer.Email;
        PhoneBox.Text = customer.Phone;
        NotesBox.Text = customer.Notes;
        await ShowCustomerAsync(customer);
    }

    private async void SaveCustButton_Click(object sender, RoutedEventArgs e)
    {
        var existing = _customers.FirstOrDefault(c => c.Name == NameBox.Text.Trim());
        var customer = existing ?? new ShopCustomer();
        customer.Name = NameBox.Text.Trim();
        customer.Contact = ContactBox.Text.Trim();
        customer.Email = EmailBox.Text.Trim();
        customer.Phone = PhoneBox.Text.Trim();
        customer.Notes = NotesBox.Text.Trim();
        if (existing is null) _customers.Add(customer);
        await _customerStore.SaveAsync(_customers);
        await ReloadCustomersAsync();
        CustomerPicker.SelectedItem = customer.Name;
    }

    private async void DeleteCustButton_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerPicker.SelectedIndex < 0 || CustomerPicker.SelectedIndex >= _customers.Count) return;
        _customers.RemoveAt(CustomerPicker.SelectedIndex);
        await _customerStore.SaveAsync(_customers);
        await ReloadCustomersAsync();
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var term = PartSearchBox.Text.Trim();
        if (term.Length == 0) return;
        var quotes = await _quoteStore.LoadAsync();
        var jobs = await _jobStore.LoadAsync();
        var ops = await _opStore.LoadAsync();
        var pos = await _poStore.LoadAsync();
        bool Hit(string desc, string no) =>
            desc.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            no.Contains(term, StringComparison.OrdinalIgnoreCase);
        var lines = new List<string> { $"PART HISTORY for \"{term}\":" };
        lines.AddRange(quotes.Where(q => Hit(q.PartDescription, q.PartNumber)).OrderByDescending(q => q.Date)
            .Select(q => $"QUOTE {q.DisplayLine} - unit {q.UnitPrice:C}"));
        lines.AddRange(jobs.Where(j => Hit(j.PartDescription, j.PartNumber)).OrderByDescending(j => j.DueDate)
            .Select(j => $"JOB {j.DisplayLine} - quoted {j.QuotedPrice:C} - margin {JobMargin(j, ops, pos):C}"));
        if (lines.Count == 1) lines.Add("No quotes or jobs match.");
        HistoryList.ItemsSource = lines;
    }
}
