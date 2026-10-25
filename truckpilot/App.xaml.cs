using Microsoft.UI.Xaml;

namespace TruckPilot
{
    public partial class App : Application
    {
        public static Window? WindowInstance { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            WindowInstance = new MainWindow();
            WindowInstance.Activate();
        }
    }
}
