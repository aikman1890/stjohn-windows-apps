using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BizPilot.Models;
using BizPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BizPilot.ViewModels;

public partial class ExpensesViewModel : ObservableObject
{
    private const double MaxBarWidth = 260;

    private readonly DbService _db;

    [ObservableProperty]
    private ObservableCollection<Expense> expenses = new();

    [ObservableProperty]
    private ObservableCollection<CategoryTotal> categoryTotals = new();

    [ObservableProperty]
    private double monthTotal;

    [ObservableProperty]
    private int filterYear;

    [ObservableProperty]
    private int selectedMonthIndex;

    [ObservableProperty]
    private ObservableCollection<int> years = new();

    [ObservableProperty]
    private ObservableCollection<string> monthNames = new();

    [ObservableProperty]
    private DateTimeOffset newDate = DateTimeOffset.Now;

    [ObservableProperty]
    private string newCategory = "General";

    [ObservableProperty]
    private string newDescription = string.Empty;

    [ObservableProperty]
    private double newAmount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReceiptFileName))]
    private string newReceiptPath = string.Empty;

    public string ReceiptFileName =>
        string.IsNullOrEmpty(NewReceiptPath) ? "No receipt attached" : Path.GetFileName(NewReceiptPath);

    public Window? OwnerWindow { get; set; }

    public ExpensesViewModel()
    {
        _db = ((App)Application.Current).Db;

        var now = DateTime.Now;
        Years = new ObservableCollection<int>(Enumerable.Range(now.Year - 2, 4));
        MonthNames = new ObservableCollection<string>(DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12));
        FilterYear = now.Year;
        SelectedMonthIndex = now.Month - 1;

        Reload();
    }

    partial void OnFilterYearChanged(int value) => Reload();

    partial void OnSelectedMonthIndexChanged(int value) => Reload();

    private void Reload()
    {
        var month = SelectedMonthIndex + 1;
        var filtered = _db.GetExpenses()
            .Where(expense => expense.Date.Month == month && expense.Date.Year == FilterYear)
            .OrderByDescending(expense => expense.Date)
            .ToList();

        Expenses = new ObservableCollection<Expense>(filtered);
        MonthTotal = filtered.Sum(expense => expense.Amount);

        var groups = filtered
            .GroupBy(expense => string.IsNullOrWhiteSpace(expense.Category) ? "Uncategorized" : expense.Category)
            .Select(group => new CategoryTotal
            {
                Category = group.Key,
                Total = group.Sum(expense => expense.Amount)
            })
            .OrderByDescending(group => group.Total)
            .ToList();

        var max = groups.Count == 0 ? 0 : groups.Max(group => group.Total);
        foreach (var group in groups)
        {
            group.BarWidth = max > 0 ? group.Total / max * MaxBarWidth : 0;
        }

        CategoryTotals = new ObservableCollection<CategoryTotal>(groups);
    }

    [RelayCommand]
    private void AddExpense()
    {
        if (NewAmount <= 0 || string.IsNullOrWhiteSpace(NewDescription))
        {
            return;
        }

        _db.InsertExpense(new Expense
        {
            Date = NewDate,
            Category = string.IsNullOrWhiteSpace(NewCategory) ? "General" : NewCategory.Trim(),
            Description = NewDescription.Trim(),
            Amount = NewAmount,
            ReceiptPath = NewReceiptPath
        });

        NewDate = DateTimeOffset.Now;
        NewDescription = string.Empty;
        NewAmount = 0;
        NewReceiptPath = string.Empty;

        Reload();
    }

    [RelayCommand]
    private void DeleteExpense(Expense? expense)
    {
        if (expense is null)
        {
            return;
        }

        _db.DeleteExpense(expense.Id);
        Reload();
    }

    [RelayCommand]
    private async Task AttachReceiptAsync()
    {
        if (OwnerWindow is null)
        {
            return;
        }

        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(OwnerWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        var receiptsDirectory = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Receipts");
        Directory.CreateDirectory(receiptsDirectory);

        var destinationPath = Path.Combine(receiptsDirectory, $"{Guid.NewGuid()}{Path.GetExtension(file.Path)}");
        File.Copy(file.Path, destinationPath, overwrite: true);

        NewReceiptPath = destinationPath;
    }
}
