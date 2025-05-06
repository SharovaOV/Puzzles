using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Puzzles.CustomControls;
using Puzzles.Enums;
using Puzzles.ViewModels;

namespace Puzzles.Views;

public partial class MainView : UserControl
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private PuzzlePiece _draggedControl;
    private bool _isDragging;
    private Point? _dragStartPosition;
    private List<Border> lightBorders = new();
    Point _mainPoint = new(30, 45);
    Size? _currentSize = null;
    public MainView()
    {
        InitializeComponent();
        
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.RegisterCanvases(WorkCanvas);
            viewModel.Deserialize += ViewModel_Deserialize;
        }
    }

    private void ViewModel_Deserialize()
    {
        foreach(Control control in WorkCanvas.Children)
        {
            control.PointerPressed += Clone_OnPointerPressed;
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is PuzzlePiece puzzlePiece)
        {
            CloneAndDrag(puzzlePiece, e);
        }
        e.Handled = true;
    } 

    private void Clone_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is PuzzlePiece puzzlePiece)
        {
            WorkCanvas.Children.Remove(puzzlePiece);
            DraggingSetX(puzzlePiece, 2*Blocks.Margin.Left + Blocks.Bounds.Width + puzzlePiece.Bounds.Left);
            DraggingSetY(puzzlePiece, puzzlePiece.Bounds.Top);

            // Добавляем на Canvas
            MainCanvas.Children.Add(puzzlePiece);

            _currentSize = new(puzzlePiece.Width, puzzlePiece.Height);
            puzzlePiece.Extract();
            // Начинаем перетаскивание копии
            StartDrag(puzzlePiece, e);
        }
        e.Handled = true;
    }

    private void CloneAndDrag(PuzzlePiece original, PointerPressedEventArgs e)
    {

        _currentSize = new(original.Bounds.Width, original.Bounds.Height);
        // Создаем копию элемента
        var clone = original.Clone();
        clone.PointerPressed += Clone_OnPointerPressed;
        clone.Classes.AddRange(original.Classes.Where(x => !x.StartsWith(":")));

        // Устанавливаем позицию рядом с оригиналом
        DraggingSetX(clone, Blocks.Margin.Left + BlocksPanel.Margin.Left + original.Bounds.Left + 20);
        DraggingSetY(clone, Blocks.Margin.Top + BlocksPanel.Margin.Top + original.Bounds.Top + 20);

        // Добавляем на Canvas
        MainCanvas.Children.Add(clone);

        // Начинаем перетаскивание копии
        StartDrag(clone, e);
    }

    private void StartDrag(PuzzlePiece clone, PointerPressedEventArgs e)
    {
        _draggedControl = clone;
        _isDragging = true;
        _dragStartPosition = e.GetPosition(clone);
        
        if (clone is IInputElement inputElement)
        {
            // захват курсора
            e.Pointer.Capture(inputElement);            
            clone.Classes.Add("dragging");
            MainCanvas.PointerMoved += MainCanvas_PointerMoved;
            MainCanvas.PointerReleased += MainCanvas_PointerReleased;
            Control? b;
            if(WorkCanvas.Children.Count>0)
                b  = WorkCanvas.Children[0];
            if (WorkCanvas.Children.Count(x=> ((PuzzlePiece)x).PuzzleConfig.ParentId == -1) == 0)
            {
                if(_draggedControl.PuzzleConfig.PersonalPieceType == PieceType.Leading)
                    CreateBackLight(_mainPoint.X, _mainPoint.Y, -1);
            }
            else
            {
                CreateSetBackLights(_draggedControl.PuzzleConfig.SideOfParent);
            }
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

            DraggingSetX(newX);
            DraggingSetY(newY);
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

                DraggingSetX( e.GetPosition(WorkCanvas).X - _draggedControl.Bounds.Width/2);
                DraggingSetY(_draggedControl.Bounds.Top - WorkCanvas.Bounds.Top);

                _draggedControl.PuzzleConfig.Consolidate();
                CorectPosition();
            }
            else
            {
               
            }
            RemoveBackLight();
            _currentSize = null;
            _draggedControl.Classes.Remove("dragging");
            _draggedControl = null;

            MainCanvas.PointerMoved -= MainCanvas_PointerMoved;
            MainCanvas.PointerReleased -= MainCanvas_PointerReleased;
        }
    }

    void CorectPosition()
    {
        Point pointControl;
        Point point = new(Canvas.GetLeft(_draggedControl), Canvas.GetTop(_draggedControl));
        foreach(Control control in BacklightCanvas.Children)
        {
            pointControl = new(Canvas.GetLeft(control), Canvas.GetTop(control));
            if (point.X >= pointControl.X && point.X <= pointControl.X + control.Bounds.Width && point.Y >= pointControl.Y && point.Y <= pointControl.Y + control.Bounds.Height)
            {
                DraggingSetX(pointControl.X);
                DraggingSetY(pointControl.Y);
                Backlight backlight = control as Backlight;
                if (backlight != null)
                {
                    if (backlight.ParentId != -1){
                        PuzzlePiece parent = (PuzzlePiece)WorkCanvas.Children.First(x => ((PuzzlePiece)x).PuzzleConfig.Id == backlight.ParentId);
                        Debug.WriteLine($"{parent.PuzzleConfig.Id}, {_draggedControl.PuzzleConfig.Id}");
                        parent.AddSlave(_draggedControl);
                    }
                    else _draggedControl.SetAsMainPiece();
                }
                break;
            }
        }        
    }

    private void CreateBackLight(double x, double y, int parentId)
    {
        // Проверка необходимых условий
        if (_draggedControl == null || BacklightCanvas == null)
            return;

        // Создаем подсветку с параметрами Avalonia
        var backlight = new Backlight
        {
            ParentId = parentId,
            Opacity = 0.5
        };

        Canvas.SetLeft(backlight, x);
        Canvas.SetTop(backlight, y);

        // Добавляем на Canvas
        BacklightCanvas.Children.Add(backlight);

        // Анимация появления (специфичная для Avalonia)
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(400),
            FillMode = FillMode.Forward,
            Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0),
                Setters = { new Setter(OpacityProperty, 0.0) }
            },
            new KeyFrame
            {
                Cue = new Cue(1),
                Setters = { new Setter(OpacityProperty, 0.5) }
            }
        }
        };

        animation.RunAsync(backlight);
    }

    void DraggingSetX( PuzzlePiece puzzlePiece, double x)
    {
        Canvas.SetLeft(puzzlePiece, x);
        if(puzzlePiece.PuzzleConfig != null)
            puzzlePiece.PuzzleConfig.X = x;
    }
    void DraggingSetY(PuzzlePiece puzzlePiece, double y)
    {
        Canvas.SetTop(puzzlePiece, y);

        if (puzzlePiece.PuzzleConfig != null)
            puzzlePiece.PuzzleConfig.Y = y;
    }
    
    void DraggingSetX(double x)=>DraggingSetX(_draggedControl, x);
    void DraggingSetY( double y) => DraggingSetY(_draggedControl, y);

    private void CreateSetBackLights(SideType sideType)
    {
        foreach(PuzzlePiece puzzlePiece in WorkCanvas.Children)
        {
            if (puzzlePiece.PuzzleConfig == null|| puzzlePiece.PuzzleConfig.ParentId == 0) continue;

            if (puzzlePiece.PuzzleConfig.CanHaveChields[(int)sideType] && puzzlePiece.PuzzleConfig.ChieldrenId[(int)sideType] == 0)
            {
                Point point = sideType switch { 
                    SideType.Bottom => new(Canvas.GetLeft(puzzlePiece), Canvas.GetTop(puzzlePiece) + puzzlePiece.Height-20),
                    SideType.Right => new(Canvas.GetLeft(puzzlePiece) + puzzlePiece.Width-20, Canvas.GetTop(puzzlePiece) ),
                };

                CreateBackLight(point.X, point.Y, puzzlePiece.PuzzleConfig.Id);
            }
        }
    }

    void RemoveBackLight()
    {
        var controls = BacklightCanvas.Children.ToArray();
        foreach(Control control in controls)
        {
            BacklightCanvas.Children.Remove(control);
        }
    }

}