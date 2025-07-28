using Microsoft.Extensions.DependencyInjection;
using MvvmNavigationKit.Abstractions;
using MvvmNavigationKit.Abstractions.ViewModelBase;
using MVVMNavigationKit.Exceptions;
using MvvmNavigationKit.NavigationServices;
using MvvmNavigationKit.NavigationStores;
using MvvmNavigationKit.Options;
using MvvmNavigationKit.Tests.TestHelper;

namespace AvaloniaApp.Tests
{
    public class NavigationServiceTests
    {
        private ServiceProvider _serviceProvider;

        public NavigationServiceTests()
        {
            ServiceCollection serviceDescriptors = new ServiceCollection();

            serviceDescriptors.AddSingleton<ViewModelTemplate, ViewModelBase>();

            serviceDescriptors.AddScoped<FakeViewModel>();

            serviceDescriptors.AddScoped<FakeViewModel2>();

            serviceDescriptors.AddScoped<ViewModelBase>();

            serviceDescriptors.Configure<NavigationOptions>(op => op.MaxSizeHistory = 20);

            serviceDescriptors.AddScoped<NavigationService>();

            serviceDescriptors.AddScoped<INavigationStore, NavigationStore>();

            serviceDescriptors.AddLogging(config => { });

            _serviceProvider = serviceDescriptors.BuildServiceProvider();
        }

        [Fact]
        public void Navigate_NavigateToVM_ShouldChangeCurrentVM()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            //Act
            navigationService.Navigate<ViewModelBase>();

