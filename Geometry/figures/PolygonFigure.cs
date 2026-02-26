using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Avalonia.Media;

namespace graphic_editor;

public abstract class PolygonFigure : IPolygon
{
    protected List<Point> vertices;

    public IReadOnlyList<Point> Vertices => vertices;

    public System.Drawing.Color LineColor { get; }
    public System.Drawing.Color FillColor { get; }
    public double Thickness { get; }

    protected PolygonFigure(IEnumerable<Point> points,
        System.Drawing.Color lineColor, System.Drawing.Color fillColor, double thickness)
    {
        vertices = points.ToList();
        LineColor = lineColor;
        FillColor = fillColor;
        Thickness = thickness;
    }

    protected Point Center =>
        new Point(vertices.Average(p => p.X),
                  vertices.Average(p => p.Y));

    public void Move(double dx, double dy)
    {
        for (int i = 0; i < vertices.Count; ++i)
            vertices[i] = new Point(vertices[i].X + dx, vertices[i].Y + dy);
    }

    public void Rotate(double angleDegrees)
    {
        var center = Center;
        for (int i = 0; i < vertices.Count; ++i)
            vertices[i] = Transform2D.Rotate(vertices[i], center, angleDegrees);
    }

    public void Scale(double sx, double sy)
    {
        var center = Center;
        for (int i = 0; i < vertices.Count; ++i)
            vertices[i] = Transform2D.Scale(vertices[i], center, sx, sy);
    }

    public void Scale(double s)
    {
        Scale(s, s);
    }

    public  virtual bool IsIn(Point p)
    {
        int count = 0;

        for (int i = 0; i < Vertices.Count; i++)
        {
            var a = Vertices[i];
            var b = Vertices[(i + 1) % Vertices.Count];

            if (RayIntersectsSegment(p, a, b)) ++count;
        }

        return (count & 1) == 1;
    }

    private bool RayIntersectsSegment(Point p, Point a, Point b)
    {
        if (a.Y > b.Y) (a, b) = (b, a);

        if (p.Y == a.Y || p.Y == b.Y) p = new Point(p.X, p.Y + 0.0001);

        if (p.Y < a.Y || p.Y > b.Y) return false;

        if (a.X > b.X)
        {
            if (p.X > a.X) return false;
        }
        else if (p.X > b.X) return false;

        double intersection = (p.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y) + a.X;

        return p.X < intersection;
    }

    public bool HasIntersection(Point lefttop, Point rightbottom)
    {
        throw new NotImplementedException();
    }

    public IFigure Intersection(IFigure figure)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IDrawFigure> Draw()
    {
        throw new NotImplementedException();
    }

    public Geometry ToGeometry()
    {
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(
                new Avalonia.Point(vertices[0].X, vertices[0].Y),
                isFilled: true
            );

            for (int i = 1; i < vertices.Count; ++i)
                ctx.LineTo(new Avalonia.Point(vertices[i].X, vertices[i].Y));

            ctx.EndFigure(isClosed: true);
        }

        return geometry;
    }
}
