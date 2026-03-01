// ViewModels/Geometry/Figures/Polygons/HeptagonViewModel.cs

using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

public class HeptagonViewModel : RegularPolygonViewModel
{
    public HeptagonViewModel(Point2D center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 7, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Семиугольник";
    }
    
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}