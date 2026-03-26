// ViewModels/Geometry/TransformHelpers.cs

using ReactiveUI;
using System.Reflection.Metadata.Ecma335;

namespace graphic_editor.Geometry;

/// <summary>
/// Статический класс с методами расширения для геометрических трансформаций точек.
/// Предоставляет утилиты для вращения, масштабирования и отражения Point2D.
/// Используется в реализациях интерфейса ITransformable для фигур.
/// </summary>
public static class PointTransformExtensions
{
    /// <summary>
    /// Вращает точку вокруг заданного центра на указанный угол.
    /// </summary>
    /// <param name="point">Исходная точка для вращения.</param>
    /// <param name="center">Центр вращения.</param>
    /// <param name="angleDegrees">Угол вращения в градусах (положительный — по часовой стрелке).</param>
    /// <returns>Новая точка Point2D с применённым вращением.</returns>
    public static Point2D Rotate(this Point2D point, Point2D center, double angleDegrees)
    {
        double rad = angleDegrees * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        var r = point - center;
        
        Point2D rfl = new Point2D(
            center.X + r.X * cos - r.Y * sin,
            center.Y + r.X * sin + r.Y * cos
        );
        Console.WriteLine(point);
        Console.WriteLine(rfl);
        Console.WriteLine(center);
        return rfl;
    }
    
    /// <summary>
    /// Масштабирует точку относительно центра с заданными коэффициентами по осям.
    /// </summary>
    /// <param name="point">Исходная точка для масштабирования.</param>
    /// <param name="center">Центр масштабирования.</param>
    /// <param name="sx">Коэффициент масштабирования по оси X.</param>
    /// <param name="sy">Коэффициент масштабирования по оси Y.</param>
    /// <returns>Новая точка Point2D с применённым масштабированием.</returns>
    public static Point2D Scale(this Point2D point, Point2D center, double sx, double sy)
    {
        return new Point2D(
            center.X + (point.X - center.X) * sx,
            center.Y + (point.Y - center.Y) * sy
        );
    }

    /// <summary>
    /// Выполняет отражение точки относительно прямой, заданной двумя точками.
    /// Использует формулу проекции точки на прямую для вычисления отражения.
    /// </summary>
    /// <param name="p">Отражаемая точка.</param>
    /// <param name="a">Первая точка, определяющая ось отражения.</param>
    /// <param name="b">Вторая точка, определяющая ось отражения.</param>
    /// <returns>Отражённая точка Point2D.</returns>
    public static Point2D Reflect(this Point2D p, Point2D a, Point2D b)
    {
        var d = b - a;
        var A = d.Y; var B = -d.X; var C = d.X * a.Y - d.Y * a.X;
        var D = (A * p.X + B * p.Y + C) / (A * A + B * B);
        return new Point2D(p.X - 2 * A * D, p.Y - 2 * B * D);
    }
}