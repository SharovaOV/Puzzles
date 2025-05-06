using Avalonia;
using System.Formats.Tar;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System;
using static Puzzles.ProgramData;

namespace Puzzles.CustomControls;

public class MainBlock : TemplatedControl
{
    private const double HORIZONTAL_WIDTH = 40;
    private const double VERTICAL_WIDTH = 55;
    private const double CORNER_RADIUS = 10;
    private const double RADIUS_SLOT_CORNER = 5;

    private Path? _pazzlePath;

    public static readonly StyledProperty<IBrush> TabFillProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(TabFill));

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(Stroke));


    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<PuzzlePiece, double>(nameof(StrokeThickness), 1.5);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PuzzlePiece, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    

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

    private Geometry? CreatePathData()
    {
        var data = new StreamGeometry();
        double leftPoint = LEFT_POINT -0.5* SEGMENT_SIZE;
        double segmentSize = SEGMENT_SIZE;

        using (var ctx = data.Open())
        {
            ctx.BeginFigure(new Point(CORNER_RADIUS, 0), true);
            ctx.LineTo(new Point(this.Bounds.Width, 0));
            ctx.LineTo(new Point(this.Bounds.Width, VERTICAL_WIDTH));
            ctx.LineTo(new Point(HORIZONTAL_WIDTH + leftPoint + segmentSize, VERTICAL_WIDTH));
            ctx.LineTo(new Point(HORIZONTAL_WIDTH + leftPoint + 0.5*segmentSize + 0.5*RADIUS_SLOT_CORNER, VERTICAL_WIDTH + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER));
            ctx.ArcTo(
               new Point(HORIZONTAL_WIDTH + leftPoint + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER, VERTICAL_WIDTH + 0.5 * segmentSize - 0.5 * RADIUS_SLOT_CORNER),
               new Size(RADIUS_SLOT_CORNER, RADIUS_SLOT_CORNER),
               0, false, SweepDirection.Clockwise
               );
            ctx.LineTo(new Point(HORIZONTAL_WIDTH + leftPoint, VERTICAL_WIDTH));
            ctx.LineTo(new Point(HORIZONTAL_WIDTH + CORNER_RADIUS, VERTICAL_WIDTH));
            ctx.ArcTo(
              new Point(HORIZONTAL_WIDTH, VERTICAL_WIDTH + CORNER_RADIUS),
              new Size(CORNER_RADIUS, CORNER_RADIUS),
              0, false, SweepDirection.CounterClockwise
              );
            ctx.LineTo(new Point(HORIZONTAL_WIDTH, this.Bounds.Height - VERTICAL_WIDTH - CORNER_RADIUS));
            ctx.ArcTo(
              new Point(HORIZONTAL_WIDTH + CORNER_RADIUS, this.Bounds.Height - VERTICAL_WIDTH),
              new Size(CORNER_RADIUS, CORNER_RADIUS),
              0, false, SweepDirection.CounterClockwise
              );
            ctx.LineTo(new Point(this.Bounds.Width, this.Bounds.Height - VERTICAL_WIDTH));
            ctx.LineTo(new Point(this.Bounds.Width, this.Bounds.Height));
            ctx.LineTo(new Point( CORNER_RADIUS, this.Bounds.Height));
            ctx.ArcTo(
              new Point(0, this.Bounds.Height - CORNER_RADIUS),
              new Size(CORNER_RADIUS, CORNER_RADIUS),
              0, false, SweepDirection.Clockwise
              );
            ctx.LineTo(new Point(0, CORNER_RADIUS));
            ctx.ArcTo(
              new Point(CORNER_RADIUS, 0),
              new Size(CORNER_RADIUS, CORNER_RADIUS),
              0, false, SweepDirection.Clockwise
              );
        }
        return data;
    }
}