using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class PreferencesPage : Page
{
    public PreferencesViewModel ViewModel { get; }

    public PreferencesPage()
    {
        ViewModel = new PreferencesViewModel(App.Settings);
        this.InitializeComponent();
    }

    private void RemoveFactButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is string fact)
            ViewModel.RemoveFactCommand.Execute(fact);
    }

    private void NewFactTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.AddFactCommand.CanExecute(null))
        {
            ViewModel.AddFactCommand.Execute(null);
            e.Handled = true;
        }
    }
}
