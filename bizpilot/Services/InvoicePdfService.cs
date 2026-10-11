using System;
using BizPilot.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BizPilot.Services;

public static class InvoicePdfService
{
    public static void GeneratePdf(Invoice invoice, Client client, BusinessProfile profile, string path)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(11).FontColor(Colors.Black));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(profile.BusinessName).Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                        if (!string.IsNullOrWhiteSpace(profile.Address))
                        {
                            column.Item().Text(profile.Address).FontSize(10);
                        }

                        if (!string.IsNullOrWhiteSpace(profile.Email))
                        {
                            column.Item().Text(profile.Email).FontSize(10);
                        }

                        if (!string.IsNullOrWhiteSpace(profile.Phone))
                        {
                            column.Item().Text(profile.Phone).FontSize(10);
                        }
                    });

                    row.ConstantItem(200).AlignRight().Column(column =>
                    {
                        column.Item().Text("INVOICE").Bold().FontSize(30).FontColor(Colors.Grey.Darken3);
                        column.Item().PaddingTop(4).Text($"Number: {invoice.Number}").Bold();
                        column.Item().Text($"Issue date: {invoice.IssueDate:MMMM d, yyyy}").FontSize(10);
                        column.Item().Text($"Due date: {invoice.DueDate:MMMM d, yyyy}").FontSize(10);
                        column.Item().Text($"Status: {invoice.Status}").FontSize(10).Bold();
                    });
                });

                page.Content().PaddingVertical(12).Column(column =>
                {
                    column.Item().Text("Bill To").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingBottom(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    column.Item().Text(client.Name).Bold();
                    if (!string.IsNullOrWhiteSpace(client.Company))
                    {
                        column.Item().Text(client.Company);
                    }

                    if (!string.IsNullOrWhiteSpace(client.Email))
                    {
                        column.Item().Text(client.Email).FontSize(10);
                    }

                    if (!string.IsNullOrWhiteSpace(client.Phone))
                    {
                        column.Item().Text(client.Phone).FontSize(10);
                    }

                    column.Item().PaddingTop(14).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Description").Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCell).AlignRight().Text("Qty").Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCell).AlignRight().Text("Unit Price").Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCell).AlignRight().Text("Amount").Bold().FontColor(Colors.White);
                        });

                        foreach (var line in invoice.Lines)
                        {
                            table.Cell().Element(RowCell).Text(line.Description);
                            table.Cell().Element(RowCell).AlignRight().Text(line.Qty.ToString("0.##"));
                            table.Cell().Element(RowCell).AlignRight().Text(line.UnitPrice.ToString("C"));
                            table.Cell().Element(RowCell).AlignRight().Text(line.Amount.ToString("C"));
                        }
                    });

                    column.Item().PaddingTop(10).AlignRight().Column(totals =>
                    {
                        totals.Item().Text($"Subtotal: {invoice.Subtotal:C}");
                        totals.Item().Text($"Tax ({invoice.TaxRate:P2}): {invoice.Tax:C}");
                        totals.Item().PaddingTop(4).Text($"Total Due: {invoice.Total:C}").Bold().FontSize(15).FontColor(Colors.Blue.Darken2);
                    });

                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        column.Item().PaddingTop(14).Text("Notes").Bold().FontSize(12);
                        column.Item().Text(invoice.Notes).FontSize(10);
                    }
                });

                page.Footer().AlignCenter().Column(footer =>
                {
                    footer.Item().PaddingTop(6).BorderTop(1).BorderColor(Colors.Grey.Lighten2);
                    footer.Item().PaddingTop(4).Text("Thank you for your business.").Italic().FontSize(10);
                });
            });
        }).GeneratePdf(path);
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Blue.Darken2).Padding(6);

    private static IContainer RowCell(IContainer container) =>
        container.BorderBottom(0.75f).BorderColor(Colors.Grey.Lighten2).Padding(6);
}
