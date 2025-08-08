using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AvaloniaApp.Models;
using AvaloniaApp.ServiceAbstractions;
using AvaloniaApp.View.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmNavigationKit.Abstractions;
using MvvmNavigationKit.Abstractions.ViewModelBase;

namespace AvaloniaApp.ViewModel
{
    internal partial class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private ViewModelTemplate? _currentOverlayViewModel;

        [ObservableProperty]
        private bool _setOvetlay = false;

        [ObservableProperty]
        private List<User> _users = new();

        private string _errorText = string.Empty;

        private int? _currentIdDelete = null;

        public MainPageViewModel(INavigationService navService, IUserService userService)
        {
            _navService = navService;
            _userService = userService;
            _ = LoadUsers();
        }

        public override void RefreshPage()
        {
            _ = LoadUsers();
        }

        private async Task LoadUsers()
        {
            try
            {
                Users = _userService.GetAllUsers();
            }
            catch (Exception ex)
            {
                _errorText = ex.Message;
                Debug.WriteLine(_errorText);
            }
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void NavToBack() => _navService.Navigate<StartPage>();

        [RelayCommand]
        private void NavToEditUser(int id)
        {
            _navService.Navigate<EditPage, int>(id);
        }

        [RelayCommand]
        private void NavToAddUser()
        {
            //SetOvetlay = true;
            //_navService.NavigateOverlay<CreateUserViewModel>(
            //    overlayAction: vm =>
            //    {
            //        CurrentOverlayViewModel = vm;
            //    },
            //    onClose: () =>
            //    {
            //        _ = LoadUsers();
            //        SetOvetlay = false;
            //    }
            //);
        }

        [RelayCommand]
        private void NavToInfoUser(int id)
        {
            //SetOvetlay = true;
            //_navService.NavigateOverlay<ShowUserViewModel, int>(
            //    id,
            //    overlayAction: vm =>
            //    {
            //        CurrentOverlayViewModel = vm;
            //    },
            //    onClose: () =>
            //    {
            //        _ = LoadUsers();
            //        SetOvetlay = false;
            //    }
            //);
        }

        [RelayCommand]
        private void DeleteUser(int id)
        {
            //_currentIdDelete = id;
            //SetOvetlay = true;
            //_navService.NavigateOverlay<ConfirmViewModel>(
            //    overlayAction: vm =>
            //    {
            //        CurrentOverlayViewModel = vm;
            //        if (vm is ConfirmViewModel viewModel)
            //        {
            //            viewModel.ConfrimAction += DeleteAction;
            //            viewModel.Title = "Удалить пользователя?";
            //        }
            //    },
            //    onClose: () =>
            //    {
            //        RefreshPage();
            //        _currentIdDelete = null;
            //        SetOvetlay = false;
            //    }
            //);
        }

        private void DeleteAction()
        {
            if (_currentIdDelete != null)
            {
                _userService.DeleteUser((int)_currentIdDelete);
            }
        }
    }
}
