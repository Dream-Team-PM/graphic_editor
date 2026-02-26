using System.Drawing;

namespace graphic_editor;

public class Triangle : PolygonFigure
{
    public Triangle(Point a, Point b, Point c,
        Color lineColor, Color fillColor, double thickness)
        : base(new List<Point> { a, b, c },
               lineColor, fillColor, thickness)
    {
    }
}