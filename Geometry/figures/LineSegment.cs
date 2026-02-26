using System.Drawing;

namespace graphic_editor;


public class LineSegment : IFigure, IGraphicFigure
{
    private Point _p1;
    private Point _p2;

    public Color LineColor { get; }
    public Color FillColor { get; }
    public double Thickness { get; }

    public LineSegment(Point p1, Point p2, Color lineColor, double thickness = 1)
    {
        _p1 = p1;
        _p2 = p2;
        LineColor = lineColor;
        FillColor = Color.Transparent;
        Thickness = thickness;
    }

    public Point Center => (_p1 + _p2) * .5;

    public ReadOnlySpan<Point> Vertex
    {
        get { return new[] { _p1, _p2 }; }
    }


    public void Move(double dx, double dy)
    {
        _p1 = new Point(_p1.X + dx, _p1.Y + dy);
        _p2 = new Point(_p2.X + dx, _p2.Y + dy);
    }

    public void Rotate(double angle)
    {
        var center = Center;
        _p1 = RotatePoint(_p1, center, angle);
        _p2 = RotatePoint(_p2, center, angle);
    }

    public void Scale(double sx, double sy)
    {
        var center = Center;
        _p1 = ScalePoint(_p1, center, sx, sy);
        _p2 = ScalePoint(_p2, center, sx, sy);
    }

    public void Scale(double s)
    {
        Scale(s, s);
    }

    public void Reflection(Point a, Point b)
    {
        _p1 = ReflectPoint(_p1, a, b);
        _p2 = ReflectPoint(_p2, a, b);
    }

    // ===============================
    // Geometry
    // ===============================

    public bool IsIn(Point point)
    {
        // Distance from point to segment
        return DistancePointToSegment(point, _p1, _p2) <= 1e-6;
    }

    public bool HasIntersection(Point lefttop, Point rightbottom)
    {
        double minX = Math.Min(lefttop.X, rightbottom.X);
        double maxX = Math.Max(lefttop.X, rightbottom.X);
        double minY = Math.Min(lefttop.Y, rightbottom.Y);
        double maxY = Math.Max(lefttop.Y, rightbottom.Y);

        return !(
            Math.Max(_p1.X, _p2.X) < minX ||
            Math.Min(_p1.X, _p2.X) > maxX ||
            Math.Max(_p1.Y, _p2.Y) < minY ||
            Math.Min(_p1.Y, _p2.Y) > maxY
        );
    }

    public IFigure Intersection(IFigure figure)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IDrawFigure> Draw()
    {
        throw new NotImplementedException();
    }

    private static Point RotatePoint(Point p, Point center, double angle)
    {
        double rad = angle * Math.PI / 180.0,
               cos = Math.Cos(rad),
               sin = Math.Sin(rad);

        Point r = p - center;

        return new Point(
            center.X + r.X * cos - r.Y * sin,
            center.Y + r.X * sin + r.Y * cos
        );
    }

    private static Point ScalePoint(Point p, Point center, double sx, double sy)
    {
        return new Point(
            center.X + (p.X - center.X) * sx,
            center.Y + (p.Y - center.Y) * sy
        );
    }

    private static Point ReflectPoint(Point p, Point a, Point b)
    {
        // Reflection across line AB
        Point d = b - a;

        double A = d.Y,
               B = -d.X,
               C = d.X * a.Y - d.Y * a.X,
               D = (A * p.X + B * p.Y + C) / (A * A + B * B);

        return new Point(
            p.X - 2 * A * D,
            p.Y - 2 * B * D
        );
    }

    private static double DistancePointToSegment(Point p, Point a, Point b)
    {
        Point d = b - a;

        if (d.X == 0 && d.Y == 0)
            return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

        double t = Math.Max(
            0,
            Math.Min(
                1,
                ((p.X - a.X) * d.X + (p.Y - a.Y) * d.Y) / (d.X * d.X + d.Y * d.Y)
            )
        );

        double projX = a.X + t * d.X, projY = a.Y + t * d.Y;

        return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
    }

    public Avalonia.Media.Geometry ToGeometry()
    {
        throw new NotImplementedException();
    }
}
