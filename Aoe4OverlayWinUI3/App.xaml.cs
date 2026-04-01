using Aoe4OverlayWinUI3.Activation;
using Aoe4OverlayWinUI3.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Contracts.Services;
using Aoe4OverlayWinUI3.Core.Services;
using Aoe4OverlayWinUI3.Models;
using Aoe4OverlayWinUI3.Services;
using Aoe4OverlayWinUI3.ViewModels;
using Aoe4OverlayWinUI3.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Aoe4OverlayWinUI3;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IOverlayService, OverlayService>();

            // Core Services
            services.AddHttpClient();
            //services.AddHttpClient<IAoe4ApiService, Aoe4ApiService>();
            services.AddSingleton<IAoe4ApiService, Aoe4ApiService>();
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<GamesListViewModel>();
            services.AddTransient<GamesListPage>();
            services.AddSingleton<ProfileViewModel>();
            services.AddTransient<ProfilePage>();
            services.AddTransient<ShellPage>();
            services.AddSingleton<ShellViewModel>();
            services.AddTransient<OverlayViewModel>();
            services.AddTransient<OverlayWindow>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Log the exception details
        System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");

        // Optionally show a message to the user
        var dialog = new ContentDialog
        {
            Title = "An error occurred",
            Content = $"Sorry, an unexpected error occurred: {e.Exception.Message}",
            CloseButtonText = "OK",
            XamlRoot = MainWindow.Content.XamlRoot
        };

        _ = dialog.ShowAsync();

        // Mark the event as handled to prevent the app from crashing
        e.Handled = true;
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);

        if (Application.Current != null)
        {
            App.MainWindow.Closed += (s, e) =>
            {
                var overlayService = App.GetService<IOverlayService>();
                overlayService?.ShutDown();
            };
        }
    }
}
