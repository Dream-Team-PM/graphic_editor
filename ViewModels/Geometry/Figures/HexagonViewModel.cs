using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

public class HexagonViewModel : RegularPolygonViewModel
{
    public HexagonViewModel(Point_1 center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 6, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Шестиугольник";
    }
    
    public override IEnumerable<Point_1> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}