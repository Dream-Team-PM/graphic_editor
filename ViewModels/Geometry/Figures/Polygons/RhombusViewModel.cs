// ViewModels/Geometry/Figures/Primitives/RhombusViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ReactiveUI;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using graphic_editor.Geometry;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс ромба — четырёхугольник с равными сторонами.
/// Вершины: верх, право, низ, лево относительно центра.
/// </summary>
public class RhombusViewModel : PolygonViewModel
{
    /// <summary>
    /// Конструктор по умолчанию (ромб 100×100 в точке (100,100)).
    /// </summary>
    public RhombusViewModel() 
        : this(100, 100, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1.0) {}

    /// <summary>
    /// Инициализирует новый ромб.
    /// </summary>
    public RhombusViewModel(
        double centerX, double centerY, 
        double width, double height,
        System.Drawing.Color lineColor, double thickness, System.Drawing.Color fillColor, double opacity = 1.0)
        // ✅ ВЫЗЫВАЕМ БАЗОВЫЙ КОНСТРУКТОР С 4 ВЕРШИНАМИ
        : base(CreateRhombusPoints(centerX, centerY, width, height), 
               lineColor, thickness, fillColor, opacity)
    {
        Name = "Ромб";
    }

    /// <summary>
    /// Вспомогательный метод для создания 4 вершин ромба.
    /// </summary>
    private static IEnumerable<Point2D> CreateRhombusPoints(
        double centerX, double centerY, double width, double height)
    {
        var halfW = width / 2;
        var halfH = height / 2;
        
        // Возвращаем 4 вершины: верх, право, низ, лево (по часовой стрелке)
        yield return new Point2D(centerX, centerY - halfH); // Верх
        yield return new Point2D(centerX + halfW, centerY); // Право
        yield return new Point2D(centerX, centerY + halfH); // Низ
        yield return new Point2D(centerX - halfW, centerY); // Лево
    }

    /// <summary>Координата X центра ромба.</summary>
    public double CenterX => (Vertices[0].X + Vertices[2].X) / 2;
    
    /// <summary>Координата Y центра ромба.</summary>
    public double CenterY => (Vertices[1].Y + Vertices[3].Y) / 2;
    
    /// <summary>Ширина ромба (горизонтальная диагональ).</summary>
    public double Width => Math.Abs(Vertices[1].X - Vertices[3].X);
    
    /// <summary>Высота ромба (вертикальная диагональ).</summary>
    public double Height => Math.Abs(Vertices[0].Y - Vertices[2].Y);
    
    /// <summary>Полуширина (радиус по X).</summary>
    public double RadiusX => Width / 2;
    
    /// <summary>Полувысота (радиус по Y).</summary>
    public double RadiusY => Height / 2;

    /// <summary>
    /// Центральная точка ромба.
    /// </summary>
    public override Point2D Center => new Point2D(CenterX, CenterY);
    
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

    /// <summary>
    /// Перемещает ромб на заданный вектор.
    /// </summary>
    public override void Move(double dx, double dy)
    {
        foreach (var vertex in Vertices)
        {
            vertex.X += dx;
            vertex.Y += dy;
        }
        NotifyPropertyChanged();
    }

    /// <summary>
    /// Проверяет попадание точки в ромб (алгоритм: |dx/rx| + |dy/ry| <= 1).
    /// </summary>
    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        var center = Center;
        var dx = Math.Abs(point.X - center.X) / RadiusX;
        var dy = Math.Abs(point.Y - center.Y) / RadiusY;
        return (dx + dy) <= 1.0 + eps;
    }

    /// <summary>
    /// Создаёт глубокую копию ромба.
    /// </summary>
    public override FigureViewModel Clone()
    {
        return new RhombusViewModel(
            CenterX, CenterY, Width, Height,
            LineColor, Thickness, FillColor, Opacity)
        {
            IsSelected = IsSelected,
            Rotation = Rotation
        };
    }

    /// <summary>
    /// Уведомляет об изменении геометрических свойств.
    /// </summary>
    private void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(CenterX));
        this.RaisePropertyChanged(nameof(CenterY));
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
        this.RaisePropertyChanged(nameof(Center));
    }
}