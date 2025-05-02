using System;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Puzzles.Converters;
using Puzzles.Models;

namespace Puzzles.CustomControls;

public class PuzzlePiece : TemplatedControl
{
    private const double RADIUS_SLOT_CORNER = 5;
    private Point _startPoint;
    private bool _isDragging;
    private Canvas? _parentCanvas;
    private Border? _dragHandle;
    private Path? _pazzlePath;
    private TextBox _textBox;
    private double _yStart;
    private double _yEnd;
    public static readonly StyledProperty<IBrush> TabFillProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(TabFill));

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(Stroke));


    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<PuzzlePiece, double>(nameof(StrokeThickness), 1.5);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PuzzlePiece, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);


   
    public static readonly StyledProperty<PieceConfig> PieceFormProperty =
        AvaloniaProperty.Register<PuzzlePiece, PieceConfig>(nameof(PieceForm),
            new PieceConfig(Enums.EdgeType.Tab),
            validate: v => v != null,
            coerce: (_, value) => value ?? new PieceConfig(Enums.EdgeType.Tab)
           );

    
    public PieceConfig PieceForm
    {
        get => this.GetValue(PieceFormProperty);
        set => SetValue(PieceFormProperty, value);
    }
    // Дополнительный сеттер специально для строковых значений
    public void SetPieceForm(string formString)
    {
        SetValue(PieceFormProperty, PieceConfig.ParsePieceForm(formString));
    }


    public IBrush TabFill
    {
        get => this.GetValue(TabFillProperty);
        set => SetValue(TabFillProperty, value);
    }

    public IBrush Stroke
    {
        get => this.GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => this.GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public string Text
    {
        get => this.GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    private static void OnPieceFormChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Sender is PuzzlePiece control && args.NewValue is string strValue)
        {
            // Если передано строковое значение, парсим его в PieceConfig
            control.SetValue(PieceFormProperty, PieceConfig.ParsePieceForm(strValue));
        }
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pazzlePath = e.NameScope.Get<Path>("PART_PazzlePath");
        _textBox = e.NameScope.Get<TextBox>("TextElement");
        _textBox.TextChanged += TextBox_TextChanged;
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        if (string.IsNullOrEmpty(textBox.Text)) return;

        textBox.Text = Regex.Replace(textBox.Text, @"[^a-zA-Z0-9]", "");
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_pazzlePath != null)
        {
            _pazzlePath.Data = CreatePathData();
        }
    }


    private Geometry CreatePathData()
    {
        var data = new StreamGeometry();
        double topPoint = 50;
        double leftPoint = 40;
        double segmentSize = 20;
        //double halfSegment = Math.Min(this.Bounds.Height, this.Bounds.Height) / 2;
        if (PieceForm is null) return null;
        using (var ctx = data.Open())
        {
            CreateTopEdge(ctx, leftPoint, segmentSize);
            CreateRightEdge(ctx, topPoint, segmentSize);

            CreateBottomEdge(ctx, leftPoint, segmentSize);

            CreateLeftEdge(ctx, topPoint, segmentSize);
            
            return data;
        }
        return null;
    }

    private void CreateTopEdge(StreamGeometryContext ctx, double leftPoint, double segmentSize)
    {
        double startX =(PieceForm.Left == Enums.EdgeType.None || PieceForm.Top == Enums.EdgeType.None)? 0.5 * segmentSize+CornerRadius.TopLeft : 0.5 * segmentSize;
        _yStart = 0.5 * segmentSize;
        ctx.BeginFigure(new Point(startX, _yStart), true);
        ctx.LineTo(new Point(leftPoint, _yStart));
        if(PieceForm.Top == Enums.EdgeType.Slot)
        {
            ctx.LineTo(new Point(leftPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, _yStart + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(leftPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER, _yStart + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER),
            
                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0,false, SweepDirection.CounterClockwise
                );
            ctx.LineTo(new Point(leftPoint +  segmentSize, _yStart));
        }
        else if(PieceForm.Top == Enums.EdgeType.Tab)
        {
            ctx.LineTo(new Point(leftPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, _yStart - 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(leftPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER, _yStart - 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER),
                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0,false, SweepDirection.Clockwise
                );
            ctx.LineTo(new Point(leftPoint +  segmentSize, _yStart));
        }

        //if() Логика скругления Если понадобится
        ctx.LineTo(new Point(this.Bounds.Width-0.5*segmentSize, _yStart));
    }

    private void CreateRightEdge(StreamGeometryContext ctx, double topPoint, double segmentSize)
    {
        double x = this.Bounds.Width - (0.5 * segmentSize);
        ctx.LineTo(new Point(x, topPoint));
        if (PieceForm.Right == Enums.EdgeType.Slot)
        {
            ctx.LineTo(new Point(x - 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(x - 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER),

                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0, false, SweepDirection.CounterClockwise
                );
            ctx.LineTo(new Point(x, topPoint+ segmentSize));
        }
        else if (PieceForm.Right == Enums.EdgeType.Tab)
        {
            ctx.LineTo(new Point(x + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(x + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER),

                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0, false, SweepDirection.Clockwise
                );
            ctx.LineTo(new Point(x, topPoint + segmentSize));

        }
        ctx.LineTo(new Point(x, this.Bounds.Height-0.5*segmentSize));
    }

    private void CreateBottomEdge(StreamGeometryContext ctx, double leftPoint, double segmentSize)
    {
        _yEnd = this.Bounds.Height - 0.5 * segmentSize;
        ctx.LineTo(new Point(leftPoint + segmentSize, _yEnd));
        if (PieceForm.Bottom == Enums.EdgeType.Slot)
        {
            ctx.LineTo(new Point(leftPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER, _yEnd - 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(leftPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, _yEnd - 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER),
                new Size(2*RADIUS_SLOT_CORNER, 2*RADIUS_SLOT_CORNER),
                0, false, SweepDirection.CounterClockwise
                );
            ctx.LineTo(new Point(leftPoint, _yEnd));
        }
        else if (PieceForm.Bottom == Enums.EdgeType.Tab)
        {
            ctx.LineTo(new Point(leftPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER, _yEnd + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(leftPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, _yEnd + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER),
                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0, false, SweepDirection.Clockwise
                );
            ctx.LineTo(new Point(leftPoint, _yEnd));
        }
        if(PieceForm.Left == Enums.EdgeType.None || PieceForm.Bottom == Enums.EdgeType.None)
        {
            ctx.LineTo(new Point(0.5 * segmentSize + this.CornerRadius.BottomLeft, _yEnd));
            ctx.ArcTo(new Point(0.5 * segmentSize , _yEnd - CornerRadius.BottomLeft),
                    new Size(this.CornerRadius.TopLeft, this.CornerRadius.TopLeft),
                    0, false, SweepDirection.Clockwise);
        }
        else
            ctx.LineTo(new Point(0.5 * segmentSize, _yEnd));
    }
    private void CreateLeftEdge(StreamGeometryContext ctx, double topPoint, double segmentSize)
    {
        double endY =  _yStart;
        endY += (PieceForm.Left == Enums.EdgeType.None || PieceForm.Top == Enums.EdgeType.None) ? 
            CornerRadius.TopLeft :0;
        ctx.LineTo(new Point(0.5 * segmentSize, topPoint + segmentSize));
        if (PieceForm.Left == Enums.EdgeType.Slot)
        {
            ctx.LineTo(new Point(segmentSize - 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(segmentSize - 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER),

                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0, false, SweepDirection.CounterClockwise
                );
            ctx.LineTo(new Point(0.5 * segmentSize, topPoint));
        }
        else if(PieceForm.Left == Enums.EdgeType.Tab)
        {
            ctx.LineTo(new Point( 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize + 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point( 0.5 * RADIUS_SLOT_CORNER, topPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER),

                new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0, false, SweepDirection.Clockwise
                );
            ctx.LineTo(new Point(0.5 * segmentSize, topPoint));

        }
            ctx.LineTo(new Point(0.5 * segmentSize, endY));
        if (PieceForm.Left == Enums.EdgeType.None || PieceForm.Top == Enums.EdgeType.None)
        {
            ctx.ArcTo(new Point(0.5*segmentSize + this.CornerRadius.TopLeft, _yStart),
                     new Size(this.CornerRadius.TopLeft, this.CornerRadius.TopLeft),
                     0, false, SweepDirection.Clockwise);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if(change.Property == PieceFormProperty)
        {
            if (_pazzlePath != null)
            {
                _pazzlePath.Data = CreatePathData();
            }
        }
    }
   

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
    }

}