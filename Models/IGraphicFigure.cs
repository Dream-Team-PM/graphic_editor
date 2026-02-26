using System.Drawing;

namespace graphic_editor.Models;

/// <summary>
/// Публичная структура точки с операторами.
/// </summary>
public record Point_1(double X, double Y)
{
    public static Point_1 Zero => new(0, 0); /// <summary>Инициализация нулевой точки.</summary>
    
    public static Point_1 operator +(Point_1 left, Point_1 right) => 
        new(left.X + right.X, left.Y + right.Y);

    public static Point_1 operator -(Point_1 left, Point_1 right) => 
        new(left.X - right.X, left.Y - right.Y);
    
    public static Point_1 operator *(Point_1 p, double scale) => 
        new(p.X * scale, p.Y * scale);
    
    public static Point_1 operator *(double scale, Point_1 p) => 
        new(p.X * scale, p.Y * scale);
    
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
    public override string ToString() => $"({X:F2}, {Y:F2})";
}

/// <summary>
/// Публичный интерфейс графической фигуры.
/// </summary>
public interface IGraphicFigure
{
    Color LineColor { get; } /// <summary>Свойство цвета для линии.</summary>
    Color FillColor { get; } /// <summary>Свойство цвета для заполнения фигуры.</summary>
    double Thickness { get; } /// <summary>Свойство толщины линии.</summary>
}

/// <summary>
/// Публичный интерфейс отрисовки фигуры (не реализован и пока не используется).
/// </summary>
public interface IDrawFigure {
	
}

/// <summary>
/// Публичный интерфейс фигуры.
/// </summary>
public interface IFigure
{
    void Rotate(double angle); /// <summary>Функция вращения фигуры на определённый угол.</summary>
    void Scale(double sx, double sy); /// <summary>Функция мастабирования фигуры.</summary>
    void RadialScale(double sx); /// <summary>Функция радиального мастабирования фигуры.</summary>
    void Reflection(Point a, Point b); /// <summary>Функция рефлексирования фигуры.</summary>
    void Move(double dx,double dy); /// <summary>Функция перемещения фигуры.</summary>
    bool IsIn(Point point,double eps); /// <summary>Функция проверки нахождения в фигуре с заданной точностью.</summary>
    Point Center { get; } /// <summary>Метод центрирования фигуры.</summary>
    ReadOnlySpan<Point> Vertex { get; } /// <summary>Точка-вершина.</summary>
    bool HasIntersection(Point lefttop,Point rightbottom); /// <summary>Функция проверки пересечения фигур.</summary>
    IFigure Intersection(IFigure figure); /// <summary>Пересечение фигур.</summary>
    IEnumerable<IDrawFigure> Draw(); /// <summary>Отрисовка фигуры.</summary>
}