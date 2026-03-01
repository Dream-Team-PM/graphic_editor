// ViewModels/Geometry/Figures/Polygons/OctagonViewModel.cs

using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

public class OctagonViewModel : RegularPolygonViewModel
{
    public OctagonViewModel(Point2D center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 8, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Восьмиугольник";
    }
    
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}