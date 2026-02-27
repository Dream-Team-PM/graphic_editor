// ViewModels/Geometry/Figures/HeptagonViewModel.cs

using System;
using System.Collections.Generic;
using System.Drawing;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс семиугольника (в процессе реализации).
/// </summary>
public class HeptagonViewModel: FigureViewModel
{
    public HeptagonViewModel(): this(0, 0, 100, 100) {}

    public HeptagonViewModel(double x, double y, double width, double height)
    {
        Name = "Семиугольник";
        Vertices.Add(new PointViewModel(x, y));
        Vertices.Add(new PointViewModel(x + width, y));
        Vertices.Add(new PointViewModel(x + width, y + height));
        Vertices.Add(new PointViewModel(x, y + height));
    }

    public double X => Vertices[0].X;
    public double Y => Vertices[0].Y;
    public double Width => Math.Abs(Vertices[2].X - Vertices[0].X);
    public double Height => Math.Abs(Vertices[2].Y - Vertices[0].Y);
    public double RadiusX => Width / 2;
    public double RadiusY => Height / 2;

    public override Point_1 Center => new Point_1(X + Width / 2, Y + Height  / 2);

    public override void Rotate(double angle)
    {
        var center = Center;
        var rad = angle * Math.PI / 180;
        var cos = Math.Cos(rad);
        var sin = Math.Sin(rad);

        foreach (var vertex in Vertices)
        {
            var dx = vertex.X - center.X;
            var dy = vertex.Y - center.Y;
            vertex.X = center.X + dx * cos - dy * sin;
            vertex.Y = center.Y + dx * sin + dy * cos;
        }

        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }

    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            vertex.X = center.X + (vertex.X - center.X) * sx;
            vertex.Y = center.Y + (vertex.Y - center.Y) * sy;
        }
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
    }

    public override void Move(double dx, double dy)
    {
        foreach (var vertex in Vertices)
        {
            vertex.X += dx;
            vertex.Y += dy;
        }
    }

    public override bool IsIn(Point_1 point, double eps = 0.001)
    {
        var center = Center;
        var dx = (point.X - center.X) / RadiusX;
        var dy = (point.Y - center.Y) / RadiusY;
        return (dx * dx + dy * dy) <= 1 + eps;
    }

    public override IEnumerable<Point_1> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}