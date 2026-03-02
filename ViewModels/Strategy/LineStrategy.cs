// ViewModels/Tools/LineStrategy.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;

using System.Drawing;

namespace graphic_editor.Tools;

public class LineStrategy : IDrawingStrategy
{
    public bool RequiresDrag => true;
    public bool RequiresMultiClick => false;
    
    public FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style) =>
        CreateFinal(start, current, style);
    
    public void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current)
    {
        if (preview is not LineViewModel line) return;
        
        // Обновляем вторую точку линии
        line.Vertices[1].X = current.X;
        line.Vertices[1].Y = current.Y;
        
        line.NotifyPropertyChanged();
    }
    
    public FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style) =>
        new LineViewModel(
            start.X, start.Y, end.X, end.Y,
            style.StrokeColor, style.StrokeWidth, Color.Transparent, style.Opacity);
}