// ViewModels/Geometry/Figures/Polygons/PentagonViewModel.cs

using System.Drawing;
using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс правильного пятиугольника.
/// </summary>
public class PentagonViewModel : RegularPolygonViewModel
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
    public PentagonViewModel(Point2D center, double radius,
        System.Drawing.Color lineColor, double thickness, System.Drawing.Color fillColor, double opacity)
        : base(center, 5, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Пятиугольник";
    }

    /// <summary>
    /// Создает клон фигуры.
    /// </summary>
    public override FigureViewModel Clone()
    {
        var clone = new PentagonViewModel(new Point2D(Center.X, Center.Y), Radius, LineColor, Thickness, FillColor, Opacity);
        return clone;
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}