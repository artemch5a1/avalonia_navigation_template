using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MvvmNavigationKit.Abstractions;
using MvvmNavigationKit.Abstractions.ViewModelBase;
using MvvmNavigationKit.Options;

namespace MvvmNavigationKit.NavigationServices
{
    /// <summary>
    /// Сервис навигации между ViewModel в приложении.
    /// Реализует паттерн Navigation Service для MVVM.
    /// </summary>
    /// <remarks>
    /// Для работы требует предварительной регистрации ViewModel в DI-контейнере.
    /// <para>
    /// История навигации реализована на основе стека, в котором хранятся ранее открытые ViewModel.
    /// По умолчанию размер истории неограничен, но может быть задан вручную через
    /// <see cref="NavigationOptions.MaxSizeHistory"/>.
    /// </para>
    /// <para>
    /// Если количество сохранённых переходов превысит заданный лимит, самая ранняя запись будет удалена.
    /// Это поведение может использоваться для ограничения потребления памяти в приложениях с большим количеством навигаций.
    /// </para>
    /// <para>
    /// Важно: чтобы избежать неожиданной потери навигационной истории, рекомендуется использовать <see cref="NavigateBack"/>
    /// только если история не пуста. Проверить это можно с помощью <see cref="HistoryIsNotEmpty"/>
    /// </para>
    /// <para>
    /// Чтобы не сохранять ViewModel в историю используйте <see cref="DestroyAndNavigate{TViewModel}()"/>
    /// или <see cref="DestroyAndNavigate{TViewModel, TParams}(TParams)"/> с передачей параметров
    /// </para>
    /// </remarks>
    public class NavigationService : INavigationService
    {
        private readonly INavigationStore _navStore;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private Stack<ViewModelTemplate> _historyNavigation = new();
        private readonly int _maxSizeHistory;

        /// <summary>
        /// Возврашает true если история не пустая
        /// </summary>
        public bool HistoryIsNotEmpty => _historyNavigation.Count > 0;

        private Action<ViewModelTemplate?>? OverlayAction { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр сервиса навигации.
        /// </summary>
        /// <param name="navStore">Хранилище состояния навигации</param>
        /// <param name="serviceProvider">Провайдер сервисов (DI-контейнер)</param>
        /// <param name="options.maxSizeHistory">
        /// Максимальный размер истории навигации.
        /// При превышении этого значения самая ранняя ViewModel будет удалена из истории.
        /// Значение по умолчанию — <c>int.MaxValue</c>, что означает отсутствие ограничений.
        /// </param>
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
            _logger = logger;
        }

        /// <summary>
        /// Выполняет переход на указанную ViewModel без параметров.
        /// </summary>
        /// <typeparam name="TViewModel">Тип ViewModel, на которую выполняется переход</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если не удалось разрешить TViewModel через DI-контейнер
        /// </exception>
        public void Navigate<TViewModel>()
            where TViewModel : ViewModelTemplate
        {
            ViewModelTemplate viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            PushToHistoryAndSetViewModel(viewModel);
            _logger.LogInformation($"Переход к {viewModel.GetType().Name}");
        }

        /// <summary>
        /// Выполняет переход на указанную ViewModel с передачей параметров.
        /// </summary>
        /// <typeparam name="TViewModel">Тип ViewModel, на которую выполняется переход</typeparam>
        /// <typeparam name="TParams">Тип параметров инициализации</typeparam>
        /// <param name="params">Параметры для инициализации ViewModel</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если не удалось разрешить TViewModel через DI-контейнер
        /// </exception>
        /// <remarks>
        /// Ожидается, что целевая ViewModel реализует метод Initialize(TParams parameters).
        /// </remarks>
        public void Navigate<TViewModel, TParams>(TParams @params)
            where TViewModel : ViewModelTemplate
        {
            ViewModelTemplate viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            viewModel.Initialize(@params);
            PushToHistoryAndSetViewModel(viewModel);
            _logger.LogInformation(
                $"Переход к {viewModel.GetType().Name} с передачей параметров: {@params}"
            );
        }

