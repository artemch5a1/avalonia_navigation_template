using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using MvvmNavigationKit.Abstractions;

namespace MvvmNavigationKit.NavigationStores
{
    public class NavigationStore : INavigationStore
    {
        private UserControl? _currentViewModel;

        public UserControl? CurrentView
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
