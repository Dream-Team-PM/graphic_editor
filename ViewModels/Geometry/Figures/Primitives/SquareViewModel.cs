// ViewModels/Geometry/Figures/Primitives/RectangleViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс квадрата (наследуется от RectangleViewModel).
/// ККвадрат — это прямоугольник с равными сторонами.
/// </summary>
public class SquareViewModel: RectangleViewModel
{
    /// <summary>Конструктор по умолчанию: квадрат в (0,0) с длиной и шириной стороны 100</summary>
    public SquareViewModel(): this(0, 0, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Green, 1.0) {}

    /// <summary>Конструктор с ограничивающим прямоугольником (для drag-отрисовки)</summary>
    public SquareViewModel(double x, double y, double width, double height, 
        System.Drawing.Color lineColor, double thickness, System.Drawing.Color fillColor, double opacity)
        : base(x, y, width, height, lineColor, thickness, fillColor, opacity)
    {
        Name = "Квадрат";
        var maxSide = Math.Max(width, height);
        // Обновляем вершины напрямую
        if (Vertices.Count == 4)
        {
            Vertices[0].X = x; Vertices[0].Y = y;
            Vertices[1].X = x + maxSide; Vertices[1].Y = y;
            Vertices[2].X = x + maxSide; Vertices[2].Y = y + maxSide;
            Vertices[3].X = x; Vertices[3].Y = y + maxSide;
        }
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
        this.RaisePropertyChanged(nameof(Side));
    }
    
    /// <summary>Конструктор квадрата с левым-верхним углом и стороной</summary>
    public SquareViewModel(double x, double y, double side, 
        System.Drawing.Color lineColor, double thickness, System.Drawing.Color fillColor, double opacity)
        : this(x, y, side, side, lineColor, thickness, fillColor, opacity) {}
    
    /// <summary>Длина стороны квадрата</summary>
    public double Side => Math.Max(Width, Height);

    /// <summary>Масштабирование с сохранением квадратной формы</summary>
    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            var scaled = Point2D.ScalePoint(vertex.ToPoint(), center, sx, sy);
            vertex.X = scaled.X;
            vertex.Y = scaled.Y;
        }
    }

    /// <summary>Удобный метод масштабирования с одним коэффициентом</summary>
    public void Scale(double s) => Scale(s, s);
}