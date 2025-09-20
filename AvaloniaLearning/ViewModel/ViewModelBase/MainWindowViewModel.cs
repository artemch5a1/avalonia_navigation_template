using System.ComponentModel;
using Avalonia.Controls;
using MvvmNavigationKit.Abstractions;

namespace AvaloniaApp.ViewModel
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public UserControl? CurrentViewModel => _navStore.CurrentView;

        private readonly INavigationStore _navStore;

        public MainWindowViewModel(INavigationStore navStore)
        {
            _navStore = navStore;
            _navStore.PropertyChanged += OnViewModelChanged;
        }

        private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_navStore.CurrentView))
            {
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            _navStore.PropertyChanged -= OnViewModelChanged;
            base.Dispose();
        }
    }
}
