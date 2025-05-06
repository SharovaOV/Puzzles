using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Puzzles.CustomControls;
using Puzzles.Models;

namespace Puzzles
{
    static class PuzzlePieceExtensions
    {
        public static void SetPieceForm(this PuzzlePiece puzzlePiece, string formString)
        {
            puzzlePiece.SetValue(PuzzlePiece.PieceFormProperty, PieceConfig.ParsePieceForm(formString));
        }
    }
}
