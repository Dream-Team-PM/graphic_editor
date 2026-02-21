// ViewModels/Geometry/Figures/LineViewModel.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

public class LineViewModel: FigureViewModel
{
    public LineViewModel(): this(0, 0, 100, 100) {}

    public LineViewModel(double x1, double y1, double x2, double y2)
    {
        Name = "Линия";
        Vertices.Add(new PointViewModel(x1, y1));
        Vertices.Add(new PointViewModel(x2, y2));
    }

    public double X1 => Vertices[0].X;
    public double Y1 => Vertices[0].Y;
    public double X2 => Vertices[1].X;
    public double Y2 => Vertices[1].Y;
    public double Length => Math.Sqrt(Math.Pow(X2 - X1, 2) + Math.Pow(Y2 - Y1, 2));
    public double Angle => Math.Atan2(Y2 - Y1, X2 - X1) * 180 / Math.PI;
    public override Point_1 Center => new Point_1((X1 + X2) / 2, (Y1 + Y2)  / 2);

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
        this.RaisePropertyChanged(nameof(X1));
        this.RaisePropertyChanged(nameof(Y1));
        this.RaisePropertyChanged(nameof(X2));
        this.RaisePropertyChanged(nameof(Y2));
        this.RaisePropertyChanged(nameof(Angle));
    }

    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            vertex.X = center.X + (vertex.X - center.X) * sx;
            vertex.Y = center.Y + (vertex.Y - center.Y) * sy;
        }
        this.RaisePropertyChanged(nameof(X1));
        this.RaisePropertyChanged(nameof(Y1));
        this.RaisePropertyChanged(nameof(X2));
        this.RaisePropertyChanged(nameof(Y2));
        this.RaisePropertyChanged(nameof(Angle));
    }

    public override void Move(double dx, double dy)
    {
        foreach (var vertex in Vertices)
        {
            vertex.X += dx;
            vertex.Y += dy;
        }
    }

    public override bool IsIn(Point_1 point, double eps = 0.05)
    {
        var px = point.X;
        var py = point.Y;
        var x1 = X1;
        var y1 = Y1;
        var x2 = X2;
        var y2 = Y2;
        var dx = x2 - x1;
        var dy = y2 - y1;
        var lengthSq = dx * dx + dy * dy;
        if (lengthSq < eps * eps)
        {
            var distToStartSq = (px - x1) * (px - x1) + (py - y1) * (py - y1);
            return distToStartSq <= eps * eps;
        }

        // Проекция точки на прямую отрезка
        var t = ((px - x1) * dx + (py - y1) * dy) / lengthSq;
        t = Math.Max(0, Math.Min(1, t));

        // Ближайшая точка на отрезке
        var closestX = x1 + t * dx;
        var closestY = y1 + t * dy;

        // Расстояние от точки до ближайшей точки на отрезке
        var distToSegmentSq = (px - closestX) * (px - closestX) + (py - closestY) * (py - closestY);
        return distToSegmentSq <= eps * eps;
    }

    public override IEnumerable<Point_1> GetVertexPoint()
    {
        yield return new Point_1(X1, Y1);
        yield return new Point_1(X2, Y2);
    }
    
    public override FigureViewModel Clone()
    {
        var clone = new LineViewModel(X1, Y1, X2, Y2)
        {
            LineColor = LineColor,
            FillColor = FillColor,
            Thickness = Thickness,
            IsSelected = IsSelected
        };
        return clone;
    }
}