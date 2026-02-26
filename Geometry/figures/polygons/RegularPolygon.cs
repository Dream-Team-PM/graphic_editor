
using System.Drawing;

namespace graphic_editor;

public class RegularPolygon : PolygonFigure
{
    public int Sides { get; }

    public RegularPolygon(
        Point center, int sides, double radius,
        Color lineColor, Color fillColor,
        double thickness)
        : base(CreateVertices(center, sides, radius),
               lineColor,
               fillColor,
               thickness)
    {
        if (sides < 3)
            throw new ArgumentException("Polygon must have at least 3 sides.");

        Sides = sides;
    }

    private static IEnumerable<Point> CreateVertices(
        Point center,
        int sides,
        double radius)
    {
        var points = new List<Point>();

        double angleStep = 2 * Math.PI / sides;

        // Start from top (visually nicer)
        double startAngle = -Math.PI / 2;

        for (int i = 0; i < sides; i++)
        {
            double angle = startAngle + i * angleStep;

            points.Add(new Point(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            ));
        }

        return points;
    }
}
