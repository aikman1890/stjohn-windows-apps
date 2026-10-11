using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BizPilot.Models;
using BizPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Windows.Storage.Pickers;

namespace BizPilot.ViewModels;

public partial class InvoicesViewModel : ObservableObject
{
    private readonly DbService _db;

    [ObservableProperty]
    private ObservableCollection<Invoice> invoices = new();

    [ObservableProperty]
    private ObservableCollection<Client> clients = new();

    [ObservableProperty]
    private Invoice? selectedInvoice;

    public Window? OwnerWindow { get; set; }

    public InvoicesViewModel()
    {
        _db = ((App)Application.Current).Db;
        Reload();
    }

    private void Reload()
    {
        var selectedId = SelectedInvoice?.Id;
        Clients = new ObservableCollection<Client>(_db.GetClients());
        Invoices = new ObservableCollection<Invoice>(_db.GetInvoicesWithTotals());
        SelectedInvoice = Invoices.FirstOrDefault(invoice => invoice.Id == selectedId);
    }

    [RelayCommand]
    private void NewInvoice()
    {
        var profile = BusinessProfile.Load();
        var sequence = _db.GetInvoicesWithTotals().Count + 1;
        SelectedInvoice = new Invoice
        {
            Number = $"INV-{DateTime.Today:yyyyMMdd}-{sequence:D3}",
            IssueDate = DateTimeOffset.Now,
            DueDate = DateTimeOffset.Now.AddDays(30),
            TaxRate = profile.TaxRate,
            Status = "Unpaid",
            ClientId = Clients.FirstOrDefault()?.Id ?? string.Empty
        };
    }

    [RelayCommand]
    private void AddLine()
    {
        var invoice = SelectedInvoice;
        if (invoice is null)
        {
            return;
        }

        invoice.Lines.Add(new InvoiceLine { InvoiceId = invoice.Id });
    }

    [RelayCommand]
    private void RemoveLine(InvoiceLine? line)
    {
        if (line is not null)
        {
            SelectedInvoice?.Lines.Remove(line);
        }
    }

    [RelayCommand]
    private void SaveInvoice()
    {
        var invoice = SelectedInvoice;
        if (invoice is null || string.IsNullOrWhiteSpace(invoice.Number))
        {
            return;
        }

        foreach (var line in invoice.Lines)
        {
            line.InvoiceId = invoice.Id;
        }

        if (_db.InvoiceExists(invoice.Id))
        {
            _db.UpdateInvoice(invoice);
        }
        else
        {
            _db.InsertInvoice(invoice);
        }

        Reload();
        SelectedInvoice = Invoices.FirstOrDefault(i => i.Id == invoice.Id);
    }

    [RelayCommand]
    private void MarkPaid() => SetStatus("Paid");

    [RelayCommand]
    private void MarkUnpaid() => SetStatus("Unpaid");

    private void SetStatus(string status)
    {
        var invoice = SelectedInvoice;
        if (invoice is null)
        {
            return;
        }

        invoice.Status = status;
        if (_db.InvoiceExists(invoice.Id))
        {
            _db.UpdateInvoice(invoice);
        }

        Reload();
        SelectedInvoice = Invoices.FirstOrDefault(i => i.Id == invoice.Id);
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        var invoice = SelectedInvoice;
        if (invoice is null || OwnerWindow is null)
        {
            return;
        }

        var picker = new FileSavePicker
        {
            SuggestedFileName = invoice.Number,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeChoices.Add("PDF", new List<string> { ".pdf" });

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(OwnerWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        var client = Clients.FirstOrDefault(c => c.Id == invoice.ClientId)
            ?? new Client { Name = "(Unknown client)" };

        InvoicePdfService.GeneratePdf(invoice, client, BusinessProfile.Load(), file.Path);

        var notification = new AppNotificationBuilder()
            .AddText("Invoice exported")
            .AddText($"{invoice.Number} was saved as a PDF.")
            .BuildNotification();
        AppNotificationManager.Default.Show(notification);
    }
}
