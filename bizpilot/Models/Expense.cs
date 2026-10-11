using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BizPilot.Models;

public partial class Expense : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private DateTimeOffset date = DateTimeOffset.Now;

    [ObservableProperty]
    private string category = "General";

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private double amount;

    [ObservableProperty]
    private string receiptPath = string.Empty;
}
