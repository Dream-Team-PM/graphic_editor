// ViewModels/Geometry/Figures/Primitives/PenPointViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс точки, созданной инструментом "Перо".
/// Представляет собой минимальный графический примитив для свободного рисования.
/// </summary>
public class PenPointViewModel : FigureViewModel
{
    /// <summary>
    /// Конструктор по умолчанию (точка в (100, 100)).
    /// </summary>
    public PenPointViewModel(): this(100, 100, Color.Black, 1, Color.Transparent, 1.0) {}
    
    /// <summary>
    /// Инициализирует новую точку пера.
    /// </summary>
    /// <param name="x">Координата X точки.</param>
    /// <param name="y">Координата Y точки.</param>
    /// <param name="lineColor">Цвет точки (обводки).</param>
    /// <param name="thickness">Размер точки (влияет на радиус отображения).</param>
    /// <param name="fillColor">Цвет заливки (не используется, всегда Transparent).</param>
    /// <param name="opacity">Непрозрачность (0.0–1.0).</param>
    public PenPointViewModel(double x, double y, Color lineColor, double thickness, Color fillColor, double opacity = 1.0)
    {
        Name = "Точка пера";
        Vertices.Add(new PointViewModel(x, y));
        LineColor = lineColor;
        Thickness = thickness;
        FillColor = Color.Transparent;
        Opacity = opacity;
    }

    /// <summary>
    /// Координата X точки.
    /// </summary>
    public double X => Vertices[0].X;
    
    /// <summary>
    /// Координата Y точки.
    /// </summary>
    public double Y => Vertices[0].Y;

    public override Point2D Center => new Point2D(X, Y);

    public override void Rotate(double angle) 
    {
        // Точка не имеет ориентации, вращение не влияет на геометрию
        NotifyPropertyChanged(); 
    }

    public override void Scale(double sx, double sy)
    {
        // Масштабирование точки не меняет её позиции, только размер отображения
        NotifyPropertyChanged(); 
    }
    
    public override void Move(double dx, double dy)
    {
        Vertices[0].X += dx;
        Vertices[0].Y += dy;
    }

    public override bool IsIn(Point2D point, double eps = 5.0)
    {
        // Проверяем попадание в круг радиусом eps вокруг точки
        var dx = point.X - X;
        var dy = point.Y - Y;
        return (dx * dx + dy * dy) <= (eps * eps);
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        yield return new Point2D(X, Y);
    }
    
    /// <summary>
    /// Уведомляет об изменении координат точки.
    /// </summary>
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }
}