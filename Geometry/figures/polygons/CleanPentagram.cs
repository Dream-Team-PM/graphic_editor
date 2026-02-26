
using System.Drawing;

namespace graphic_editor;

public class CleanPentagram : PolygonFigure
{
    public CleanPentagram(Point center, double outerRadius,
        Color lineColor, Color fillColor, double thickness)
        : base(CreateVertices(center, outerRadius),
               lineColor, fillColor, thickness)
    {
    }

    private static IEnumerable<Point> CreateVertices(Point center, double R)
    {
        var points = new List<Point>();
        double r = R * .382; // inner radius ratio for regular star

        for (int i = 0; i < 10; i++)
        {
            double angle = i * Math.PI / 5;
            double radius = (i % 2 == 0) ? R : r;

            points.Add(new Point(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            ));
        }

        return points;
    }
}
