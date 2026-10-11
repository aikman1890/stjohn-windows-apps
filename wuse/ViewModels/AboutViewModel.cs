using CommunityToolkit.Mvvm.ComponentModel;
using Wuse.Services;

namespace Wuse.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly AiService _ai;

    [ObservableProperty]
    private string aiStatus = "Checking on-device AI…";

    public AboutViewModel(AiService ai)
    {
        _ai = ai;
    }

    public string AppVersion => GetAppVersion();

    public async Task InitializeAsync()
    {
        await _ai.InitializeAsync();
        AiStatus = _ai.StatusMessage;
    }

    private static string GetAppVersion()
    {
        try
        {
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
        catch
        {
            return "1.0.0";
        }
    }
}
