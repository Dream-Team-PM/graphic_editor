// ViewModels/Tools/PolygonStrategy.cs

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;


namespace graphic_editor.Tools;

public abstract class PolygonStrategy : IDrawingStrategy
{
    public bool RequiresDrag => true;
    public bool RequiresMultiClick => false;
    
    protected abstract FigureViewModel CreatePolygon(Point2D center, double radius, StyleSettings style);
    
    public FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style) =>
        CreateFinal(start, current, style);
    
    public void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current)
    {
        if (preview is not RegularPolygonViewModel polygon) return;
        
        var center = new Point2D((start.X + current.X) / 2, (start.Y + current.Y) / 2);
        var radius = Math.Max(Math.Abs(current.X - start.X), Math.Abs(current.Y - start.Y)) / 2;
        
        polygon.UpdateVertices(center, radius);
    }
    
    public FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style)
    {
        var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var radius = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y)) / 2;
        return CreatePolygon(center, radius, style);
    }
}