        /// <summary>
        /// Выполняет переход к предыдущей ViewModel в истории навигации.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Если история пуста, метод завершается без действий.
        /// </para>
        /// <para>
        /// Чтобы актуализировать данные при возврате, переопределяейте метод
        /// <see cref="ViewModelTemplate.RefreshPage"/> в целевой ViewModel
        /// </para>
        /// <para>
        /// Текущая ViewModel не добавляется в историю при возврате назад.
        /// </para>
        /// <para>
        /// Если будет превышен лимит истории, самое раннее использование <see cref="NavigateBack"/>
        /// перестанет работать из-за очистки последнего элемента истории навигации
        /// </para>
        /// </remarks>
        public void NavigateBack()
        {
            if (!HistoryIsNotEmpty)
            {
                _logger.LogError($"Попытка перехода назад при пустой истории");
                return;
            }
            _navStore.CurrentViewModel?.Dispose();
            ViewModelTemplate viewModel = _historyNavigation.Pop();
            viewModel.RefreshPage();
            _navStore.CurrentViewModel = viewModel;
            _logger.LogInformation($"Поэтапный возврат к {viewModel.GetType().Name}");
        }

        /// <summary>
        /// Выполняет переход на указанную ViewModel с очищением текущей
        /// </summary>
        /// <typeparam name="TViewModel"> Тип ViewModel, на которую выполняется переход</typeparam>
        /// <remarks>
        /// <para>
        /// Не сохраняет текущую ViewModel в историю
        /// </para>
        /// <para>
        /// Вызывает <see cref="ViewModelTemplate.Dispose"/> у текущей ViewModel
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если не удалось разрешить TViewModel через DI-контейнер
        /// </exception>
        public void DestroyAndNavigate<TViewModel>()
            where TViewModel : ViewModelTemplate
        {
            ViewModelTemplate viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            DisposeAndSetViewModel(viewModel);
            _logger.LogInformation($"Переход к {viewModel.GetType().Name} без сохранения истории");
        }

        /// <summary>
        /// Выполняет переход на указанную ViewModel с передачей параметров с очищением текущей ViewModel
        /// </summary>
        /// <typeparam name="TViewModel"> Тип ViewModel, на которую выполняется переход</typeparam>
        /// <typeparam name="TParams">Тип параметров инициализации</typeparam>
        /// <param name="params">Параметры для инициализации ViewModel</param>
        /// <remarks>
        /// <para>
        /// Не сохраняет текущую ViewModel в историю
        /// </para>
        /// <para>
        /// Вызывает <see cref="ViewModelTemplate.Dispose"/> у текущей ViewModel
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если не удалось разрешить TViewModel через DI-контейнер
        /// </exception>
        public void DestroyAndNavigate<TViewModel, TParams>(TParams @params)
            where TViewModel : ViewModelTemplate
        {
            ViewModelTemplate viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            viewModel.Initialize(@params);
            DisposeAndSetViewModel(viewModel);
            _logger.LogInformation(
                $"Переход к {viewModel.GetType().Name} без сохранения истории c передачей параметров: {@params}"
            );
        }

        /// <summary>
        /// Сбрасывает историю навигации и выполняет переход на указанную ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel">Тип ViewModel, на которую выполняется переход</typeparam>
        public void ResetAndNavigate<TViewModel>()
            where TViewModel : ViewModelTemplate
        {
            DestroyAndNavigate<TViewModel>();
            _historyNavigation.Clear();
            _logger.LogInformation($"История была очищена");
        }

        /// <summary>
        /// Сбрасывает историю навигации и выполняет переход на указанную ViewModel с передачей параметров.
        /// </summary>
        /// <typeparam name="TViewModel"> Тип ViewModel, на которую выполняется переход</typeparam>
        /// <typeparam name="TParams">Тип параметров инициализации</typeparam>
        /// <param name="params">Параметры для инициализации ViewModel</param>
        /// <remarks>
        /// <para>
        /// Не сохраняет текущую ViewModel в историю
        /// </para>
        /// <para>
        /// Вызывает <see cref="ViewModelTemplate.Dispose"/> у текущей ViewModel
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается, если не удалось разрешить TViewModel через DI-контейнер
        /// </exception>
        public void ResetAndNavigate<TViewModel, TParams>(TParams @params)
            where TViewModel : ViewModelTemplate
        {
            DestroyAndNavigate<TViewModel, TParams>(@params);
            _historyNavigation.Clear();
            _logger.LogInformation($"История была очищена");
        }

