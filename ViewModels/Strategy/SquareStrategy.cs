// ViewModels/Tools/SquareStrategy.cs

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;

namespace graphic_editor.Tools;
public class SquareStrategy : PrimitiveStrategyBase
{
    protected override FigureViewModel CreateFigure(double x, double y, double w, double h, StyleSettings style) =>
        new SquareViewModel(x, y, w, style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
    
    protected override void UpdatePreviewVertices(FigureViewModel preview, 
        double x, double y, double width, double height)
    {
        // Square наследуется от Rectangle, поэтому та же логика
        if (preview is not RectangleViewModel rect) return;
        
        rect.Vertices[0].X = x; rect.Vertices[0].Y = y;
        rect.Vertices[1].X = x + width; rect.Vertices[1].Y = y;
        rect.Vertices[2].X = x + width; rect.Vertices[2].Y = y + width; // ← width, не height!
        rect.Vertices[3].X = x; rect.Vertices[3].Y = y + width;
    }
    
    protected override bool ForceSquare => true;
}