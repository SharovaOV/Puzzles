using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using Puzzles.ViewModels;

namespace Puzzles.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _services;
        private readonly IDataTemplate _viewLocator;

        private ViewModelBase? _currentViewModel;
        private Control? _currentView;

        public NavigationService(IServiceProvider services, IDataTemplate viewLocator)
        {
            _services = services;
            _viewLocator = viewLocator;
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                CurrentView = value != null ? _viewLocator.Build(value) : null;
                CurrentViewModelChanged?.Invoke();
            }
        }

        public Avalonia.Controls.Control? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                CurrentViewChanged?.Invoke();
            }
        }

        public event Action? CurrentViewModelChanged;
        public event Action? CurrentViewChanged;

        public void NavigateTo<T>() where T : ViewModelBase
        {
            CurrentViewModel = _services.GetRequiredService<T>();
        }
    }
}
