// ViewModels/Geometry/Figures/Primitives/EllipseViewModel.cs

using System;
using System.Collections.Generic;
using System.Drawing;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс эллипса, заданного ограничивающим прямоугольником.
/// </summary>
public class EllipseViewModel: FigureViewModel
{
    /// <summary>
    /// Конструктор по умолчанию (эллипс 100×100 в начале координат).
    /// </summary>
    public EllipseViewModel(): this(0, 0, 100, 100, Color.Black, 1, Color.Green, 1.0) {}

    /// <summary>
    /// Инициализирует новый экземпляр эллипса.
    /// </summary>
    /// <param name="x">Координата X левого верхнего угла ограничивающего прямоугольника.</param>
    /// <param name="y">Координата Y левого верхнего угла ограничивающего прямоугольника.</param>
    /// <param name="width">Ширина эллипса.</param>
    /// <param name="height">Высота эллипса.</param>
    /// <param name="lineColor">Цвет обводки.</param>
    /// <param name="thickness">Толщина обводки.</param>
    /// <param name="fillColor">Цвет заливки.</param>
    /// <param name="opacity">Непрозрачность (0.0–1.0).</param>
    public EllipseViewModel(double x, double y, double width, double height, Color lineColor, double thickness, Color fillColor, double opacity)
    {
        Name = "Эллипс";
        Vertices.Add(new PointViewModel(x, y));
        Vertices.Add(new PointViewModel(x + width, y));
        Vertices.Add(new PointViewModel(x + width, y + height));
        Vertices.Add(new PointViewModel(x, y + height));
        LineColor = lineColor;
        Thickness = thickness;
        FillColor = fillColor == default ? Color.Transparent : fillColor;
        Opacity = opacity;
    }

    /// <summary>
    /// Координата X левого верхнего угла ограничивающего прямоугольника.
    /// </summary>
    public double X => Vertices[0].X;
    
    /// <summary>
    /// Координата Y левого верхнего угла ограничивающего прямоугольника.
    /// </summary>
    public double Y => Vertices[0].Y;
    
    /// <summary>
    /// Ширина эллипса.
    /// </summary>
    public double Width => Math.Abs(Vertices[2].X - Vertices[0].X);
    
    /// <summary>
    /// Высота эллипса.
    /// </summary>
    public double Height => Math.Abs(Vertices[2].Y - Vertices[0].Y);
    
    /// <summary>
    /// Горизонтальный радиус эллипса.
    /// </summary>
    public double RadiusX => Width / 2;
    
    /// <summary>
    /// Вертикальный радиус эллипса.
    /// </summary>
    public double RadiusY => Height / 2;

    /// <summary>Публичное абстрактное свойство центрирования фигуры (точка вращения/масштабирования).</summary>
    public override Point2D Center => new Point2D(
        Vertices.Average(v => v.X),
        Vertices.Average(v => v.Y)
    );

    /// <summary>
    /// Поворачивает фигуру на заданный угол вокруг центра.
    /// </summary>
    /// <param name="angle">Угол поворота в градусах (положительный = по часовой стрелке).</param>
    public override void Rotate(double angle)
    {
        var center = Center;

        foreach (var vertex in Vertices)
        {
            var rotated = vertex.ToPoint().Rotate(center, angle);
            vertex.X = rotated.X;
            vertex.Y = rotated.Y;
        }
        NotifyPropertyChanged();
    }

    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            var scaled = vertex.ToPoint().Scale(center, sx, sy);
            vertex.X = scaled.X;
            vertex.Y = scaled.Y;
        }
        NotifyPropertyChanged();
    }

    public override void Move(double dx, double dy)
    {
        foreach (var vertex in Vertices)
        {
            vertex.X += dx;
            vertex.Y += dy;
        }
    }

    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        // Проверка попадания точки в эллипс по каноническому уравнению
        var center = Center;
        var dx = (point.X - center.X) / RadiusX;
        var dy = (point.Y - center.Y) / RadiusY;
        return (dx * dx + dy * dy) <= 1 + eps;
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
    
    /// <summary>
    /// Уведомляет об изменении геометрических свойств эллипса.
    /// </summary>
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
        this.RaisePropertyChanged(nameof(RadiusX));
        this.RaisePropertyChanged(nameof(RadiusY));
    }

    /// <summary>
    /// Создает клон фигуры.
    /// </summary>
    public override FigureViewModel Clone()
    {
        var clone = new EllipseViewModel(X, Y, Width, Height, LineColor, Thickness, FillColor, Opacity);
        return clone;
    }
}
