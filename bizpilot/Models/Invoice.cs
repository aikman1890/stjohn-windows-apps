using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BizPilot.Models;

public partial class Invoice : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string clientId = string.Empty;

    [ObservableProperty]
    private string number = string.Empty;

    [ObservableProperty]
    private DateTimeOffset issueDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset dueDate = DateTimeOffset.Now.AddDays(30);

    [ObservableProperty]
    private double taxRate = 0.0825;

    [ObservableProperty]
    private string status = "Unpaid";

    [ObservableProperty]
    private string notes = string.Empty;

    public ObservableCollection<InvoiceLine> Lines { get; } = new();

    public double Subtotal => Lines.Sum(line => line.Amount);

    public double Tax => Subtotal * TaxRate;

    public double Total => Subtotal + Tax;

    public string ClientName { get; set; } = string.Empty;

    public Invoice()
    {
        Lines.CollectionChanged += OnLinesChanged;
    }

    partial void OnTaxRateChanged(double value) => RaiseTotalsChanged();

    private void OnLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (InvoiceLine line in e.NewItems)
            {
                line.PropertyChanged += OnLinePropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (InvoiceLine line in e.OldItems)
            {
                line.PropertyChanged -= OnLinePropertyChanged;
            }
        }

        RaiseTotalsChanged();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(InvoiceLine.Amount)
            or nameof(InvoiceLine.Qty)
            or nameof(InvoiceLine.UnitPrice))
        {
            RaiseTotalsChanged();
        }
    }

    private void RaiseTotalsChanged()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Total));
    }
}
