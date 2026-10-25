using TaxPilot.Services;
using Microsoft.UI.Xaml;

namespace TaxPilot
{
    public partial class App : Application
    {
        public static Window? WindowInstance { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await SampleSeed.SeedIfFirstRunAsync();
            WindowInstance = new MainWindow();
            WindowInstance.Activate();
        }
    }
}
