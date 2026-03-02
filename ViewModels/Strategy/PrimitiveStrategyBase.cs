// ViewModels/Tools/PrimitiveStrategyBase.cs — УЛУЧШЕННАЯ ВЕРСИЯ

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;

namespace graphic_editor.Tools;

public abstract class PrimitiveStrategyBase : IDrawingStrategy
{
    public bool RequiresDrag => true;
    public bool RequiresMultiClick => false;
    
    // Абстрактные методы для конкретных реализаций
    protected abstract FigureViewModel CreateFigure(
        double x, double y, double width, double height, StyleSettings style);
    
    protected abstract void UpdatePreviewVertices(FigureViewModel preview, 
        double x, double y, double width, double height);
    
    protected abstract bool ForceSquare { get; }
    
    public FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style) =>
        CreateFinal(start, current, style);
    
    public void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current)
    {
        var x = Math.Min(start.X, current.X);
        var y = Math.Min(start.Y, current.Y);
        var width = Math.Abs(current.X - start.X);
        var height = Math.Abs(current.Y - start.Y);
        
        if (ForceSquare)
        {
            var size = Math.Max(width, height);
            UpdatePreviewVertices(preview, x, y, size, size);
        }
        else
        {
            UpdatePreviewVertices(preview, x, y, width, height);
        }
        
        // Уведомляем о изменении свойств (для ReactiveUI)
        NotifyPreviewChanged(preview);
    }
    
    public FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);
        
        if (ForceSquare)
        {
            var size = Math.Max(width, height);
            return CreateFigure(x, y, size, size, style);
        }
        return CreateFigure(x, y, width, height, style);
    }
    
    private void NotifyPreviewChanged(FigureViewModel preview)
    {
        // Универсальное уведомление для ReactiveUI
        // preview.RaisePropertyChanged(nameof(FigureViewModel.Vertices));
        preview.NotifyPropertyChanged();
        if (preview is RectangleViewModel r)
        {
            r.NotifyPropertyChanged();
        }
        else if (preview is EllipseViewModel e)
        {
            e.NotifyPropertyChanged();
        }
    }
}