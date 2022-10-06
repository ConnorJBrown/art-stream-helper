using ArtStreamHelper.Core.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using Windows.Foundation;

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
        MainPage.DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private MainViewModel ViewModel => (MainViewModel)MainPage.DataContext;
    
    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.PromptText):
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
            foreach (var prompt in ViewModel.PromptList)
            {
                OutlinedTextBlock text = new OutlinedTextBlock
                {
                    Style = style,
                    Text = $"Prompt: {prompt}"
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