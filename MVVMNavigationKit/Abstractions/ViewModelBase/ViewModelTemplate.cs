using CommunityToolkit.Mvvm.ComponentModel;
using MVVMNavigationKit.Exceptions;

namespace MvvmNavigationKit.Abstractions.ViewModelBase
{
    /// <summary>
    /// Базовый шаблон ViewModel для использования в MVVM.
    /// Поддерживает инициализацию параметрами, обновление страницы и освобождение ресурсов.
    /// </summary>
    public abstract class ViewModelTemplate : ObservableObject, IDisposable
    {
        /// <summary>
        /// Инициализирует ViewModel с заданными параметрами.
        /// </summary>
        /// <typeparam name="T">Тип параметров инициализации.</typeparam>
        /// <param name="params">Объект параметров.</param>
        /// <remarks>
        /// Вызывает защищённый виртуальный метод <see cref="InitializeParams{T}(T)"/>,
        /// который может быть переопределён в производных классах.
        /// </remarks>
        public void Initialize<T>(T @params)
        {
            InitializeParams(@params);
        }

        /// <summary>
        /// Обновляет данные ViewModel при возврате назад в навигации.
        /// </summary>
        /// <remarks>
        /// Вызывается навигационным сервисом после <see cref="INavigationService.NavigateBack"/>.
        /// Используйте для обновления состояния при повторной активации страницы.
        /// </remarks>
        public abstract void RefreshPage();

        /// <summary>
        /// Обрабатывает параметры инициализации ViewModel.
        /// </summary>
        /// <typeparam name="T">Тип параметров.</typeparam>
        /// <param name="params">Параметры для инициализации.</param>
        /// <remarks>
        /// Переопределите этот метод в производных классах для применения параметров.
        /// </remarks>
        protected abstract void InitializeParams<T>(T @params);

        /// <summary>
        /// Безопасно приводит переданный объект к заданному типу.
        /// </summary>
        /// <typeparam name="T">Ожидаемый тип параметра.</typeparam>
        /// <param name="params">Объект параметров для преобразования.</param>
        /// <returns>Параметры, приведённые к типу <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если:
        /// <list type="bullet">
        /// <item>Параметры не могут быть приведены к типу <typeparamref name="T"/>.</item>
        /// <item>Передан null, но <typeparamref name="T"/> не допускает null.</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// Удобный помощник для безопасного приведения параметров в производных ViewModel.
        /// </remarks>
        protected static T GetAs<T>(object? @params)
        {
            if (@params is null && default(T) is null)
                return default!;

            if (@params is T t)
                return t;

            throw new TypeMismatchException(typeof(T), @params?.GetType());
        }

        /// <summary>
        /// Освобождает ресурсы, связанные с ViewModel.
        /// </summary>
        /// <remarks>
        /// Переопределите в производных классах, чтобы освободить ресурсы или отменить подписки.
        /// </remarks>
        public abstract void Dispose();
    }
}
