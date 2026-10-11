using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BizPilot.Models;

public partial class InvoiceLine : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string invoiceId = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private double qty = 1;

    [ObservableProperty]
    private double unitPrice;

    public double Amount => Qty * UnitPrice;

    partial void OnQtyChanged(double value) => OnPropertyChanged(nameof(Amount));

    partial void OnUnitPriceChanged(double value) => OnPropertyChanged(nameof(Amount));
}
