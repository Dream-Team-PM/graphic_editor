// ViewModels/Geometry/TransformHelpers.cs

using ReactiveUI;

namespace graphic_editor.Geometry;

/// <summary>
/// Общий класс трансформаций для фигур из Geometry (до конца не внедрен - пример есть в LineViewModel).
/// </summary>
public static class PointTransformExtensions
{
    /// <summary>Публичный статический метод для вращения точки.</summary>
    public static Point2D Rotate(this Point2D point, Point2D center, double angleDegrees)
    {
        double rad = angleDegrees * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        var r = point - center;
        return new Point2D(
            center.X + r.X * cos - r.Y * sin,
            center.Y + r.X * sin + r.Y * cos
        );
    }
    
    /// <summary>Публичный статический метод для масштабирования точки.</summary>
    public static Point2D Scale(this Point2D point, Point2D center, double sx, double sy)
    {
        return new Point2D(
            center.X + (point.X - center.X) * sx,
            center.Y + (point.Y - center.Y) * sy
        );
    }

    public static Point2D Reflect(this Point2D p, Point2D a, Point2D b)
    {
        var d = b - a;
        var A = d.Y; var B = -d.X; var C = d.X * a.Y - d.Y * a.X;
        var D = (A * p.X + B * p.Y + C) / (A * A + B * B);
        return new Point2D(p.X - 2 * A * D, p.Y - 2 * B * D);
    }
}