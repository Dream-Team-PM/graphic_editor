// ViewModels/Tools/PentagramStrategy.cs
using graphic_editor.Geometry;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using graphic_editor.State;
using graphic_editor.Interfaces;

namespace graphic_editor.Tools;

/// <summary>Стратегия для рисования пентаграммы (звезды) через центр и радиус.</summary>
public class PentagramStrategy : IDrawingStrategy
{
    public bool RequiresDrag => true;
    public bool RequiresMultiClick => false;
    
    public FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style) =>
        CreateFinal(start, current, style);
    
    public void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current)
    {
        if (preview is not PentagramViewModel star) return;
        
        var center = new Point2D((start.X + current.X) / 2, (start.Y + current.Y) / 2);
        var radius = Math.Max(Math.Abs(current.X - start.X), Math.Abs(current.Y - start.Y)) / 2;
        
        star.UpdateVertices(center, radius);
    }
    
    public FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style)
    {
        var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var radius = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y)) / 2;
        
        return new PentagramViewModel(
            center, radius,
            style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
    }
}