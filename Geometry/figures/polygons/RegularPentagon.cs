using System.Drawing;
namespace graphic_editor;

public class RegularPentagon : RegularPolygon
{
    public RegularPentagon(
        Point center,
        double radius,
        Color lineColor,
        Color fillColor,
        double thickness)
        : base(center, 5, radius, lineColor, fillColor, thickness)
    {
    }
}
