// ViewModels/Geometry/Figures/TriangleViewModel.cs
using System.Collections.Generic;
using System.Drawing;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using ReactiveUI;

namespace graphic_editor.Geometry;

public class TriangleViewModel : PolygonViewModel
{
    public TriangleViewModel(Point_1 a, Point_1 b, Point_1 c,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(new List<Point_1> { a, b, c }, 
            lineColor, thickness, fillColor, opacity)
    {
        Name = "Треугольник";
    }
    
    public override IEnumerable<Point_1> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
    
    /// <summary>Масштабирование треугольника относительно центра</summary>
    public void UpdateVertices(Point_1 center, double newRadius)
    {
        // Для треугольника просто масштабируем относительно центра
        var scale = newRadius / 50; // 50 - начальный радиус
    
        for (int i = 0; i < Vertices.Count; i++)
        {
            var dx = Vertices[i].X - center.X;
            var dy = Vertices[i].Y - center.Y;
            Vertices[i].X = center.X + dx * scale;
            Vertices[i].Y = center.Y + dy * scale;
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