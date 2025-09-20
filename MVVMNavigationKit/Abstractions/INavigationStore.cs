using System.ComponentModel;
using Avalonia.Controls;
using MvvmNavigationKit.Abstractions.ViewModelBase;

namespace MvvmNavigationKit.Abstractions
{
    public interface INavigationStore : INotifyPropertyChanged
    {
        UserControl? CurrentView { get; set; }
    }
}
