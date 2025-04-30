using System;
using System.Runtime.Intrinsics.X86;
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




    public static readonly StyledProperty<IBrush> TabFillProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(TabFill), Brushes.Violet);

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(Stroke), Brushes.DarkViolet);


    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<PuzzlePiece, double>(nameof(StrokeThickness), 1.5);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PuzzlePiece, string>(nameof(Text));


   
    public static readonly StyledProperty<PieceConfig> PieceFormProperty =
        AvaloniaProperty.Register<PuzzlePiece, PieceConfig>(nameof(PieceForm),
            new PieceConfig(Enums.EdgeType.Slot),
            validate: v => v != null,
            coerce: (_, value) => value ?? new PieceConfig(Enums.EdgeType.Slot)
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
        double topPoint = 100;
        double leftPoint = 90;
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
        double startX =(PieceForm.Left == Enums.EdgeType.None || PieceForm.Top == Enums.EdgeType.None)?CornerRadius.TopLeft : 0;
        double yStart = (PieceForm.Top == Enums.EdgeType.Slot) ? 0.5 * segmentSize : 0;
        ctx.BeginFigure(new Point(startX, yStart), true);
        ctx.LineTo(new Point(leftPoint, yStart));
        if(PieceForm.Top == Enums.EdgeType.Tab)
        {
            ctx.LineTo(new Point(leftPoint + 0.5 * segmentSize - RADIUS_SLOT_CORNER, yStart + 0.5 * segmentSize - RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(leftPoint + 0.5 * segmentSize - RADIUS_SLOT_CORNER, yStart + 0.5 * segmentSize - RADIUS_SLOT_CORNER),
                new Size(2*RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0,false, SweepDirection.Clockwise
                );
            ctx.LineTo(new Point(leftPoint +  segmentSize, yStart));
        }
        else if(PieceForm.Top == Enums.EdgeType.Slot)
        {
            ctx.LineTo(new Point(leftPoint + 0.5 * segmentSize - RADIUS_SLOT_CORNER, yStart - 0.5 * segmentSize + RADIUS_SLOT_CORNER));
            ctx.ArcTo(
                new Point(leftPoint + 0.5 * segmentSize - RADIUS_SLOT_CORNER, yStart - 0.5 * segmentSize + RADIUS_SLOT_CORNER),
                new Size(2*RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
                0,false, SweepDirection.Clockwise
                );
            ctx.LineTo(new Point(leftPoint +  segmentSize, yStart));
        }

        //if() Логика скругления
        ctx.LineTo(new Point(this.Bounds.Width, yStart));
    }

    private void CreateRightEdge(StreamGeometryContext ctx, double topPoint, double segmentSize)
    {   
        ctx.LineTo(new Point(this.Bounds.Width, this.Bounds.Height));
    }

    private void CreateBottomEdge(StreamGeometryContext ctx, double leftPoint, double segmentSize)
    {   
        ctx.LineTo(new Point(0, this.Bounds.Height));
    }
    private void CreateLeftEdge(StreamGeometryContext ctx, double topPoint, double segmentSize)
    {
        double endY = PieceForm.Top == Enums.EdgeType.Slot ? 0.5 * segmentSize : 0;
        endY += (PieceForm.Left == Enums.EdgeType.None || PieceForm.Top == Enums.EdgeType.None) ? 
            CornerRadius.TopLeft :0;

        ctx.LineTo(new Point(0, endY));
        if (PieceForm.Left == Enums.EdgeType.None || PieceForm.Top == Enums.EdgeType.None)
        {
            ctx.ArcTo(new Point(0, endY),
                     new Size(2*this.CornerRadius.TopLeft, this.CornerRadius.TopLeft),
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