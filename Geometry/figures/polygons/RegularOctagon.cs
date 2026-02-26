using System.Drawing;
namespace graphic_editor;

public class RegularOctagon : RegularPolygon
{
    public RegularOctagon(
        Point center,
        double radius,
        Color lineColor,
        Color fillColor,
        double thickness)
        : base(center, 8, radius, lineColor, fillColor, thickness)
    {
    }
}
