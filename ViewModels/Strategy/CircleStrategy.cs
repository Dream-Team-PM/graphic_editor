// ViewModels/Tools/CircleStrategy.cs

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;

namespace graphic_editor.Tools;
public class CircleStrategy : PrimitiveStrategyBase
{
    private readonly StyleSettings _defaultStyle;
    
    public CircleStrategy(StyleSettings defaultStyle) => _defaultStyle = defaultStyle;
    
    protected override FigureViewModel CreateFigure(double x, double y, double w, double h, StyleSettings style) =>
        new CircleViewModel(x, y, w, h, style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
    
    protected override void UpdatePreviewVertices(FigureViewModel preview, 
        double x, double y, double width, double height)
    {
        // Circle наследуется от Ellipse
        if (preview is not EllipseViewModel ellipse) return;
        
        var size = Math.Max(width, height);
        ellipse.Vertices[0].X = x; ellipse.Vertices[0].Y = y;
        ellipse.Vertices[1].X = x + size; ellipse.Vertices[1].Y = y;
        ellipse.Vertices[2].X = x + size; ellipse.Vertices[2].Y = y + size;
        ellipse.Vertices[3].X = x; ellipse.Vertices[3].Y = y + size;
    }
    
    protected override bool ForceSquare => true;
}