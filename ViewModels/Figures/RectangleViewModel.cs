// ViewModels/Figure/RectangleViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using graphic_editor.Models;

namespace graphic_editor.ViewModels;

public class RectangleViewModel: FigureViewModel
{
    public RectangleViewModel(): this(0, 0, 100, 100) {}

    public RectangleViewModel(double x, double y, double width, double height)
    {
        Name = "Прямоугольник";
        Vertices.Add(new PointViewModel(x, y));
        Vertices.Add(new PointViewModel(x + width, y));
        Vertices.Add(new PointViewModel(x + width, y + height));
        Vertices.Add(new PointViewModel(x, y + height));
    }

    public double X => Vertices[0].X;
    public double Y => Vertices[0].Y;
    public double Width => Math.Abs(Vertices[2].X - Vertices[0].X);
    public double Height => Math.Abs(Vertices[2].Y - Vertices[0].Y);

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

        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
    }

    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            vertex.X = center.X + (vertex.X - center.X) * sx;
            vertex.Y = center.Y + (vertex.Y - center.Y) * sy;
        }
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
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
        var minX = Math.Min(Math.Min(Vertices[0].X, Vertices[1].X), Math.Min(Vertices[2].X, Vertices[3].X)) - eps;
        var maxX = Math.Max(Math.Max(Vertices[0].X, Vertices[1].X), Math.Max(Vertices[2].X, Vertices[3].X)) + eps;
        var minY = Math.Min(Math.Min(Vertices[0].Y, Vertices[1].Y), Math.Min(Vertices[2].Y, Vertices[3].Y)) - eps;
        var maxY = Math.Max(Math.Max(Vertices[0].Y, Vertices[1].Y), Math.Max(Vertices[2].Y, Vertices[3].Y)) + eps;
        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
    }

    public override IEnumerable<Point_1> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
}