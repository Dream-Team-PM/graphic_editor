using Avalonia.Media;
namespace graphic_editor;
public interface IFigure
{
    void Move(double dx, double dy);
    void Rotate(double angleDegrees);

    void Scale(double sx, double sy);
    void Scale(double factor);   // radial
    bool IsIn(Point p);

    bool HasIntersection(Point lefttop,Point rightbottom);
    IFigure Intersection(IFigure figure);
    IEnumerable<IDrawFigure> Draw();
    Geometry ToGeometry();
}