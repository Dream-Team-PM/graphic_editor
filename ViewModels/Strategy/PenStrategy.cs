// ViewModels/Tools/PenStrategy.cs (multi-click)

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;


namespace graphic_editor.Tools;

public class PenStrategy : IDrawingStrategy
{
    public bool RequiresDrag => false;
    public bool RequiresMultiClick => true;
    
    public FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style) =>
        new PenPointViewModel(current.X, current.Y, style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
    
    public void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current)
    {
        if (preview is PenPointViewModel point)
        {
            point.Vertices[0].X = current.X;
            point.Vertices[0].Y = current.Y;
        }
    }
    
    public FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style) =>
        new PenPointViewModel(end.X, end.Y, style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
}