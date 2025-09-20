using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MvvmNavigationKit.Abstractions;
using MvvmNavigationKit.Abstractions.ViewModelBase;
using MvvmNavigationKit.Options;

namespace MvvmNavigationKit.NavigationServices
{
    public class NavigationService : INavigationService
    {
        private readonly INavigationStore _navStore;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private readonly int _maxSizeHistory;

        private Dictionary<Type, Type> _keyViewModel = new();
        
        private Stack<UserControl> viewHistory = new();

        public bool HistoryIsNotEmpty => viewHistory.Count > 0;

        private Action<ViewModelTemplate?>? OverlayAction { get; set; }

        public NavigationService(
            INavigationStore navStore,
            IServiceProvider serviceProvider,
            ILogger<NavigationService> logger,
            IOptions<NavigationOptions> options
        )
        {
            _navStore = navStore;
            _serviceProvider = serviceProvider;
            _maxSizeHistory = options.Value.MaxSizeHistory;
            _keyViewModel = options.Value.KeyViewModel;
            _logger = logger;
        }

        public void Navigate<TUserControl>()
            where TUserControl : UserControl
        {
            TUserControl userControl = _serviceProvider.GetRequiredService<TUserControl>();

            userControl.DataContext = GetViewModel(userControl);

            PushToHistoryAndNavigate(userControl);
        }

        public void Navigate<TUserControl, TParams>(TParams @params)
            where TUserControl : UserControl
        {
            TUserControl userControl = _serviceProvider.GetRequiredService<TUserControl>();

            ViewModelTemplate viewModelTemplate = GetViewModel(userControl);

            viewModelTemplate.Initialize(@params);
            userControl.DataContext = viewModelTemplate;

            PushToHistoryAndNavigate(userControl);
        }

        public void NavigateAndForget<TUserControl>()
            where TUserControl : UserControl
        {
            TUserControl userControl = _serviceProvider.GetRequiredService<TUserControl>();

            ViewModelTemplate viewModelTemplate = GetViewModel(userControl);

            userControl.DataContext = viewModelTemplate;

            OnlyNavigate(userControl);
        }

        public void NavigateAndForget<TUserControl, TParams>(TParams @params)
            where TUserControl : UserControl
        {
            TUserControl userControl = _serviceProvider.GetRequiredService<TUserControl>();

            ViewModelTemplate viewModelTemplate = GetViewModel(userControl);

            viewModelTemplate.Initialize(@params);
            userControl.DataContext = viewModelTemplate;

            OnlyNavigate(userControl);
        }

        public void NavigateBack() 
        {
            if (!HistoryIsNotEmpty)
            {
                _logger.LogError($"Попытка перехода назад при пустой истории");
                return;
            }

            if (_navStore.CurrentView?.DataContext is ViewModelTemplate vmOld)
                vmOld.Dispose();

            UserControl userControl = viewHistory.Pop();

            _navStore.CurrentView = userControl;

            if (_navStore.CurrentView.DataContext is ViewModelTemplate vm)
                vm.RefreshPage();
        }

        public void NavigateOverlay<TUserControl>(
            Action<UserControl?, ViewModelTemplate?>? overlayAction = null,
            Action? onClose = null
        )
            where TUserControl : UserControl
        {
            TUserControl userControl = _serviceProvider.GetRequiredService<TUserControl>();

            userControl.DataContext = GetViewModel(userControl);

            if(userControl.DataContext is ViewModelTemplate vm)
                overlayAction?.Invoke(userControl, vm);
            else 
                overlayAction?.Invoke(userControl, null);

            OverlayAction = vm =>
                {
                    overlayAction?.Invoke(null, null);
                    onClose?.Invoke();
                    vm?.Dispose();
                };

            viewHistory.Push(userControl);

            _logger.LogInformation($"Оверлейная навигация на {userControl.GetType().Name}");
        }

        public void CloseOverlay()
        {
            if (!HistoryIsNotEmpty)
            {
                _logger.LogError("Попытка закрытия оверлейного окна при пустой истории");
                return;
            }

            UserControl userControl = viewHistory.Pop();

            OverlayAction?.Invoke(null);

            OverlayAction = null;

            _logger.LogInformation("Закрытие оверлейного окна");
        }

        private void PushToHistoryAndNavigate(UserControl userControl)
        {
            if (_navStore.CurrentView is not null)
                viewHistory.Push(_navStore.CurrentView);

            _navStore.CurrentView = userControl;
            _logger.LogInformation($"Переход к {userControl.GetType().Name}" +
                $"с сохранением истории");
        }

        private void OnlyNavigate(UserControl userControl)
        {
            _navStore.CurrentView = userControl;
            _logger.LogInformation($"Переход к {userControl.GetType().Name}" +
                $"без сохранения в историю");
        }

        private ViewModelTemplate GetViewModel<TUserControl>(TUserControl userControl)
            where TUserControl : UserControl
        {
            Type? viewModelType = _keyViewModel!.GetValueOrDefault(typeof(TUserControl), null);

            if (viewModelType is null)
                throw new ArrayTypeMismatchException(
                    $"Для {typeof(TUserControl).Name} не зарегистрирована ViewModel"
                );

            ViewModelTemplate viewModelTemplate =
                (ViewModelTemplate)_serviceProvider.GetRequiredService(viewModelType);

            return viewModelTemplate;
        }
    }
}
