using System;
using System.Collections.ObjectModel;
using System.Linq;
using BizPilot.Models;
using BizPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace BizPilot.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DbService _db;

    [ObservableProperty]
    private double outstandingTotal;

    [ObservableProperty]
    private double paidThisMonth;

    [ObservableProperty]
    private double expenseThisMonth;

    [ObservableProperty]
    private int clientCount;

    [ObservableProperty]
    private ObservableCollection<Invoice> recentInvoices = new();

    public DashboardViewModel()
    {
        _db = ((App)Application.Current).Db;
        Load();
    }

    [RelayCommand]
    private void Refresh() => Load();

    private void Load()
    {
        var invoices = _db.GetInvoicesWithTotals();
        var now = DateTime.Now;

        OutstandingTotal = invoices
            .Where(invoice => !string.Equals(invoice.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            .Sum(invoice => invoice.Total);

        PaidThisMonth = invoices
            .Where(invoice => string.Equals(invoice.Status, "Paid", StringComparison.OrdinalIgnoreCase)
                && invoice.IssueDate.Month == now.Month
                && invoice.IssueDate.Year == now.Year)
            .Sum(invoice => invoice.Total);

        ExpenseThisMonth = _db.GetExpenses()
            .Where(expense => expense.Date.Month == now.Month && expense.Date.Year == now.Year)
            .Sum(expense => expense.Amount);

        ClientCount = _db.GetClients().Count;

        RecentInvoices = new ObservableCollection<Invoice>(invoices.Take(5));
    }
}
