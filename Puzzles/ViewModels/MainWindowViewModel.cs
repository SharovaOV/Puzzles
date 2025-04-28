using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Puzzles.Services;

namespace Puzzles.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private INavigationService _navigation;

        [ObservableProperty]
        private Control? _currenView;
        public MainWindowViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            _navigation.CurrentViewChanged += OnCurrentViewChanged;
            _navigation.NavigateTo<MainViewModel>();
        }
        private void OnCurrentViewChanged()
        {
            // Обновляем привязанное свойство
            CurrenView = _navigation.CurrentView;
        }
    }
}
