using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;

namespace graphic_editor.Tools;
public class OctagonStrategy : PolygonStrategy
{
    protected override FigureViewModel CreatePolygon(Point2D center, double radius, StyleSettings style) =>
        new OctagonViewModel(center, radius, style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
}