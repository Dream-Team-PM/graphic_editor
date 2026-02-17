using System.Drawing;
namespace graphic_editor;

public record Point(double X, double Y);
public interface IGraphicFigure
{
    Color Linecolor { get; }
    Color Fillcolor { get; }
    double Thickness { get; }
}

public interface IDrawFigure { }
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