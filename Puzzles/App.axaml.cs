using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Puzzles.ViewModels;
using Puzzles.Views;
using Avalonia.Controls.Templates;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Puzzles.Services;
using System;

namespace Puzzles;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        InitializeServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeServices()
    {
        var host = Host
        .CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.UseMicrosoftDependencyResolver();
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();

            ConfigureServices(services);
        })
        .UseEnvironment(Environments.Development)
        .Build();

        Services = host.Services;
        Services.UseMicrosoftDependencyResolver();
    }
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDataTemplate, ViewLocator>();
        services.AddSingleton<INavigationService, NavigationService>();

        ViewModelRegistration(services);
    }

    private void ViewModelRegistration(IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}