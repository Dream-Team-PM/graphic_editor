namespace graphic_editor.Models;

/// <summary>
/// Публичная структура точки с операторами.
/// </summary>
public record Point_1(double X, double Y)
{
    public static Point_1 Zero => new(0, 0); /// <summary>Инициализация нулевой точки.</summary>
    
	/// <summary>Оператор + для точки.</summary>
    public static Point_1 operator +(Point_1 left, Point_1 right) => 
        new(left.X + right.X, left.Y + right.Y);
	/// <summary>Оператор - для точки.</summary>
    public static Point_1 operator -(Point_1 left, Point_1 right) => 
        new(left.X - right.X, left.Y - right.Y);
    /// <summary>Оператор * для точки.</summary>
    public static Point_1 operator *(Point_1 p, double scale) => 
        new(p.X * scale, p.Y * scale);
    public static Point_1 operator *(double scale, Point_1 p) => 
        new(p.X * scale, p.Y * scale);
    /// <summary>Оператор / для точки.</summary>
    public static Point_1 operator /(Point_1 p, double scale) => 
        new(p.X / scale, p.Y / scale);
    
    /// <summary>Смещение точки по dx/dy.</summary>
    public Point_1 Offset(double dx, double dy) => new(X + dx, Y + dy);
    
    /// <summary>Публичный метод нахождения расстояния до точки.</summary>
    public double DistanceTo(Point_1 other) => 
        Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    
    /// <summary>Публичный статический метод масштабирования точки.</summary>
    public static Point_1 ScalePoint(Point_1 p, Point_1 center, double sx, double sy)
    {
        return new Point_1(
            center.X + (p.X - center.X) * sx,
            center.Y + (p.Y - center.Y) * sy
        );
    }
    
    /// <summary>Публичный статический метод нахождения расстояния от точки до сегмента.</summary>
    public static double DistancePointToSegment(Point_1 p, Point_1 a, Point_1 b)
    {
        var d = b - a;
        if (d.X == 0 && d.Y == 0)
            return p.DistanceTo(a);
            
        double t = Math.Max(0, Math.Min(1, 
            ((p.X - a.X) * d.X + (p.Y - a.Y) * d.Y) / (d.X * d.X + d.Y * d.Y)));
            
        var proj = new Point_1(a.X + t * d.X, a.Y + t * d.Y);
        return p.DistanceTo(proj);
    }
    
    /// <summary>Публичный метод нахождения расстояния до точки (без использования корня).</summary>
    public double DistanceToSq(Point_1 other) => 
        Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2);
    
    /// <summary>Публичный статический метод нахождения расстояния от точки до сегмента (без использования корня).</summary>
    public static double DistanceToSegmentSq(Point_1 p, Point_1 a, Point_1 b)
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
    public static bool IsPointNearSegment(Point_1 p, Point_1 a, Point_1 b, double eps)
    {
        return DistanceToSegmentSq(p, a, b) <= eps * eps;
    }
    /// <summary>Публичный метод пприведения точки к строке.</summary>
    public override string ToString() => $"({X:F15}, {Y:F15})";
}