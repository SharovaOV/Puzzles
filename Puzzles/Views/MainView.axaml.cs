using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Puzzles.CustomControls;

namespace Puzzles.Views;

public partial class MainView : UserControl
{
    private Control _draggedControl;
    private bool _isDragging;
    private Point _dragStartPosition;

    public MainView()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is PuzzlePiece puzzlePiece)
        {
            CloneAndDrag(puzzlePiece, e);
        }
        e.Handled = true;
    }

    private void CloneAndDrag(PuzzlePiece original, PointerPressedEventArgs e)
    {
        // Создаем копию элемента
        var clone = new PuzzlePiece
        {
            Width = original.Width,
            Height = original.Height,
            TabFill = original.TabFill,
            Stroke = original.Stroke,
            //Background = original.Background,
            PieceForm = original.PieceForm.Clone()            
        };

        clone.Classes.AddRange(original.Classes.Where(x => !x.StartsWith(":")));

        //foreach (var className in original.Classes)
        //{
        //    if (className.StartsWith(":")) continue;
        //    clone.Classes.Add(className);
        //}


        // Устанавливаем позицию рядом с оригиналом
        Canvas.SetLeft(clone, Blocks.Margin.Left + BlocksPanel.Margin.Left + original.Bounds.Left + 20);
        Canvas.SetTop(clone, Blocks.Margin.Top + BlocksPanel.Margin.Top + original.Bounds.Top + 20);

        // Добавляем на Canvas
        MainCanvas.Children.Add(clone);
        MainCanvas.PointerMoved += MainCanvas_PointerMoved;
        MainCanvas.PointerReleased += MainCanvas_PointerReleased;

        // Начинаем перетаскивание копии
        StartDrag(clone, e);
    }

    private void StartDrag(Control clone, PointerPressedEventArgs e)
    {
        _draggedControl = clone;
        _isDragging = true;
        _dragStartPosition = e.GetPosition(clone);
        if (clone is IInputElement inputElement)
        {
            // захват курсора
            e.Pointer.Capture(inputElement);
            
                clone.Classes.Add("dragging");
        }
        else
        {
            _isDragging = false;
        }

        e.Handled = true;
    }

    private void MainCanvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        if( _isDragging && _draggedControl != null)
        {
            var currentPosition = e.GetPosition(MainCanvas);
            var newX = currentPosition.X - _draggedControl.Bounds.Width/2;
            var newY = currentPosition.Y - 40;

            newX = Math.Max(0, Math.Min(MainCanvas.Bounds.Width - _draggedControl.Bounds.Width, newX));
            newY = Math.Max(0, Math.Min(MainCanvas.Bounds.Height - _draggedControl.Bounds.Height, newY));

            Canvas.SetLeft(_draggedControl, newX);
            Canvas.SetTop(_draggedControl, newY);
        }
    }

    private void MainCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging && _draggedControl is not null)
        {
            _isDragging = false;
            MainCanvas.Children.Remove(_draggedControl);
            if (_draggedControl.Bounds.Left > 2*Blocks.Margin.Left + Blocks.Bounds.Width +  WorkCanvas.Bounds.Left && 
                    _draggedControl.Bounds.Top > WorkCanvas.Bounds.Top)
            {
                WorkCanvas.Children.Add(_draggedControl);
                Canvas.SetLeft(_draggedControl, e.GetPosition(WorkCanvas).X- _draggedControl.Bounds.Width/2);
                Canvas.SetTop(_draggedControl, _draggedControl.Bounds.Top - WorkCanvas.Bounds.Top);
            }
            else
            {
               
            }
            _draggedControl.Classes.Remove("dragging");
            _draggedControl = null;

            MainCanvas.PointerMoved -= MainCanvas_PointerMoved;
            MainCanvas.PointerReleased -= MainCanvas_PointerReleased;
        }
    }

}