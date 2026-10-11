using System.Collections.ObjectModel;
using System.Linq;
using BizPilot.Models;
using BizPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace BizPilot.ViewModels;

public partial class ClientsViewModel : ObservableObject
{
    private readonly DbService _db;

    [ObservableProperty]
    private ObservableCollection<Client> clients = new();

    [ObservableProperty]
    private Client? selectedClient;

    public ClientsViewModel()
    {
        _db = ((App)Application.Current).Db;
        Reload();
    }

    private void Reload()
    {
        var selectedId = SelectedClient?.Id;
        Clients = new ObservableCollection<Client>(_db.GetClients());
        SelectedClient = Clients.FirstOrDefault(client => client.Id == selectedId);
    }

    [RelayCommand]
    private void NewClient()
    {
        SelectedClient = new Client();
    }

    [RelayCommand]
    private void SaveClient()
    {
        var client = SelectedClient;
        if (client is null || string.IsNullOrWhiteSpace(client.Name))
        {
            return;
        }

        if (_db.ClientExists(client.Id))
        {
            _db.UpdateClient(client);
        }
        else
        {
            _db.InsertClient(client);
        }

        Reload();
        SelectedClient = Clients.FirstOrDefault(c => c.Id == client.Id);
    }

    [RelayCommand]
    private void DeleteClient()
    {
        var client = SelectedClient;
        if (client is null)
        {
            return;
        }

        if (_db.ClientExists(client.Id))
        {
            _db.DeleteClient(client.Id);
        }

        SelectedClient = null;
        Reload();
    }
}
