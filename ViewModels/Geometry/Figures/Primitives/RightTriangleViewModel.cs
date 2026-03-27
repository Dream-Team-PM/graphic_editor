// ViewModels/Geometry/Figures/Primitives/RightTriangleViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ReactiveUI;
using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс прямоугольного треугольника с прямым углом в левом-нижнем углу.
/// Вершины: (0,0) — прямой угол, (width,0) — правый угол, (0,height) — верхний угол.
/// </summary>
public class RightTriangleViewModel : PolygonViewModel
{
   /// <summary>Конструктор по умолчанию.</summary>
    public RightTriangleViewModel() 
        : this(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1.0) {}

    /// <summary>
    /// Инициализирует прямоугольный треугольник.
    /// </summary>
    public RightTriangleViewModel(
        double x, double y, 
        double width, double height,
        Color lineColor, double thickness, Color fillColor, double opacity = 1.0)
        // ✅ ВЫЗЫВАЕМ БАЗОВЫЙ КОНСТРУКТОР С 3 ВЕРШИНАМИ
        : base(CreateRightTrianglePoints(x, y, width, height), 
               lineColor, thickness, fillColor, opacity)
    {
        Name = "Прямоугольный треугольник";
    }

    /// <summary>
    /// Вспомогательный метод для создания 3 вершин прямоугольного треугольника.
    /// </summary>
    private static IEnumerable<Point2D> CreateRightTrianglePoints(
        double x, double y, double width, double height)
    {
        // Прямоугольный треугольник: 3 вершины
        // Вершина 0: прямой угол (левый-нижний)
        yield return new Point2D(x, y);
        // Вершина 1: правый угол
        yield return new Point2D(x + width, y);
        // Вершина 2: верхний угол
        yield return new Point2D(x, y + height);
    }

    /// <summary>Обновляет вершины треугольника.</summary>
    public void UpdateVertices(double x, double y, double width, double height)
    {
        while (Vertices.Count < 3)
            Vertices.Add(new PointViewModel());
        
        // 🔺 Прямоугольный треугольник: 3 вершины
        Vertices[0].X = x;              Vertices[0].Y = y;              // Прямой угол (левый-нижний)
        Vertices[1].X = x + width;      Vertices[1].Y = y;              // Правый угол
        Vertices[2].X = x;              Vertices[2].Y = y + height;     // Верхний угол
    }

    public double X => Vertices[0].X;
    public double Y => Vertices[0].Y;
    public double Width => Math.Abs(Vertices[1].X - Vertices[0].X);
    public double Height => Math.Abs(Vertices[2].Y - Vertices[0].Y);

    /// <summary>Центроид треугольника (среднее арифметическое вершин).</summary>
    public override Point2D Center => new Point2D(
        (Vertices[0].X + Vertices[1].X + Vertices[2].X) / 3,
        (Vertices[0].Y + Vertices[1].Y + Vertices[2].Y) / 3
    );

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
		NotifyPropertyChanged();
    }

    /// <summary>
    /// Проверка попадания точки через барицентрические координаты.
    /// </summary>
    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        var A = Vertices[0].ToPoint();
        var B = Vertices[1].ToPoint();
        var C = Vertices[2].ToPoint();
        
        var denom = ((B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y));
        if (Math.Abs(denom) < eps) return false;
        
        var a = ((B.Y - C.Y) * (point.X - C.X) + (C.X - B.X) * (point.Y - C.Y)) / denom;
        var b = ((C.Y - A.Y) * (point.X - C.X) + (A.X - C.X) * (point.Y - C.Y)) / denom;
        var c = 1 - a - b;
        
        return a >= -eps && b >= -eps && c >= -eps && 
               a <= 1 + eps && b <= 1 + eps && c <= 1 + eps;
    }

    public override IEnumerable<Point2D> GetVertexPoint() =>
        Vertices.Take(3).Select(v => v.ToPoint());

    public override FigureViewModel Clone() =>
        new RightTriangleViewModel(X, Y, Width, Height, LineColor, Thickness, FillColor, Opacity)
        {
            IsSelected = IsSelected,
            Rotation = Rotation
        };

    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
        this.RaisePropertyChanged(nameof(Center));
        this.RaisePropertyChanged(nameof(Vertices));
    }
}