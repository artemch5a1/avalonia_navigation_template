using System;
using AvaloniaApp.Models;
using AvaloniaApp.ServiceAbstractions;
using AvaloniaApp.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmNavigationKit.Abstractions;

namespace AvaloniaApp.ViewModel
{
    public partial class CreateUserViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private User _user = new User();

        [ObservableProperty]
        private string _error = string.Empty;

        public CreateUserViewModel(INavigationService navigationService, IUserService userService)
        {
            _navigationService = navigationService;
            _userService = userService;
        }

        [RelayCommand]
        private void CreateUser()
        {
            (bool success, string? error) validRes = UserValidator.ValidateUser(User);
            if (validRes.success)
            {
                TryCreateUser();
            }
            else
            {
                Error = validRes.error ?? string.Empty;
            }
        }

        [RelayCommand]
        private void NavToBack()
        {
            _navigationService.CloseOverlay();
        }

        private void TryCreateUser()
        {
            try
            {
                _userService.CreateUser(User);
                NavToBack();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }
}
