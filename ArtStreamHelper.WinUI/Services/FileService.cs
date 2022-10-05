using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArtStreamHelper.Core.Services;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace ArtStreamHelper.WinUI.Services;

public class FileService : IFileService
{
    public async Task<Stream> PickFileAsync(List<string> fileTypes)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".txt");

        // Get the current window's HWND by passing in the Window object
        var hwnd = WindowNative.GetWindowHandle(App.Window);

        // Associate the HWND with the file picker
        InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();

        if (file == null)
        {
            return null;
        }

        return await file.OpenStreamForReadAsync();
    }
}