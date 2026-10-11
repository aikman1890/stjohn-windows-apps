using System.Threading.Tasks;
using BizPilot.Models;
using BizPilot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace BizPilot.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AiService _ai;

    [ObservableProperty]
    private string businessName = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private double taxRate = 0.0825;

    [ObservableProperty]
    private string aiStatus = string.Empty;

    [ObservableProperty]
    private bool isAiAvailable;

    public SettingsViewModel()
    {
        _ai = ((App)Application.Current).Ai;

        var profile = BusinessProfile.Load();
        BusinessName = profile.BusinessName;
        Address = profile.Address;
        Email = profile.Email;
        Phone = profile.Phone;
        TaxRate = profile.TaxRate;

        RefreshAiStatus();
    }

    [RelayCommand]
    private void SaveProfile()
    {
        new BusinessProfile
        {
            BusinessName = BusinessName.Trim(),
            Address = Address.Trim(),
            Email = Email.Trim(),
            Phone = Phone.Trim(),
            TaxRate = TaxRate
        }.Save();
    }

    [RelayCommand]
    private async Task RefreshAiAsync()
    {
        AiStatus = "Checking on-device AI...";
        await _ai.InitializeAsync();
        RefreshAiStatus();
    }

    private void RefreshAiStatus()
    {
        IsAiAvailable = _ai.IsAvailable;
        AiStatus = _ai.StatusMessage;
    }
}
