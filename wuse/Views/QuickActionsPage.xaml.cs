using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Wuse.ViewModels;

namespace Wuse.Views;

public sealed partial class QuickActionsPage : Page
{
    public QuickActionsViewModel ViewModel { get; }

    private bool _initialized;

    public QuickActionsPage()
    {
        ViewModel = new QuickActionsViewModel(App.Quick);
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

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.WebSearchCommand.CanExecute(null))
        {
            ViewModel.WebSearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void FolderBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.OpenFolderCommand.CanExecute(null))
        {
            ViewModel.OpenFolderCommand.Execute(null);
            e.Handled = true;
        }
    }
}
