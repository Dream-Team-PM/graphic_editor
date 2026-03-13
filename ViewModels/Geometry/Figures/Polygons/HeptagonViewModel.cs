// ViewModels/Geometry/Figures/Polygons/HeptagonViewModel.cs

using System.Drawing;
using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс правильного семиугольника.
/// </summary>
public class HeptagonViewModel : RegularPolygonViewModel
{
    /// <summary>
    /// Инициализирует новый экземпляр шестиугольника.
    /// </summary>
    /// <param name="center">Центр шестиугольника.</param>
    /// <param name="radius">Радиус описанной окружности.</param>
    /// <param name="lineColor">Цвет обводки.</param>
    /// <param name="thickness">Толщина обводки.</param>
    /// <param name="fillColor">Цвет заливки.</param>
    /// <param name="opacity">Непрозрачность (0.0–1.0).</param>
    public HeptagonViewModel(Point2D center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 7, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Семиугольник";
    }

    /// <summary>
    /// Создает клон фигуры.
    /// </summary>
    public override FigureViewModel Clone()
    {
        var clone = new HeptagonViewModel(new Point2D(Center.X, Center.Y), Radius, LineColor, Thickness, FillColor, Opacity);
        return clone;
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}