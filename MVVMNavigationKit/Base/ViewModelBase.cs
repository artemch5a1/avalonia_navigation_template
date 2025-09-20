using System;
using MvvmNavigationKit.Abstractions.ViewModelBase;

namespace MVVMNavigationKit.Base
{
    /// <summary>
    /// Базовый класс для всех ViewModel в приложении.
    /// Реализует шаблон <see cref="ViewModelTemplate"/>.
    /// </summary>
    /// <remarks>
    /// Предоставляет пустую базовую реализацию для:
    /// <para>
    /// - Инициализации ViewModel с параметрами
    /// </para>
    /// <para>
    /// - Обновления страницы
    /// </para>
    /// И представляет базовую реализацию для <see cref="Dispose"/>
    /// </remarks>
    public class ViewModelBase : ViewModelTemplate
    {
        protected bool IsDisposed { get; set; } = false;

        /// <summary>
        /// Переопределяемый метод для освобождения ресурсов
        /// </summary>
        /// <remarks>
        /// Базовая реализация отменяет финализацию
        /// и устанавливает <see cref="IsDisposed"/> = true
        /// </remarks>
        public override void Dispose()
        {
            if (IsDisposed)
                return;
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        public override void RefreshPage() { }

        protected override void InitializeParams<T>(T @params) { }
    }
}
