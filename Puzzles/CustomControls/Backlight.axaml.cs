using Avalonia;
using Avalonia.Controls.Primitives;
using Puzzles.Enums;

namespace Puzzles.CustomControls;

public class Backlight : TemplatedControl
{
    public static readonly StyledProperty<int> ParentIdProperty =
        AvaloniaProperty.Register<Backlight, int>(nameof(ParentId), 0);

    public static readonly StyledProperty<SideType> PositionRelativeToParentProperty =
        AvaloniaProperty.Register<Backlight, SideType>(nameof(PositionRelativeToParent));

    public int ParentId
    {
        get => this.GetValue(ParentIdProperty);
        set => SetValue(ParentIdProperty, value);
    }

    public SideType PositionRelativeToParent
    {
        get => this.GetValue(PositionRelativeToParentProperty);
        set => SetValue(PositionRelativeToParentProperty, value);
    }


}