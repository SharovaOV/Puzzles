using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Puzzles.CustomControls;
using Puzzles.Enums;
using Puzzles.Models;

namespace Puzzles.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public event Action Deserialize;
        private Canvas _workCanvas;
        private string _workPuzzlePath;
        private string _freePuzzlePath;

        public MainViewModel(SerializeFileNames serializePaths)
        {
            _workPuzzlePath = Path.Combine(serializePaths.MainPath, serializePaths.WorkPiecesFile);
            _freePuzzlePath = Path.Combine(serializePaths.MainPath, serializePaths.FreePiecesFile);
        }
        public void RegisterCanvases(Canvas workCanvas)
        {
            _workCanvas = workCanvas;
        }

        [RelayCommand]
        private void Save()
        {
            if (File.Exists(_workPuzzlePath))
                File.Delete(_workPuzzlePath);

            if (File.Exists(_freePuzzlePath))
                File.Delete(_freePuzzlePath);
            
            List<PieceInfo> workPieces = new();
            List<PieceInfo> freePieces = new();

            foreach (PuzzlePiece puzzlePiece in _workCanvas.Children)
            {
                if (puzzlePiece.PuzzleConfig.ParentId != 0)
                {
                    workPieces.Add(puzzlePiece.PuzzleConfig);
                }
                else
                {
                    freePieces.Add(puzzlePiece.PuzzleConfig);
                }
            }
                string json = JsonConvert.SerializeObject(workPieces);
                File.AppendAllTextAsync(_workPuzzlePath, json);
                json = JsonConvert.SerializeObject(freePieces);
                File.AppendAllTextAsync(_freePuzzlePath, json);
            
        }

        [RelayCommand]
        private void Load()
        {
            _workCanvas.Children.Clear();
            List<PieceInfo> workPieces = new();
            List<PieceInfo> freePieces = new();

            workPieces = JsonConvert.DeserializeObject<List<PieceInfo>>(File.ReadAllText(_workPuzzlePath));
            freePieces = JsonConvert.DeserializeObject<List<PieceInfo>>(File.ReadAllText(_freePuzzlePath));
            
            CreateWorkPices(workPieces);
            CreateFreePices(freePieces);

            Deserialize?.Invoke();
        }

        private void CreateWorkPices(List<PieceInfo> workPieces)
        {
            Dictionary<int, PuzzlePiece> pieces = new();
            foreach (PieceInfo workPiece in workPieces)
            {
                pieces[workPiece.Id] = PuzzlePiece.CreateFromPuzzleConfig(workPiece);
                Canvas.SetLeft(pieces[workPiece.Id], workPiece.X);
                Canvas.SetTop(pieces[workPiece.Id], workPiece.Y);
                _workCanvas.Children.Add(pieces[workPiece.Id]);
            }

            PuzzlePiece masterPuzzle;
            SideType side;
            foreach (var piece in pieces)
            {
                if (!piece.Value.PuzzleConfig.IsFirstPies)
                {
                    masterPuzzle = pieces[piece.Value.PuzzleConfig.ParentId];
                    masterPuzzle.AddSlave(piece.Value);
                }
            }
        }

        private void CreateFreePices(List<PieceInfo> freePieces)
        {
            int space = 10;
            Point currentPoint = new(0, (int)_workCanvas.Bounds.Height);
            int newX;
            PuzzlePiece currentPuzzle;
            foreach (PieceInfo freePiece in freePieces)
            {
                currentPuzzle = PuzzlePiece.CreateFromPuzzleConfig(freePiece);
                if (currentPoint.X == 0)
                    currentPoint.Y -= (int)freePiece.Height - space;
                Canvas.SetLeft(currentPuzzle, currentPoint.X);
                Canvas.SetTop(currentPuzzle, currentPoint.Y);
                _workCanvas.Children.Add(currentPuzzle);
                freePiece.X = currentPoint.X;
                freePiece.Y = currentPoint.Y;

                newX = currentPoint.X + (int)freePiece.Width + space;
                currentPoint.X = newX > _workCanvas.Bounds.Width ? 0 : newX;

            }
        }

    }
}
