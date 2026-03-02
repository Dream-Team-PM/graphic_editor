// ViewModels/Tools/EllipseStrategy.cs

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;

namespace graphic_editor.Tools;
public class EllipseStrategy : PrimitiveStrategyBase
{
    private readonly StyleSettings _defaultStyle;
    
    public EllipseStrategy(StyleSettings defaultStyle) => _defaultStyle = defaultStyle;
    
    protected override FigureViewModel CreateFigure(double x, double y, double w, double h, StyleSettings style) =>
        new EllipseViewModel(x, y, w, h, style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
    
    protected override void UpdatePreviewVertices(FigureViewModel preview, 
        double x, double y, double width, double height)
    {
        if (preview is not EllipseViewModel ellipse) return;
        
        ellipse.Vertices[0].X = x; ellipse.Vertices[0].Y = y;
        ellipse.Vertices[1].X = x + width; ellipse.Vertices[1].Y = y;
        ellipse.Vertices[2].X = x + width; ellipse.Vertices[2].Y = y + height;
        ellipse.Vertices[3].X = x; ellipse.Vertices[3].Y = y + height;
    }
    
    protected override bool ForceSquare => false;
}