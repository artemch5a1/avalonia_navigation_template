using System.Threading.Tasks;
using AvaloniaApp.Models;
using AvaloniaApp.ServiceAbstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmNavigationKit.Abstractions;

namespace AvaloniaApp.ViewModel
{
    public partial class ShowUserViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private User _user = new();

        [ObservableProperty]
        private string _error = string.Empty;

        private int _idUser;

        public ShowUserViewModel(INavigationService navigationService, IUserService userService)
        {
            _navigationService = navigationService;
            _userService = userService;
        }

        protected override void InitializeParams<T>(T @params)
        {
            _idUser = GetAs<int>(@params);
            _ = LoadUser();
        }

        public override void RefreshPage()
        {
            _ = LoadUser();
        }

        private async Task LoadUser()
        {
            User? user = await _userService.GetUserById(_idUser);
            if (user != null)
            {
                User = user;
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.CloseOverlay();
        }
    }
}
