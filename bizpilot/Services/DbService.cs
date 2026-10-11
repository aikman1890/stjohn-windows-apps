using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BizPilot.Models;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace BizPilot.Services;

public sealed class DbService
{
    private const string DateFormat = "yyyy-MM-dd";
    private readonly string _connectionString;

    public DbService()
    {
        var dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "bizpilot.db");
        _connectionString = "Data Source=" + dbPath;
        CreateTables();
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private static string ToDbDate(DateTimeOffset value) =>
        value.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static DateTimeOffset FromDbDate(string value) =>
        DateTimeOffset.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);

    private void CreateTables()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Clients (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Company TEXT NOT NULL DEFAULT '',
                Email TEXT NOT NULL DEFAULT '',
                Phone TEXT NOT NULL DEFAULT '',
                Notes TEXT NOT NULL DEFAULT ''
            );
            CREATE TABLE IF NOT EXISTS Invoices (
                Id TEXT PRIMARY KEY,
                ClientId TEXT NOT NULL,
                Number TEXT NOT NULL,
                IssueDate TEXT NOT NULL,
                DueDate TEXT NOT NULL,
                TaxRate REAL NOT NULL,
                Status TEXT NOT NULL,
                Notes TEXT NOT NULL DEFAULT ''
            );
            CREATE TABLE IF NOT EXISTS InvoiceLines (
                Id TEXT PRIMARY KEY,
                InvoiceId TEXT NOT NULL,
                Description TEXT NOT NULL DEFAULT '',
                Qty REAL NOT NULL,
                UnitPrice REAL NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Expenses (
                Id TEXT PRIMARY KEY,
                Date TEXT NOT NULL,
                Category TEXT NOT NULL,
                Description TEXT NOT NULL DEFAULT '',
                Amount REAL NOT NULL,
                ReceiptPath TEXT NOT NULL DEFAULT ''
            );
            """;
        command.ExecuteNonQuery();
    }

    // ---------------- Clients ----------------

    public List<Client> GetClients()
    {
        var result = new List<Client>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Company, Email, Phone, Notes FROM Clients ORDER BY Name COLLATE NOCASE;";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Client
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Company = reader.GetString(2),
                Email = reader.GetString(3),
                Phone = reader.GetString(4),
                Notes = reader.GetString(5)
            });
        }

        return result;
    }

    public Client? GetClient(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Company, Email, Phone, Notes FROM Clients WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Client
        {
            Id = reader.GetString(0),
            Name = reader.GetString(1),
            Company = reader.GetString(2),
            Email = reader.GetString(3),
            Phone = reader.GetString(4),
            Notes = reader.GetString(5)
        };
    }

    public bool ClientExists(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Clients WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return System.Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    public void InsertClient(Client client)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Clients (Id, Name, Company, Email, Phone, Notes)
            VALUES ($id, $name, $company, $email, $phone, $notes);
            """;
        command.Parameters.AddWithValue("$id", client.Id);
        command.Parameters.AddWithValue("$name", client.Name);
        command.Parameters.AddWithValue("$company", client.Company);
        command.Parameters.AddWithValue("$email", client.Email);
        command.Parameters.AddWithValue("$phone", client.Phone);
        command.Parameters.AddWithValue("$notes", client.Notes);
        command.ExecuteNonQuery();
    }

    public void UpdateClient(Client client)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Clients
            SET Name = $name, Company = $company, Email = $email, Phone = $phone, Notes = $notes
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", client.Id);
        command.Parameters.AddWithValue("$name", client.Name);
        command.Parameters.AddWithValue("$company", client.Company);
        command.Parameters.AddWithValue("$email", client.Email);
        command.Parameters.AddWithValue("$phone", client.Phone);
        command.Parameters.AddWithValue("$notes", client.Notes);
        command.ExecuteNonQuery();
    }

    public void DeleteClient(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Clients WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    // ---------------- Invoices ----------------

    public List<Invoice> GetInvoicesWithTotals()
    {
        var invoices = new List<Invoice>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT i.Id, i.ClientId, i.Number, i.IssueDate, i.DueDate, i.TaxRate, i.Status, i.Notes,
                   COALESCE(c.Name, '') AS ClientName
            FROM Invoices i
            LEFT JOIN Clients c ON c.Id = i.ClientId
            ORDER BY i.IssueDate DESC;
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            invoices.Add(new Invoice
            {
                Id = reader.GetString(0),
                ClientId = reader.GetString(1),
                Number = reader.GetString(2),
                IssueDate = FromDbDate(reader.GetString(3)),
                DueDate = FromDbDate(reader.GetString(4)),
                TaxRate = reader.GetDouble(5),
                Status = reader.GetString(6),
                Notes = reader.GetString(7),
                ClientName = reader.GetString(8)
            });
        }

        reader.Close();

        foreach (var invoice in invoices)
        {
            LoadInvoiceLines(connection, invoice);
        }

        return invoices;
    }

    public Invoice? GetInvoice(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT i.Id, i.ClientId, i.Number, i.IssueDate, i.DueDate, i.TaxRate, i.Status, i.Notes,
                   COALESCE(c.Name, '') AS ClientName
            FROM Invoices i
            LEFT JOIN Clients c ON c.Id = i.ClientId
            WHERE i.Id = $id;
            """;
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var invoice = new Invoice
        {
            Id = reader.GetString(0),
            ClientId = reader.GetString(1),
            Number = reader.GetString(2),
            IssueDate = FromDbDate(reader.GetString(3)),
            DueDate = FromDbDate(reader.GetString(4)),
            TaxRate = reader.GetDouble(5),
            Status = reader.GetString(6),
            Notes = reader.GetString(7),
            ClientName = reader.GetString(8)
        };

        reader.Close();
        LoadInvoiceLines(connection, invoice);
        return invoice;
    }

    public bool InvoiceExists(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM Invoices WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return System.Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    public void InsertInvoice(Invoice invoice)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO Invoices (Id, ClientId, Number, IssueDate, DueDate, TaxRate, Status, Notes)
                VALUES ($id, $clientId, $number, $issueDate, $dueDate, $taxRate, $status, $notes);
                """;
            command.Parameters.AddWithValue("$id", invoice.Id);
            command.Parameters.AddWithValue("$clientId", invoice.ClientId);
            command.Parameters.AddWithValue("$number", invoice.Number);
            command.Parameters.AddWithValue("$issueDate", ToDbDate(invoice.IssueDate));
            command.Parameters.AddWithValue("$dueDate", ToDbDate(invoice.DueDate));
            command.Parameters.AddWithValue("$taxRate", invoice.TaxRate);
            command.Parameters.AddWithValue("$status", invoice.Status);
            command.Parameters.AddWithValue("$notes", invoice.Notes);
            command.ExecuteNonQuery();
        }

        foreach (var line in invoice.Lines)
        {
            InsertInvoiceLine(connection, transaction, invoice.Id, line);
        }

        transaction.Commit();
    }

    public void UpdateInvoice(Invoice invoice)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE Invoices
                SET ClientId = $clientId, Number = $number, IssueDate = $issueDate, DueDate = $dueDate,
                    TaxRate = $taxRate, Status = $status, Notes = $notes
                WHERE Id = $id;
                """;
            command.Parameters.AddWithValue("$id", invoice.Id);
            command.Parameters.AddWithValue("$clientId", invoice.ClientId);
            command.Parameters.AddWithValue("$number", invoice.Number);
            command.Parameters.AddWithValue("$issueDate", ToDbDate(invoice.IssueDate));
            command.Parameters.AddWithValue("$dueDate", ToDbDate(invoice.DueDate));
            command.Parameters.AddWithValue("$taxRate", invoice.TaxRate);
            command.Parameters.AddWithValue("$status", invoice.Status);
            command.Parameters.AddWithValue("$notes", invoice.Notes);
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM InvoiceLines WHERE InvoiceId = $invoiceId;";
            command.Parameters.AddWithValue("$invoiceId", invoice.Id);
            command.ExecuteNonQuery();
        }

        foreach (var line in invoice.Lines)
        {
            InsertInvoiceLine(connection, transaction, invoice.Id, line);
        }

        transaction.Commit();
    }

    public void DeleteInvoice(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM InvoiceLines WHERE InvoiceId = $invoiceId;";
            command.Parameters.AddWithValue("$invoiceId", id);
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM Invoices WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static void InsertInvoiceLine(SqliteConnection connection, SqliteTransaction transaction, string invoiceId, InvoiceLine line)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO InvoiceLines (Id, InvoiceId, Description, Qty, UnitPrice)
            VALUES ($id, $invoiceId, $description, $qty, $unitPrice);
            """;
        command.Parameters.AddWithValue("$id", line.Id);
        command.Parameters.AddWithValue("$invoiceId", invoiceId);
        command.Parameters.AddWithValue("$description", line.Description);
        command.Parameters.AddWithValue("$qty", line.Qty);
        command.Parameters.AddWithValue("$unitPrice", line.UnitPrice);
        command.ExecuteNonQuery();
    }

    private static void LoadInvoiceLines(SqliteConnection connection, Invoice invoice)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Description, Qty, UnitPrice FROM InvoiceLines WHERE InvoiceId = $invoiceId ORDER BY rowid;";
        command.Parameters.AddWithValue("$invoiceId", invoice.Id);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            invoice.Lines.Add(new InvoiceLine
            {
                Id = reader.GetString(0),
                InvoiceId = invoice.Id,
                Description = reader.GetString(1),
                Qty = reader.GetDouble(2),
                UnitPrice = reader.GetDouble(3)
            });
        }
    }

    // ---------------- Expenses ----------------

    public List<Expense> GetExpenses()
    {
        var result = new List<Expense>();
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Date, Category, Description, Amount, ReceiptPath FROM Expenses ORDER BY Date DESC;";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Expense
            {
                Id = reader.GetString(0),
                Date = FromDbDate(reader.GetString(1)),
                Category = reader.GetString(2),
                Description = reader.GetString(3),
                Amount = reader.GetDouble(4),
                ReceiptPath = reader.GetString(5)
            });
        }

        return result;
    }

    public void InsertExpense(Expense expense)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Expenses (Id, Date, Category, Description, Amount, ReceiptPath)
            VALUES ($id, $date, $category, $description, $amount, $receiptPath);
            """;
        command.Parameters.AddWithValue("$id", expense.Id);
        command.Parameters.AddWithValue("$date", ToDbDate(expense.Date));
        command.Parameters.AddWithValue("$category", expense.Category);
        command.Parameters.AddWithValue("$description", expense.Description);
        command.Parameters.AddWithValue("$amount", expense.Amount);
        command.Parameters.AddWithValue("$receiptPath", expense.ReceiptPath);
        command.ExecuteNonQuery();
    }

    public void UpdateExpense(Expense expense)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Expenses
            SET Date = $date, Category = $category, Description = $description,
                Amount = $amount, ReceiptPath = $receiptPath
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", expense.Id);
        command.Parameters.AddWithValue("$date", ToDbDate(expense.Date));
        command.Parameters.AddWithValue("$category", expense.Category);
        command.Parameters.AddWithValue("$description", expense.Description);
        command.Parameters.AddWithValue("$amount", expense.Amount);
        command.Parameters.AddWithValue("$receiptPath", expense.ReceiptPath);
        command.ExecuteNonQuery();
    }

    public void DeleteExpense(string id)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Expenses WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }
}
