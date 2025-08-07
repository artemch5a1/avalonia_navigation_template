using System.Threading.Tasks;
using AvaloniaApp.Models;
using AvaloniaApp.ServiceAbstractions;
using AvaloniaApp.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmNavigationKit.Abstractions;

namespace AvaloniaApp.ViewModel
{
    public partial class EditPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        private int? _idUser;

        [ObservableProperty]
        public string _userName = string.Empty;

        [ObservableProperty]
        public string _userSurname = string.Empty;

        [ObservableProperty]
        public string _userEmail = string.Empty;

        [ObservableProperty]
        private string _error = string.Empty;

        public RelayCommand NavToBackCommand { get; }

        public EditPageViewModel(INavigationService navigationService, IUserService userService)
        {
            _navigationService = navigationService;
            _userService = userService;
            NavToBackCommand = new RelayCommand(NavigateBack);
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
            User? user = await _userService.GetUserById(_idUser ?? -1);
            if (user != null)
            {
                UserName = user.Name;
                UserSurname = user.Surname;
                UserEmail = user.Email;
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task UpdateUser()
        {
            if (_idUser == null)
            {
                Error = "ID не был передан";
                return;
            }

            User user = new User()
            {
                Id = _idUser ?? 0,
                Name = UserName,
                Surname = UserSurname,
                Email = UserEmail,
            };

            (bool success, string? error) resValid = UserValidator.ValidateUser(user);

            if (!resValid.success)
            {
                Error = resValid.error ?? string.Empty;
                return;
            }

            bool success = await _userService.UpdateUser(user);

            if (success)
            {
                NavigateBack();
            }
        }

        private void NavigateBack()
        {
            _navigationService.NavigateBack();
        }
    }
}
