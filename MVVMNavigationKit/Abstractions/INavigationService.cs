using Avalonia.Controls;
using MvvmNavigationKit.Abstractions.ViewModelBase;

namespace MvvmNavigationKit.Abstractions
{
    public interface INavigationService
    {
        public void RegistrViewModel<TUserControl, TViewModel>()
            where TViewModel : ViewModelTemplate
            where TUserControl : UserControl;

        void Navigate<TViewModel>()
            where TViewModel : UserControl;

        void Navigate<TViewModel, TParams>(TParams @params)
            where TViewModel : UserControl;

        //public void DestroyAndNavigate<TViewModel>()
        //    where TViewModel : UserControl;

        //public void DestroyAndNavigate<TViewModel, TParams>(TParams @params)
        //    where TViewModel : UserControl;

        //public void ResetAndNavigate<TViewModel>()
        //    where TViewModel : UserControl;

        //public void ResetAndNavigate<TViewModel, TParams>(TParams @params)
        //    where TViewModel : UserControl;

        //public void NavigateOverlay<TViewModel>(
        //    Action<ViewModelTemplate?>? overlayAction = null,
        //    Action? onClose = null
        //)
        //    where TViewModel : UserControl;

        //public void NavigateOverlay<TViewModel, TParam>(
        //    TParam @params,
        //    Action<ViewModelTemplate?>? overlayAction = null,
        //    Action? onClose = null
        //)
        //    where TViewModel : ViewModelTemplate;

        //public void CloseOverlay();

        //void NavigateBack();
    }
}
