// ViewModels/Geometry/Figures/Polygons/PentagramViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

public class PentagramViewModel : PolygonViewModel
{
    public double OuterRadius { get; }

    public PentagramViewModel(Point2D center, double outerRadius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(CreateVertices(center, outerRadius), 
            lineColor, thickness, fillColor, opacity)
    {
        Name = "Пентаграмма";
        OuterRadius = outerRadius;
    }

    private static IEnumerable<Point2D> CreateVertices(Point2D center, double R)
    {
        var points = new List<Point2D>();
        double r = R * 0.382; // Внутренний радиус для правильной звезды

        for (int i = 0; i < 10; i++)
        {
            double angle = i * Math.PI / 5;
            double radius = (i % 2 == 0) ? R : r;

            points.Add(new Point2D(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            ));
        }

        return points;
    }
    
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
    
    /// <summary>Пересчёт вершин пентаграммы по новому внешнему радиусу</summary>
    // ViewModels/Geometry/Figures/PentagramViewModel.cs

    /// <summary>Пересчёт вершин пентаграммы по новому внешнему радиусу</summary>
    public void UpdateVertices(Point2D center, double newOuterRadius)
    {
        var points = CreateVertices(center, newOuterRadius).ToList();
    
        for (int i = 0; i < Vertices.Count && i < points.Count; i++)
        {
            Vertices[i].X = points[i].X;
            Vertices[i].Y = points[i].Y;
        }
    
        // Уведомляем об изменении каждой вершины
        foreach (var vertex in Vertices)
        {
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
    
        this.RaisePropertyChanged(nameof(Center));
        this.RaisePropertyChanged(nameof(OuterRadius));
        this.RaisePropertyChanged(nameof(Vertices));
    }

    /// <summary>Уведомление об изменении геометрии</summary>
    protected new void NotifyPropertyChanged()
    {
        base.NotifyPropertyChanged();
        // Дополнительно уведомляем о изменении OuterRadius если нужно
        this.RaisePropertyChanged(nameof(OuterRadius));
    }
}