using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Puzzles.ViewModels;

namespace Puzzles.Services
{
    public interface INavigationService
    {
        // Добавляем необходимые события
        event Action CurrentViewModelChanged;
        event Action CurrentViewChanged;

        // Остальные члены интерфейса
        ViewModelBase? CurrentViewModel { get; }
        Control? CurrentView { get; }
        void NavigateTo<T>() where T : ViewModelBase;
    }
}
