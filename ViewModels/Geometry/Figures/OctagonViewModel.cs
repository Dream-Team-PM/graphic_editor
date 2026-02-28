using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

public class OctagonViewModel : RegularPolygonViewModel
{
    public OctagonViewModel(Point_1 center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 8, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Восьмиугольник";
    }
    
    public override IEnumerable<Point_1> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}