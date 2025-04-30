using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Puzzles.Models;

namespace Puzzles.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private PieceConfig _mainPiece = new(Enums.EdgeType.None, Enums.EdgeType.Slot, Enums.EdgeType.None, Enums.EdgeType.None);
    }
}
