// ViewModels/Geometry/Figures/Polygons/PentagonViewModel.cs

using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

public class PentagonViewModel : RegularPolygonViewModel
{
    public PentagonViewModel(Point2D center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 5, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Пятиугольник";
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}