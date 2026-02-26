// ViewModels/Geometry/TransformHelpers.cs

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

/// <summary>
/// Общий класс трансформаций для фигур из Geometry (до конца не внедрен - пример есть в LineViewModel).
/// </summary>
public static class TransformHelpers
{
    /// <summary>Публичный статический метод для вращения точки.</summary>
    public static Point_1 RotatePoint(Point_1 point, Point_1 center, double angleDegrees)
    {
        double rad = angleDegrees * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        
        var r = point - center;
        return new Point_1(
            center.X + r.X * cos - r.Y * sin,
            center.Y + r.X * sin + r.Y * cos
        );
    }
    
    /// <summary>Публичный статический метод для масштабирования точки.</summary>
    public static Point_1 ScalePoint(Point_1 point, Point_1 center, double sx, double sy)
    {
        return new Point_1(
            center.X + (point.X - center.X) * sx,
            center.Y + (point.Y - center.Y) * sy
        );
    }
}