using Avalonia;
using Avalonia.Media;
namespace graphic_editor;

public class Ellipse : IFigure
{
    public Point Center { get; private set; }
    public double RadiusX { get; private set; }
    public double RadiusY { get; private set; }
    public double Rotation { get; private set; } // degrees

    public Ellipse(Point center, double radiusX, double radiusY)
    {
        Center = center;
        RadiusX = radiusX;
        RadiusY = radiusY;
        Rotation = 0;
    }

    public void Move(double dx, double dy)
    {
        Center = new Point(Center.X + dx, Center.Y + dy);
    }

    public void Rotate(double angleDegrees)
    {
        Rotation += angleDegrees;
    }

    public void Scale(double s)
    {
        Scale(s, s);
    }

    public void Scale(double sx, double sy)
    {
        RadiusX *= sx;
        RadiusY *= sy;
    }

    public bool IsIn(Point p)
    {
        // Transform point into ellipse local space

        double rad = -Rotation * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);

        double dx = p.X - Center.X;
        double dy = p.Y - Center.Y;

        double localX = dx * cos - dy * sin;
        double localY = dx * sin + dy * cos;

        double value =
            localX * localX / (RadiusX * RadiusX) +
            localY * localY / (RadiusY * RadiusY);

        return value <= 1.0;
    }

    public Geometry ToGeometry()
    {
        throw new NotImplementedException();
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
}
