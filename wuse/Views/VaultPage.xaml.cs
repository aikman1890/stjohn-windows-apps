using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wuse.ViewModels;

namespace Wuse.Views;

/// <summary>
/// Secure login vault UI. Passwords live only in the Windows Credential
/// Locker — the list shows names and usernames, never passwords. Copying a
/// password auto-clears the clipboard after 30 seconds. Deletion always
/// asks first.
/// </summary>
public sealed partial class VaultPage : Page
{
    public VaultViewModel ViewModel { get; }

    public VaultPage()
    {
        ViewModel = new VaultViewModel();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // From chat ("save a login for Netflix"): the site arrives as the
        // navigation parameter and is prefilled; secrets are typed here,
        // in masked fields — never in chat.
        if (e.Parameter is string prefill && !string.IsNullOrWhiteSpace(prefill))
            ViewModel.NewSiteName = prefill;
    }

    private void ShowPasswordCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
    {
        NewPasswordBox.PasswordRevealMode =
            ShowPasswordCheckBox.IsChecked == true
                ? PasswordRevealMode.Visible
                : PasswordRevealMode.Hidden;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var password = NewPasswordBox.Password;
        NewPasswordBox.Password = string.Empty; // clear the box immediately
        ViewModel.AddLogin(password);
    }

    private void CopyUsername_Click(object sender, RoutedEventArgs e)
    {
        if (RowOf(sender) is VaultLoginRow row && ViewModel.CopyUsernameCommand.CanExecute(row.Name))
            ViewModel.CopyUsernameCommand.Execute(row.Name);
    }

    private void CopyPassword_Click(object sender, RoutedEventArgs e)
    {
        if (RowOf(sender) is VaultLoginRow row && ViewModel.CopyPasswordCommand.CanExecute(row.Name))
            ViewModel.CopyPasswordCommand.Execute(row.Name);
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (RowOf(sender) is not VaultLoginRow row)
            return;

        var dialog = new ContentDialog
        {
            Title = $"Delete the login for {row.Name}?",
            Content = "This removes it from the encrypted vault. You can't undo this.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Keep it",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot,
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
            ViewModel.ConfirmDelete(row.Name);
    }

    private static object? RowOf(object sender) =>
        (sender as FrameworkElement)?.DataContext;
}
