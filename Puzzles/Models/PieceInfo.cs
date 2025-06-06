﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Puzzles.Enums;

namespace Puzzles.Models
{
    public partial class PieceInfo : ObservableObject, IDisposable
    {
        protected static int _lastId = 1;
        private int _id =0 ;
        public int Id { 
            get =>_id; 
            set 
            { 
                _id = value;  
                _lastId = int.Max(_lastId, value) + 1; 
            } 
        }
        public PieceType PersonalPieceType { get; set; }
        public SideType SideOfParent { get; set; }
        public int ParentId { get; set; } = 0;
        public int[] ChieldrenId { get;  set; } = new int[4];
        public bool[] CanHaveChields { get; set; } = new bool[4];
        public bool IsFirstPies { get; set; } = false;
        public string Text { get; set; }
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double Width { get; set; }
        public double Height { get; set; }

        public IBrush Color { get; set; } =  Brushes.Violet;
        public IBrush StrokeColor { get; set; } =  Brushes.DarkViolet;

        public PieceInfo(PieceType pieceType, IBrush color, IBrush strokeColor, string text)
        {
            Color = color;
            StrokeColor = strokeColor;
            Text = text;
            if (pieceType == PieceType.Leading)
                LeadingInit();
            else
                SlaveInit();
        }
        public PieceInfo() { }
        private void LeadingInit()
        {
            PersonalPieceType = PieceType.Leading;
            CanHaveChields[(int)SideType.Right] = true;
            CanHaveChields[(int)SideType.Bottom] = true;
            SideOfParent = SideType.Bottom;
        }
        private void SlaveInit()
        {
            PersonalPieceType = PieceType.Slave;
            SideOfParent = SideType.Right;
        }

        public void Consolidate()
        {
            if(Id == 0)
                Id = _lastId++;
        }
        public void Dispose()
        {
           
        }
    }
}
