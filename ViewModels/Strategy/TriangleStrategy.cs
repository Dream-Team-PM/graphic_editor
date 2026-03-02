// ViewModels/Tools/TriangleStrategy.cs
using graphic_editor.Geometry;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using graphic_editor.State;
using graphic_editor.Interfaces;
using graphic_editor.Helpers;

namespace graphic_editor.Tools;

/// <summary>Стратегия для рисования произвольного треугольника через bounding box.</summary>
public class TriangleStrategy : IDrawingStrategy
{
    public bool RequiresDrag => true;
    public bool RequiresMultiClick => false;
    
    public FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style) =>
        CreateFinal(start, current, style);
    
    public void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current)
    {
        if (preview is not TriangleViewModel triangle) return;
        
        UpdateTriangleBoundingBox(triangle, start, current);
        
        triangle.NotifyPropertyChanged();
    }
    
    public FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style)
    {
        // Создаём треугольник с начальными вершинами (равносторонний, радиус 50)
        var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var initialRadius = 50.0;
        
        var a = new Point2D(center.X, center.Y - initialRadius);                    // Верх
        var b = new Point2D(center.X - initialRadius, center.Y + initialRadius);    // Лево-низ
        var c = new Point2D(center.X + initialRadius, center.Y + initialRadius);    // Право-низ
        
        var triangle = new TriangleViewModel(a, b, c, 
            style.StrokeColor, style.StrokeWidth, style.FillColor, style.Opacity);
        
        // Сразу масштабируем под целевой bounding box
        UpdateTriangleBoundingBox(triangle, start, end);
        
        return triangle;
    }
    
    /// <summary>Масштабирует треугольник под целевой bounding box (аналог UpdatePolygonBoundingBox).</summary>
    // ViewModels/Tools/TriangleStrategy.cs
private void UpdateTriangleBoundingBox(TriangleViewModel triangle, Point2D start, Point2D end)
{
    // 1. Вычисляем текущий bounding box
    var minX = triangle.Vertices.Min(v => v.X);
    var maxX = triangle.Vertices.Max(v => v.X);
    var minY = triangle.Vertices.Min(v => v.Y);
    var maxY = triangle.Vertices.Max(v => v.Y);
    
    // 2. Защита от нулевого размера
    var origWidth = Math.Max(maxX - minX, 1.0);
    var origHeight = Math.Max(maxY - minY, 1.0);
    
    // 3. Целевые размеры
    var targetWidth = Math.Abs(end.X - start.X);
    var targetHeight = Math.Abs(end.Y - start.Y);
    
    // 4. Коэффициент масштабирования
    var scaleX = targetWidth / origWidth;
    var scaleY = targetHeight / origHeight;
    var scale = Math.Max(scaleX, scaleY);
    
    // ✅ 5. Защита от слишком маленького масштаба
    if (scale < 0.01)
    {
        DebugLog.Write($"[WARN] Triangle scale too small: {scale}");
        return;
    }
    
    // 6. Центры
    var origCenter = new Point2D((minX + maxX) / 2, (minY + maxY) / 2);
    var targetCenter = new Point2D(
        Math.Min(start.X, end.X) + targetWidth / 2,
        Math.Min(start.Y, end.Y) + targetHeight / 2);
    
    // 7. Применяем трансформацию
    foreach (var vertex in triangle.Vertices)
    {
        var dx = vertex.X - origCenter.X;
        var dy = vertex.Y - origCenter.Y;
        
        vertex.X = targetCenter.X + dx * scale;
        vertex.Y = targetCenter.Y + dy * scale;
        
        // ✅ 8. Уведомляем через public метод
        vertex.NotifyPropertyChanged();
    }
    
    // ✅ 9. Уведомляем треугольник
    triangle.NotifyPropertyChanged();
}
}