            //Assert
            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(ViewModelBase));
        }

        [Fact]
        public void Navigate_NavigateWithParams_ShouldCallInitializeWithRightParams()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            (int, string) @param = (12, "string");

            //Act
            navigationService.Navigate<FakeViewModel, (int, string)>(@param);

            //Assert
            FakeViewModel fakeView = _serviceProvider.GetRequiredService<FakeViewModel>();
            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            Assert.True(fakeView.itemParam == @param);
            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel));
        }

        [Fact]
        public void Navigate_NavigateWithWrongTypeParams_ShouldThrowArgumentException()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            int @param = 12;

            //Act & Assert
            TypeMismatchException? result = Assert.Throws<TypeMismatchException>(() =>
                navigationService.Navigate<FakeViewModel, int>(@param)
            );

            Assert.Equal(
                new TypeMismatchException(typeof((int, string)), typeof(int)).Message,
                result.Message
            );
        }

        [Fact]
        public void Navigate_NavigateWithReferenceTypeParam_ShouldInitialize()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            Animal animal = new Animal() { Name = "Foo", Age = 10 };

            //Act
            navigationService.Navigate<FakeViewModel2, Animal>(animal);

            //Assert
            FakeViewModel2 fakeView = _serviceProvider.GetRequiredService<FakeViewModel2>();

            Assert.True(animal.Equals(fakeView.animal));
        }

        [Fact]
        public void NavigateBack_NavigateWithNotEmptyHistory_NavigateToRightVM()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            //Act
            navigationService.Navigate<FakeViewModel>();

            navigationService.Navigate<FakeViewModel2>();

            navigationService.Navigate<ViewModelBase>();

            //Act & Assert
            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(ViewModelBase));

            navigationService.NavigateBack();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel2));

            navigationService.NavigateBack();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel));
        }

        [Fact]
        public void NavigateBack_NavigateWithNotEmptyHistory_NavigateAndRefreshAndCleanHistory()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            Animal animal = new Animal() { Name = "A", Age = 1 };

            //Act
            navigationService.Navigate<FakeViewModel2, Animal>(animal);
            navigationService.Navigate<FakeViewModel>();
            navigationService.NavigateBack();

            //Assert
            FakeViewModel2 fakeViewModel2 = _serviceProvider.GetRequiredService<FakeViewModel2>();

            Assert.Equal("тест", fakeViewModel2.animal.Breed);
            Assert.True(!navigationService.HistoryIsNotEmpty);
        }

        [Fact]
        public void NavigateBack_NavigateStackOverflow_NotNavigateToFirstVM()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            //Act
            navigationService.Navigate<FakeViewModel>();

            navigationService.Navigate<FakeViewModel2>();

            for (int i = 0; i < 20; i++)
            {
                navigationService.Navigate<ViewModelBase>();
            }

            //Act & Assert
            for (int i = 0; i < 19; i++)
            {
                navigationService.NavigateBack();
                Assert.True(navStore.CurrentViewModel!.GetType() == typeof(ViewModelBase));
            }

            navigationService.NavigateBack();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel2));

            navigationService.NavigateBack();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel2));
        }

        [Fact]
        public void DestroyAndNavigate_NavigateWithoutParams_ShouldChangeViewModelAndNotSaveInHistory()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            //Act
            navigationService.Navigate<ViewModelBase>();
            navigationService.DestroyAndNavigate<FakeViewModel>();

            //Assert
            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel));
            Assert.True(!navigationService.HistoryIsNotEmpty);
        }

        [Fact]
        public void DestroyAndNavigate_NavigateWithParams_ShouldInitializeAndNotSaveInHistoryAndChangeVM()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            (int, string) @param = (12, "string");

            //Act
            navigationService.Navigate<ViewModelBase>();
            navigationService.DestroyAndNavigate<FakeViewModel, (int, string)>(@param);

            //Assert
            FakeViewModel fakeView = _serviceProvider.GetRequiredService<FakeViewModel>();
            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            Assert.True(fakeView.itemParam == @param);
            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel));
            Assert.True(!navigationService.HistoryIsNotEmpty);
        }

        [Fact]
        public void NavigateOverlay_NavigateOverlayWithoutParams_RightAction()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            INavigationStore navigationStore =
                _serviceProvider.GetRequiredService<INavigationStore>();

            ViewModelTemplate? viewModel = null;

            FakeViewModel2 fakeView = _serviceProvider.GetRequiredService<FakeViewModel2>();

            //Act
            navigationService.Navigate<FakeViewModel2>();

            navigationService.NavigateOverlay<FakeViewModel>(
                overlayAction: vm =>
                {
                    viewModel = vm;
                },
                onClose: () =>
                {
                    fakeView.RefreshPage();
                }
            );

            //Assert
            Assert.Equal(typeof(FakeViewModel), viewModel?.GetType());

            navigationService.CloseOverlay();

            Assert.Equal("тест", fakeView.animal.Breed);
        }

        [Fact]
        public void NavigateOverlay_NavigateOverlayWithParams_RightActionAndInitialize()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            INavigationStore navigationStore =
                _serviceProvider.GetRequiredService<INavigationStore>();

            ViewModelTemplate? viewModel = null;

            FakeViewModel2 fakeView = _serviceProvider.GetRequiredService<FakeViewModel2>();

            FakeViewModel fakeView1 = _serviceProvider.GetRequiredService<FakeViewModel>();

            (int, string) @params = (15, "тест");

            //Act
            navigationService.Navigate<FakeViewModel2>();

            navigationService.NavigateOverlay<FakeViewModel, (int, string)>(
                @params,
                overlayAction: vm =>
                {
                    viewModel = vm;
                },
                onClose: () =>
                {
                    fakeView.RefreshPage();
                }
            );

            //Assert
            Assert.Equal(typeof(FakeViewModel), viewModel?.GetType());

            Assert.Equal(@params, fakeView1.itemParam);

            navigationService.CloseOverlay();

            Assert.Equal("тест", fakeView.animal.Breed);
        }

        [Fact]
        public void ResetAndNavigate_ResetAndNavigateToVM_ShouldClearHistory()
        {
            //Arrange
            NavigationService navigationService =
                _serviceProvider.GetRequiredService<NavigationService>();

            //Act
            navigationService.Navigate<FakeViewModel2>();
            navigationService.Navigate<ViewModelBase>();
            navigationService.Navigate<FakeViewModel>();
            navigationService.ResetAndNavigate<FakeViewModel2>();

            //Assert
            INavigationStore navStore = _serviceProvider.GetRequiredService<INavigationStore>();

            Assert.True(navStore.CurrentViewModel!.GetType() == typeof(FakeViewModel2));
            Assert.True(!navigationService.HistoryIsNotEmpty);
        }
    }
}
