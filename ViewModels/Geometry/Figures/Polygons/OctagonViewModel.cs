// ViewModels/Geometry/Figures/Polygons/OctagonViewModel.cs

using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс правильного восьмиугольника.
/// </summary>
public class OctagonViewModel : RegularPolygonViewModel
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
    public OctagonViewModel(Point2D center, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(center, 8, radius, lineColor, thickness, fillColor, opacity)
    {
        Name = "Восьмиугольник";
    }
    
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}