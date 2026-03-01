// ViewModels/Geometry/Figures/Polygons/RegularPolygonViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using ReactiveUI;

namespace graphic_editor.Geometry;

/// <summary>
/// Базовый класс для правильных многоугольников.
/// </summary>
public abstract class RegularPolygonViewModel : PolygonViewModel
{
    public int Sides { get; }
    public double Radius { get; }

    protected RegularPolygonViewModel(Point2D center, int sides, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(CreateVertices(center, sides, radius), 
            lineColor, thickness, fillColor, opacity)
    {
        if (sides < 3)
            throw new ArgumentException("Polygon must have at least 3 sides.");
        
        Sides = sides;
        Radius = radius;
    }

    /// <summary>Создание вершин правильного многоугольника</summary>
    private static IEnumerable<Point2D> CreateVertices(Point2D center, int sides, double radius)
    {
        var points = new List<Point2D>();
        double angleStep = 2 * Math.PI / sides;
        double startAngle = -Math.PI / 2; // Начинаем с верха

        for (int i = 0; i < sides; i++)
        {
            double angle = startAngle + i * angleStep;
            points.Add(new Point2D(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            ));
        }

        return points;
    }
    
    /// <summary>Пересчёт вершин правильного многоугольника по новому радиусу</summary>
    // ViewModels/Geometry/Figures/RegularPolygonViewModel.cs

    /// <summary>Пересчёт вершин правильного многоугольника по новому радиусу</summary>
    public void UpdateVertices(Point2D center, double newRadius)
    {
        double angleStep = 2 * Math.PI / Sides;
        double startAngle = -Math.PI / 2; // Начинаем с верха

        for (int i = 0; i < Vertices.Count && i < Sides; i++)
        {
            double angle = startAngle + i * angleStep;
            Vertices[i].X = center.X + newRadius * Math.Cos(angle);
            Vertices[i].Y = center.Y + newRadius * Math.Sin(angle);
        }
    
        // Уведомляем об изменении каждой вершины
        foreach (var vertex in Vertices)
        {
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
    
        this.RaisePropertyChanged(nameof(Center));
        this.RaisePropertyChanged(nameof(Vertices));
    }
}