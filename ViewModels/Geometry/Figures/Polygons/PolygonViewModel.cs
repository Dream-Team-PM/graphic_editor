// ViewModels/Geometry/Figures/Polygons/PolygonViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;
namespace graphic_editor.Geometry;

/// <summary>
/// Базовый класс для многоугольников.
/// </summary>
public abstract class PolygonViewModel : FigureViewModel
{
    public event EventHandler VerticesChanged;
    /// <summary>Конструктор с набором вершин</summary>
    protected PolygonViewModel(IEnumerable<Point2D> points, 
        Color lineColor, double thickness, Color fillColor, double opacity)
    {
        Name = GetType().Name.Replace("ViewModel", "");
        
        // Конвертируем Point2D → PointViewModel для реактивности
        foreach (var point in points)
            Vertices.Add(new PointViewModel(point.X, point.Y));
        
        LineColor = lineColor;
        Thickness = thickness;
        FillColor = fillColor == default ? Color.Transparent : fillColor;
        Opacity = opacity;
    }

    /// <summary>Центр многоугольника (среднее арифметическое вершин)</summary>
    public override Point2D Center => new Point2D(
        Vertices.Average(v => v.X),
        Vertices.Average(v => v.Y)
    );

    /// <summary>Перемещение всех вершин</summary>
    public override void Move(double dx, double dy)
    {
        foreach (var vertex in Vertices)
        {
            vertex.X += dx;
            vertex.Y += dy;
        }
        NotifyPropertyChanged();
    }

    /// <summary>Поворот вокруг центра</summary>
    public override void Rotate(double angleDegrees)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            var rotated = vertex.ToPoint().Rotate(center, angleDegrees);
            vertex.X = rotated.X;
            vertex.Y = rotated.Y;
        }
        NotifyPropertyChanged();
    }

    /// <summary>Масштабирование относительно центра</summary>
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

    /// <summary>Проверка попадания точки в многоугольник (алгоритм ray casting)</summary>
    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        int count = 0;
        var vertices = Vertices.Select(v => v.ToPoint()).ToList();
        
        for (int i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            
            if (RayIntersectsSegment(point, a, b, eps)) 
                count++;
        }
        
        return (count & 1) == 1;
    }
    
    /// <summary>Проверка пересечения луча с отрезком (для IsIn)</summary>
    private bool RayIntersectsSegment(Point2D p, Point2D a, Point2D b, double eps)
    {
        // Упорядочиваем по Y
        if (a.Y > b.Y) (a, b) = (b, a);
        
        // Избегаем попадания точно в вершину
        if (Math.Abs(p.Y - a.Y) < eps || Math.Abs(p.Y - b.Y) < eps)
            p = new Point2D(p.X, p.Y + eps);
        
        if (p.Y < a.Y || p.Y > b.Y) return false;
        if (p.X > Math.Max(a.X, b.X)) return false;
        
        // Вычисляем X пересечения луча с отрезком
        if (Math.Abs(b.Y - a.Y) < eps) return p.X <= Math.Max(a.X, b.X);
        
        double intersectionX = (p.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y) + a.X;
        
        return p.X < intersectionX;
    }

    /// <summary>Масштабирование вершин относительно центра</summary>
    protected void UpdateVerticesScale(Point2D center, double scale)
    {
        foreach (var vertex in Vertices)
        {
            var dx = vertex.X - center.X;
            var dy = vertex.Y - center.Y;
            vertex.X = center.X + dx * scale;
            vertex.Y = center.Y + dy * scale;
        }
        NotifyPropertyChanged();
    }
    
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }

    /// <summary>Уведомление об изменении геометрии</summary>
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(Center));
        // Уведомляем об изменении каждой вершины
        foreach (var vertex in Vertices)
        {
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
        this.RaisePropertyChanged(nameof(Vertices));
    
        // 🔥 Триггерим событие для перерисовки
        VerticesChanged?.Invoke(this, EventArgs.Empty);
    }
}