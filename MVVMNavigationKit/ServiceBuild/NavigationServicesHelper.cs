using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmNavigationKit.Abstractions;
using MvvmNavigationKit.Abstractions.ViewModelBase;
using MvvmNavigationKit.NavigationServices;
using MvvmNavigationKit.NavigationStores;
using MvvmNavigationKit.Options;
using MVVMNavigationKit.Base.NavigationView;

namespace MVVMNavigationKit.ServiceBuild
{
    public static class NavigationServicesHelper
    {
        private static Dictionary<Type, Type> _keyViewModel = new();

        public static void RegistrNavigationCase<TUserControl, TViewModel>()
            where TViewModel : ViewModelTemplate
            where TUserControl : UserControl
        {
            _keyViewModel[typeof(TUserControl)] = typeof(TViewModel);
        }

        public static void CreateServiceCollections(IServiceCollection services)
        {
            services.AddLogging(config =>
            {
                config.SetMinimumLevel(LogLevel.Information);
            });

            foreach (var kvp in _keyViewModel)
            {
                services.AddTransient(kvp.Key);
                services.AddTransient(kvp.Value);
            }

            services.AddTransient<NavigationViewModel>();
            services.AddTransient<MainWindow>();

            services.AddSingleton<INavigationStore, NavigationStore>();

            services.Configure<NavigationOptions>(opt => 
            {
                opt.KeyViewModel = _keyViewModel;
                opt.MaxSizeHistory = int.MaxValue;
            });

            services.AddSingleton<INavigationService, NavigationService>();
        }

        public static Window GetNavigationWindow(IServiceProvider serviceProvider) 
        {
            MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = serviceProvider.GetRequiredService<NavigationViewModel>();

            return mainWindow;
        }
    }
}
