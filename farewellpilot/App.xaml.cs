using FarewellPilot.Services;
using Microsoft.UI.Xaml;

namespace FarewellPilot
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
