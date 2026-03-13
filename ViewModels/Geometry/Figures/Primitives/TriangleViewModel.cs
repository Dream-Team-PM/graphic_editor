// ViewModels/Geometry/Figures/Primitives/TriangleViewModel.cs
using System.Collections.Generic;
using System.Drawing;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using ReactiveUI;

namespace graphic_editor.Geometry;

public class TriangleViewModel : PolygonViewModel
{
    public TriangleViewModel(Point2D a, Point2D b, Point2D c,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(new List<Point2D> { a, b, c }, 
            lineColor, thickness, fillColor, opacity)
    {
        Name = "Треугольник";
    }

    /// <summary>
    /// Создает клон фигуры.
    /// </summary>
    public override FigureViewModel Clone()
    {
        var a = new Point2D(Vertices[0].X, Vertices[0].Y);
        var b = new Point2D(Vertices[1].X, Vertices[1].Y);
        var c = new Point2D(Vertices[2].X, Vertices[2].Y);

        var clone = new TriangleViewModel(a, b, c, LineColor, Thickness, FillColor, Opacity);

        return clone;
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
    
    /// <summary>Масштабирование треугольника относительно центра</summary>
    public void UpdateVertices(Point2D center, double newRadius)
    {
        var scale = newRadius / 50;
    
        for (int i = 0; i < Vertices.Count; i++)
        {
            var dx = Vertices[i].X - center.X;
            var dy = Vertices[i].Y - center.Y;
            Vertices[i].X = center.X + dx * scale;
            Vertices[i].Y = center.Y + dy * scale;
        }
        foreach (var vertex in Vertices)
        {
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
    
        this.RaisePropertyChanged(nameof(Center));
        this.RaisePropertyChanged(nameof(Vertices));
    }
}