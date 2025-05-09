using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Puzzles.Converters;
using Puzzles.Enums;
using Puzzles.Models;
using static Puzzles.ProgramData;

namespace Puzzles.CustomControls;

public class PuzzlePiece : TemplatedControl
{
    private const double RADIUS_SLOT_CORNER = 5;
    private Path? _pazzlePath;
    private TextBox? _textBox;
    private double _yStart;
    private double _yEnd;

    private Button _paletteBtn;


    //public PuzzlePiece SlavePieces { get; } = new(); // �������

    // ������� ���������� (���� �������)
    public Point? ConnectionPoint { get; set; }   


    public static readonly StyledProperty<IBrush> TabFillProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(TabFill));

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<PuzzlePiece, IBrush>(nameof(Stroke));


    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<PuzzlePiece, double>(nameof(StrokeThickness), 1.5);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PuzzlePiece, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<PieceInfo> PuzzleConfigProperty =
        AvaloniaProperty.Register<PuzzlePiece, PieceInfo>(nameof(PuzzleConfig), null);


    public static readonly StyledProperty<PuzzlePiece?> MasterPieceProperty =
        AvaloniaProperty.Register<PuzzlePiece, PuzzlePiece?>(nameof(MasterPiece), null);

    public static readonly StyledProperty<PuzzlePiece?[]> SlavePiecesProperty =
        AvaloniaProperty.Register<PuzzlePiece, PuzzlePiece?[]>(nameof(SlavePieces), new PuzzlePiece?[4], coerce: (o, v) => v ?? new PuzzlePiece?[4]);

    public PuzzlePiece?[] SlavePieces
    {
        get => this.GetValue(SlavePiecesProperty);
        set => SetValue(SlavePiecesProperty, value);
    }

    public PuzzlePiece? MasterPiece
    {
        get => this.GetValue(MasterPieceProperty);
        set => SetValue(MasterPieceProperty, value);
    }

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

    // �������������� ������ ���������� ��� ��������� ��������
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
    
    public PieceInfo PuzzleConfig
    {
        get => this.GetValue(PuzzleConfigProperty);
        set => SetValue(PuzzleConfigProperty, value);
    }
    
    private static void OnPieceFormChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Sender is PuzzlePiece control && args.NewValue is string strValue)
        {
            // ���� �������� ��������� ��������, ������ ��� � PieceConfig
            control.SetValue(PieceFormProperty, PieceConfig.ParsePieceForm(strValue));
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pazzlePath = e.NameScope.Get<Path>("PART_PazzlePath");

        _textBox = e.NameScope.Get<TextBox>("TextElement");
        _textBox.TextChanged += TextBox_TextChanged;

        _paletteBtn = e.NameScope.Get<Button>("PaletteButton");
        _paletteBtn.Click += PaletteButton_Click;
        //SlavePieces = new PuzzlePiece[4];
    }

    private void PaletteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
       (TabFill, Stroke) =  GetRandomColorPair();
        DrawingPath();
        SetSlaveCollors(TabFill, Stroke);
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
        
            DrawingPath();
    }

    public void DrawingPath()
    {
        if (_pazzlePath != null)
        {
            _pazzlePath.Data = CreatePathData();
        }
    }
    private Geometry? CreatePathData()
    {
        var data = new StreamGeometry();
        if (PieceForm is null) return null;
        using (var ctx = data.Open())
        {
            CreateTopEdge(ctx, LEFT_POINT, SEGMENT_SIZE);

            CreateRightEdge(ctx, TOP_POINT, SEGMENT_SIZE);

            CreateBottomEdge(ctx, LEFT_POINT, SEGMENT_SIZE);

            CreateLeftEdge(ctx, TOP_POINT, SEGMENT_SIZE);
            
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

        //if() ������ ���������� ���� �����������
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
        PuzzleConfigUpdate();
        if (change.Property == PieceFormProperty)
        {
            if (_pazzlePath != null)
            {
                _pazzlePath.Data = CreatePathData();
            }
        }
        //else if(change.Property == MasterPieceProperty)
        //{
        //    PuzzleConfig.ParentId = (MasterPiece != null)? MasterPiece.PuzzleConfig.Id:0;
        //}
    }
   
    public void Extract()
    {
        if (PuzzleConfig == null) return;
        ExtractAsMainPiece();
        ExtractMaster();
        foreach (PuzzlePiece slavePiece in SlavePieces)
        {
           RemoveSlave(slavePiece);
        }
    }

    public void AddSlave(PuzzlePiece slave)
    {
        if (slave == null || PuzzleConfig == null) return;
        Debug.WriteLine($"AddSlave {slave.PuzzleConfig.Id} to {PuzzleConfig.Id}");
        slave.SetMaster(this);

        if(slave.PuzzleConfig.PersonalPieceType == PieceType.Slave)
        {
            slave.TabFill = this.TabFill;
            slave.Stroke = this.Stroke;
        }
        SlavePieces[(int)slave.PuzzleConfig.SideOfParent] = slave;
        PuzzleConfig.ChieldrenId[(int)slave.PuzzleConfig.SideOfParent] = slave.PuzzleConfig.Id;
    }
   
    public void RemoveSlave(PuzzlePiece slave)
    {
        if (slave == null || PuzzleConfig == null) return;
        slave.SetMaster(null);
        SlavePieces[(int)slave.PuzzleConfig.SideOfParent] = null;
        PuzzleConfig.ChieldrenId[(int)slave.PuzzleConfig.SideOfParent] = 0;
        slave.Extract();
    }
    public void SetMaster(PuzzlePiece master)
    {
        MasterPiece = master;
        PuzzleConfig.ParentId = master?.PuzzleConfig.Id??0;
        if (MasterPiece != null && PuzzleConfig.PersonalPieceType == PieceType.Slave && _textBox != null)
            _textBox.Text = MasterPiece.Text;
    }
    public void SetAsMainPiece()
    {
        PuzzleConfig.IsFirstPies = true;
        PuzzleConfig.ParentId = -1;
    }
    public void ExtractAsMainPiece()
    {
        if (!PuzzleConfig.IsFirstPies) return;
        PuzzleConfig.ParentId = 0;
        PuzzleConfig.IsFirstPies = false;
    }
    public void ExtractMaster()
    {
        MasterPiece?.RemoveSlave(this);
    }

    public PuzzlePiece Clone()
    {
        return new PuzzlePiece
        {
            Width = Width,
            Height = Height,
            TabFill = TabFill,
            Stroke = Stroke,
            SlavePieces = new PuzzlePiece[4],
            PieceForm = PieceForm.Clone(),
            PuzzleConfig = new PieceInfo
            (
                Enum.Parse<PieceType>(this.Classes.First(x => x != "dragging" && !x.Contains(":"))),
                TabFill,
                Stroke,
                Text
             )
            {
                Width = Width,
                Height = Height,
                ParentId = 0,
                ChieldrenId = new int[4]
            }
        };

    }

    public void SetSlaveCollors(IBrush fill, IBrush stroke)
    {
        foreach(PuzzlePiece slave in SlavePieces)
        {
            if(slave !=null && slave.PuzzleConfig.PersonalPieceType == PieceType.Slave)
            {
                slave.TabFill = fill;
                slave.Stroke = stroke;
                slave.DrawingPath();
            }
        }
    }

    void PuzzleConfigUpdate()
    {
        if (PuzzleConfig == null) return;
        PuzzleConfig.Width = (Bounds.Width > 0) ? Bounds.Width : PuzzleConfig.Width;
        PuzzleConfig.Height = (Bounds.Height > 0) ? Bounds.Height : PuzzleConfig.Height;
        PuzzleConfig.Color = TabFill;
        PuzzleConfig.StrokeColor = Stroke;
        PuzzleConfig.Text = Text;
    }

    public static PuzzlePiece CreateFromPuzzleConfig(PieceInfo puzzleConfig)
    {
        PuzzlePiece puzzlePiece = new PuzzlePiece
        {
            Width = puzzleConfig.Width,
            Height = puzzleConfig.Height,
            TabFill = puzzleConfig.Color,
            Stroke = puzzleConfig.StrokeColor,
            Text = puzzleConfig.Text,
            MasterPiece = null,
            SlavePieces = new PuzzlePiece[4],
            PuzzleConfig = puzzleConfig
        };
        var s = $"{puzzleConfig.PersonalPieceType}";
        puzzlePiece.Classes.Add($"{puzzleConfig.PersonalPieceType}");

        return puzzlePiece;
    }

}