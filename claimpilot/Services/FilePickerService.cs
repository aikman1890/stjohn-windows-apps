using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ClaimPilot.Services;

/// <summary>
/// Win32-parented file pickers for the unpackaged WinUI 3 app.
/// </summary>
public static class FilePickerService
{
    public static async Task<StorageFile?> PickSaveFileAsync(string suggestedName, string extension, string fileTypeLabel)
    {
        try
        {
            if (App.MainWindow is null)
            {
                return null;
            }

            var picker = new FileSavePicker { SuggestedFileName = suggestedName };
            picker.FileTypeChoices.Add(fileTypeLabel, new List<string> { extension });

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            return await picker.PickSaveFileAsync();
        }
        catch
        {
            return null;
        }
    }

    public static async Task<StorageFile?> PickSingleFileAsync()
    {
        try
        {
            if (App.MainWindow is null)
            {
                return null;
            }

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            return await picker.PickSingleFileAsync();
        }
        catch
        {
            return null;
        }
    }
}