        /// <summary>
        /// Выполняет навигацию во вложенное оверлейное окно без смены основного контента.
        /// ViewModel будет записана в историю и может быть закрыта через GoBackOneStep.
        /// </summary>
        /// <typeparam name="TViewModel">Тип ViewModel оверлея.</typeparam>
        /// <param name="overlayAction">Действие, которое устанавливает или сбрасывает overlay ViewModel в хосте.</param>
        /// <param name="onClose">Дополнительное действие, выполняемое при закрытии оверлея.</param>
        public void NavigateOverlay<TViewModel>(
            Action<ViewModelTemplate?>? overlayAction = null,
            Action? onClose = null
        )
            where TViewModel : ViewModelTemplate
        {
            ViewModelTemplate? viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            overlayAction?.Invoke(viewModel);

            OverlayAction = vm =>
            {
                overlayAction?.Invoke(null);
                onClose?.Invoke();
            };

            _historyNavigation.Push(viewModel);

            _logger.LogInformation($"Оверлейная навигация на {viewModel.GetType().Name}");
        }

        /// <summary>
        /// Выполняет навигацию во вложенное оверлейное окно без смены основного контента
        /// с параметрами.
        /// ViewModel будет записана в историю и может быть закрыта через GoBackOneStep.
        /// </summary>
        /// <typeparam name="TParam">Тип передаваемых параметров.</typeparam>
        /// <typeparam name="TViewModel">Тип ViewModel оверлея.</typeparam>
        /// <param name="params"> Параметры инициализации</param>
        /// <param name="overlayAction">Действие, которое устанавливает или сбрасывает overlay ViewModel в хосте.</param>
        /// <param name="onClose">Дополнительное действие, выполняемое при закрытии оверлея.</param>
        public void NavigateOverlay<TViewModel, TParam>(
            TParam @params,
            Action<ViewModelTemplate?>? overlayAction = null,
            Action? onClose = null
        )
            where TViewModel : ViewModelTemplate
        {
            ViewModelTemplate viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            viewModel.Initialize(@params);

            overlayAction?.Invoke(viewModel);

            OverlayAction = vm =>
            {
                overlayAction?.Invoke(null);
                onClose?.Invoke();
            };

            _historyNavigation.Push(viewModel);

            _logger.LogInformation(
                $"Оверлейная навигация на {viewModel.GetType().Name} с передачей параметров: {@params}"
            );
        }

        /// <summary>
        /// Выполняет действие по закрытию оверлейного окна
        /// и очищает ViewModel
        /// </summary>
        public void CloseOverlay()
        {
            if (!HistoryIsNotEmpty)
            {
                _logger.LogError("Попытка закрытия оверлейного окна при пустой истории");
                return;
            }

            ViewModelTemplate viewModel = _historyNavigation.Pop();

            viewModel.Dispose();

            OverlayAction?.Invoke(null);

            OverlayAction = null;

            _logger.LogInformation("Закрытие оверлейного окна");
        }

        /// <summary>
        /// Добавляет текущую ViewModel в историю и устанавливает новую ViewModel.
        /// </summary>
        /// <param name="viewModel">Новая ViewModel для перехода</param>
        /// <remarks>
        /// <para>
        /// Если текущая ViewModel в <see cref="_navStore"/> равна null, она не добавляется в историю.
        /// </para>
        /// <para>
        /// При достижении максимального размера истории (<see cref="_maxSizeHistory"/>),
        /// самая старая запись удаляется.
        /// </para>
        /// </remarks>
        private void PushToHistoryAndSetViewModel(ViewModelTemplate viewModel)
        {
            if (_navStore.CurrentViewModel != null)
            {
                if (_maxSizeHistory <= _historyNavigation.Count)
                {
                    string type = RemoveLastVM();
                    _logger.LogWarning(
                        $"Превышен лимит истории, удалена самая старая запись ({type})"
                    );
                }
                _historyNavigation.Push(_navStore.CurrentViewModel);
            }
            _navStore.CurrentViewModel = viewModel;
        }

        /// <summary>
        /// Функция для очищения текущей ViewModel и переходу на следующую
        /// </summary>
        /// <remarks>
        /// Вызывается базовый переопределяемый метод у текущей ViewModel
        /// <see cref="ViewModelTemplate.Dispose"/>
        /// </remarks>
        private void DisposeAndSetViewModel(ViewModelTemplate viewModel)
        {
            if (_navStore.CurrentViewModel != null)
            {
                _navStore.CurrentViewModel.Dispose();
            }
            _navStore.CurrentViewModel = viewModel;
        }

        /// <summary>
        /// Функция для удаления первой записи истории
        /// </summary>
        private string RemoveLastVM()
        {
            _historyNavigation = new(_historyNavigation);
            ViewModelTemplate vm = _historyNavigation.Pop();
            vm.Dispose();
            _historyNavigation = new(_historyNavigation);
            return vm.GetType().Name;
        }
    }
}
