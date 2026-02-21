using System.Drawing;

namespace graphic_editor.Models;

public record Point_1(double X, double Y)
{
    public static Point_1 Zero => new(0, 0);
    public Point_1 Offset(double dx, double dy) => new(X + dx, Y + dy);
    public double DistanceTo(Point_1 other) => 
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    public override string ToString() => $"({X:F2}, {Y:F2})";
}

public interface IGraphicFigure
{
    Color LineColor { get; }
    Color FillColor { get; }
    double Thickness { get; }
}

public interface IDrawFigure {
	
}

public interface IFigure
{
    void Rotate(double angle);
    void Scale(double sx, double sy);
    void RadialScale(double sx);
    void Reflection(Point a, Point b);
    void Move(double dx,double dy);
    bool IsIn(Point point,double eps); 
    Point Center { get; }
    ReadOnlySpan<Point> Vertex { get; }
    bool HasIntersection(Point lefttop,Point rightbottom);
    IFigure Intersection(IFigure figure);
    IEnumerable<IDrawFigure> Draw();
}