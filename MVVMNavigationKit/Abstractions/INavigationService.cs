using Avalonia.Controls;
using MvvmNavigationKit.Abstractions.ViewModelBase;

namespace MvvmNavigationKit.Abstractions
{
    public interface INavigationService
    {
        void Navigate<TUserControl>()
            where TUserControl : UserControl;

        void Navigate<TUserControl, TParams>(TParams @params)
            where TUserControl : UserControl;

        public void NavigateAndForget<TUserControl>()
            where TUserControl : UserControl;

        public void NavigateAndForget<TUserControl, TParams>(TParams @params)
            where TUserControl : UserControl;

        public void NavigateBack();

        public void NavigateOverlay<TUserControl>(
            Action<UserControl?, ViewModelTemplate?>? overlayAction = null,
            Action? onClose = null
        )
            where TUserControl : UserControl;

        public void CloseOverlay();
    }
}
