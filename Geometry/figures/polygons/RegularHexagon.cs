using System.Drawing;
namespace graphic_editor;

public class RegularHexagon : RegularPolygon
{
    public RegularHexagon(
        Point center,
        double radius,
        Color lineColor,
        Color fillColor,
        double thickness)
        : base(center, 6, radius, lineColor, fillColor, thickness)
    {
    }
}
