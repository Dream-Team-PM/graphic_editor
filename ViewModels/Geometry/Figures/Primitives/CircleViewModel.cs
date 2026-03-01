// ViewModels/Geometry/Figures/Primitives/CircleViewModel.cs
using System;
using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс круга (наследуется от EllipseViewModel).
/// Круг — это эллипс с равными радиусами.
/// </summary>
public class CircleViewModel : EllipseViewModel
{
    /// <summary>Конструктор по умолчанию: круг в (0,0) радиусом 50</summary>
    public CircleViewModel() 
        : this(0, 0, 50, Color.Black, 1, Color.Transparent, 1.0) {}

    /// <summary>Конструктор с ограничивающим прямоугольником (для drag-отрисовки)</summary>
    /// <param name="x">Левый-верхний угол X ограничивающего прямоугольника</param>
    /// <param name="y">Левый-верхний угол Y ограничивающего прямоугольника</param>
    /// <param name="width">Ширина ограничивающего прямоугольника</param>
    /// <param name="height">Высота ограничивающего прямоугольника</param>
    public CircleViewModel(double x, double y, double width, double height, 
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(x, y, width, height, lineColor, thickness, fillColor, opacity)
    {
        Name = "Круг";
        var maxRadius = Math.Max(Width, Height) / 2.0;
        base.Scale(maxRadius / RadiusX, maxRadius / RadiusY);
    }
    
    /// <summary>Конструктор круга с центром и радиусом (для программного создания)</summary>
    public CircleViewModel(double centerX, double centerY, double radius, 
        Color lineColor, double thickness, Color fillColor, double opacity)
        : this(centerX - radius, centerY - radius, radius * 2, radius * 2, 
            lineColor, thickness, fillColor, opacity) {}

    /// <summary>Радиус круга (единый для X и Y)</summary>
    public double Radius => Math.Max(RadiusX, RadiusY);
    public double Width => RadiusX * 2;

    /// <summary>Масштабирование с сохранением круглой формы</summary>
    public override void Scale(double sx, double sy)
    {
        // Для круга используем средний коэффициент, чтобы сохранить форму
        var avgScale = (sx + sy) / 2.0;
        base.Scale(avgScale, avgScale);
    }

    /// <summary>Удобный метод масштабирования с одним коэффициентом</summary>
    public void Scale(double s) => Scale(s, s);
    
    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        var dx = point.X - Center.X;
        var dy = point.Y - Center.Y;
        var radius = Radius;
        return (dx * dx + dy * dy) <= radius * radius + eps;
    }
}