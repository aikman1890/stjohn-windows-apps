using Microsoft.UI.Xaml;
namespace TestMinimal;
public partial class App : Application
{
    public App() { this.InitializeComponent(); }
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var w = new MainWindow();
        w.Activate();
    }
}
