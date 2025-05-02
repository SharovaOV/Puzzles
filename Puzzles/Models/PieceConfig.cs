using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Puzzles.Enums;
using Puzzles.Converters;

namespace Puzzles.Models
{
    [TypeConverter(typeof(PieceFormConverter))]
    public class PieceConfig
    {
        public EdgeType Left { get; private set; } = EdgeType.None;
        public EdgeType Top { get; private set; } = EdgeType.None;
        public EdgeType Right { get; private set; } = EdgeType.None;
        public EdgeType Bottom { get; private set; } = EdgeType.None;

        public PieceConfig(EdgeType uniformEdge)
        {
            SetValues(uniformEdge);
        }

        public PieceConfig(EdgeType horizontalEdge, EdgeType verticalEdge)
        {
            SetValues( horizontalEdge, verticalEdge);
        }

        public PieceConfig(EdgeType left, EdgeType top, EdgeType right, EdgeType bottom)
        {
            SetValues(left, top, right, bottom);
        }                       
   
        public PieceConfig(string uniformEdgeString)
        {
            SetValues(uniformEdgeString);
        }

        public PieceConfig(string horizontalEdgeString, string verticalEdgeString)
        {
            SetValues( horizontalEdgeString,  verticalEdgeString);
        }    

        public PieceConfig(string leftString, string topString, string rightString, string bottomString)
        {
            SetValues(leftString, topString, rightString, bottomString);
        }

        private void SetValues(EdgeType uniformEdge)
        {
            Left = Top = Right = Bottom = uniformEdge;
        }

        private void SetValues(EdgeType horizontalEdge, EdgeType verticalEdge)
        {
            Left = Right = horizontalEdge;
            Top = Bottom = verticalEdge;
        }

        private void SetValues(EdgeType left, EdgeType top, EdgeType right, EdgeType bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }


        public void SetValues(string uniformEdgeString)
        {
            if (Enum.TryParse<EdgeType>(uniformEdgeString, out EdgeType uniformEdge))
                SetValues(uniformEdge);
            else
                throw new NotImplementedException();
        }

        public void SetValues(string horizontalEdgeString, string verticalEdgeString)
        {
            if(!Enum.TryParse<EdgeType>(horizontalEdgeString, out EdgeType horizontalEdge) ||
                !Enum.TryParse<EdgeType>(verticalEdgeString, out EdgeType verticalEdge) )
                throw new NotImplementedException();
            SetValues(horizontalEdge, verticalEdge);
        }       


        public void SetValues(string leftString, string topString, string rightString, string bottomString)
        {
            if (!Enum.TryParse<EdgeType>(leftString, out EdgeType left) ||
                !Enum.TryParse<EdgeType>(topString, out EdgeType top) ||
                !Enum.TryParse<EdgeType>(rightString, out EdgeType right) ||
                !Enum.TryParse<EdgeType>(bottomString, out EdgeType bottom)
                )
                throw new NotImplementedException();
            SetValues(Left = left, Top = top, Right = right, Bottom = bottom);
        }

        public PieceConfig(string[] filsString)
        {
            if (filsString.Length == 0)
                return;
            if(filsString.Length == 1)
            {
                SetValues(filsString[0]);
                return;
            }
            if(filsString.Length < 4)
            {
                SetValues(filsString[0], filsString[1]);
                return;
            }
            SetValues(filsString[0], filsString[1], filsString[2], filsString[3]);
        }

        public static PieceConfig ParsePieceForm(string input)
        {
            string[] edges = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new (edges);
        }

        public override string ToString() => $"{Left},{Top},{Right},{Bottom}";
        public bool Equals(PieceConfig other)
        {
            return this.Left == other.Left &&
                   this.Top == other.Top &&
                   this.Right == other.Right &&
                   this.Bottom == other.Bottom;
        }
        public override bool Equals(object? obj)
        {

            return base.Equals(obj);
        }

        public PieceConfig Clone()
        {
            return new PieceConfig(Left, Top, Right, Bottom);
        }
    }
}
