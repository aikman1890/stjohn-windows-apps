using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel { get; }

    private bool _initialized;

    public AboutPage()
    {
        ViewModel = new AboutViewModel(App.Ai);
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!_initialized)
        {
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
    }
}
