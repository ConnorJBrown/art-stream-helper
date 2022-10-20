using ArtStreamHelper.Core.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.IO;
using Windows.Foundation;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArtStreamHelper.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Art Stream Helper";
        var window = GetAppWindowForCurrentWindow();
        window.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/logo.ico"));
        MainPage.DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        UpdatePromptMaxWidth();
        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private MainViewModel ViewModel => (MainViewModel)MainPage.DataContext;
    
    private AppWindow GetAppWindowForCurrentWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }
    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.OriginalPromptList):
            case nameof(MainViewModel.PromptPrefix):
                UpdatePromptMaxWidth();
                TextBlockPrompt.InvalidateMeasure();
                break;
            case nameof(MainViewModel.TimeText):
                TextBlockTime.InvalidateMeasure();
                break;
        }
    }

    private void UpdatePromptMaxWidth()
    {
        double maxWidth = GetMaxTextWidth();
        TextBlockPrompt.Width = maxWidth;
    }

    private double GetMaxTextWidth()
    {
        double maxWidth = 0;

        if (MainPage.Resources.TryGetValue("MyOutlinedTextBlockStyle", out object value) && value is Style style)
        {
            foreach (var prompt in ViewModel.OriginalPromptList)
            {
                OutlinedTextBlock text = new OutlinedTextBlock
                {
                    Style = style,
                    Text = $"{ViewModel.PromptPrefix}: {prompt}"
                };

                text.Measure(new Size(4000, 4000));
                if (maxWidth < text.DesiredSize.Width)
                {
                    maxWidth = text.DesiredSize.Width;
                }
            }
        }

        return maxWidth;
    }
}