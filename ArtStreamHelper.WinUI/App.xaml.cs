using ArtStreamHelper.Core.Services;
using ArtStreamHelper.Core.ViewModels;
using ArtStreamHelper.WinUI.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArtStreamHelper.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var thing = DispatcherQueue.GetForCurrentThread();


            var serviceCollection = new ServiceCollection()
                .AddSingleton<IFileService, FileService>()
                .AddSingleton<IPlatformServices, PlatformServices>()
                .AddSingleton<MainViewModel>();

            Ioc.Default.ConfigureServices(serviceCollection.BuildServiceProvider());

            m_window = new MainWindow();
            m_window.Activate();
        }

        private static Window m_window;

        public static Window Window => m_window;

    }
}
