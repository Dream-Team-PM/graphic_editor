// ViewModels/Geometry/Point2D.cs

namespace graphic_editor.Geometry;

/// <summary>
/// Публичная структура точки с операторами.
/// </summary>
public record Point2D(double X, double Y)
{
    public static Point2D Zero => new(0, 0); /// <summary>Инициализация нулевой точки.</summary>
    
	/// <summary>Оператор + для точки.</summary>
    public static Point2D operator +(Point2D left, Point2D right) => 
        new(left.X + right.X, left.Y + right.Y);
	/// <summary>Оператор - для точки.</summary>
    public static Point2D operator -(Point2D left, Point2D right) => 
        new(left.X - right.X, left.Y - right.Y);
    /// <summary>Оператор * для точки.</summary>
    public static Point2D operator *(Point2D p, double scale) => 
        new(p.X * scale, p.Y * scale);
    public static Point2D operator *(double scale, Point2D p) => 
        new(p.X * scale, p.Y * scale);
    /// <summary>Оператор / для точки.</summary>
    public static Point2D operator /(Point2D p, double scale) => 
        new(p.X / scale, p.Y / scale);
    
    // === Методы экземпляра ===
    /// <summary>Смещение точки по dx/dy.</summary>
    public Point2D Offset(double dx, double dy) => new(X + dx, Y + dy);
    
    /// <summary>Публичный метод нахождения расстояния до точки.</summary>
    public double DistanceTo(Point2D other) => 
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    
    /// <summary>Публичный метод нахождения расстояния до точки (без использования корня).</summary>
    public double DistanceToSq(Point2D other) => 
        Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2);
    
    /// <summary>Публичный статический метод масштабирования точки.</summary>
    public static Point2D ScalePoint(Point2D p, Point2D center, double sx, double sy)
    {
        return new Point2D(
            center.X + (p.X - center.X) * sx,
            center.Y + (p.Y - center.Y) * sy
        );
    }
    
    /// <summary>Публичный статический метод нахождения расстояния от точки до сегмента.</summary>
    public static double DistancePointToSegment(Point2D p, Point2D a, Point2D b)
    {
        var d = b - a;
        if (d.X == 0 && d.Y == 0)
            return p.DistanceTo(a);
            
        double t = Math.Max(0, Math.Min(1, 
            ((p.X - a.X) * d.X + (p.Y - a.Y) * d.Y) / (d.X * d.X + d.Y * d.Y)));
            
        var proj = new Point2D(a.X + t * d.X, a.Y + t * d.Y);
        return p.DistanceTo(proj);
    }
    
    
    /// <summary>Публичный статический метод нахождения расстояния от точки до сегмента (без использования корня).</summary>
    public static double DistanceToSegmentSq(Point2D p, Point2D a, Point2D b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lengthSq = dx * dx + dy * dy;
        
        if (lengthSq < 1e-10) // отрезок вырожден в точку
            return p.DistanceToSq(a);
        // Проекция точки на прямую отрезка
        var t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq;
        t = Math.Max(0, Math.Min(1, t));
        
        var closestX = a.X + t * dx;
        var closestY = a.Y + t * dy;
        
        return Math.Pow(p.X - closestX, 2) + Math.Pow(p.Y - closestY, 2);
    }
    
    /// <summary>Публичный метод доступности точки возле сегмента.</summary>
    public static bool IsPointNearSegment(Point2D p, Point2D a, Point2D b, double eps)
    {
        return DistanceToSegmentSq(p, a, b) <= eps * eps;
    }
    /// <summary>Публичный метод пприведения точки к строке.</summary>
    public override string ToString() => $"({X:F6}, {Y:F6})";
}