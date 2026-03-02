// ViewModels/Geometry/Figures/Primitives/EllipseViewModel.cs

using System;
using System.Collections.Generic;
using System.Drawing;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс эллипса.
/// </summary>
public class EllipseViewModel: FigureViewModel
{
    public EllipseViewModel(): this(0, 0, 100, 100, Color.Black, 1, Color.Green, 1.0) {}

    public EllipseViewModel(double x, double y, double width, double height, Color lineColor, double thickness, Color fillColor, double opacity)
    {
        Name = "Эллипс";
        Vertices.Add(new PointViewModel(x, y));
        Vertices.Add(new PointViewModel(x + width, y));
        Vertices.Add(new PointViewModel(x + width, y + height));
        Vertices.Add(new PointViewModel(x, y + height));
        LineColor = lineColor;
        Thickness = thickness;
        FillColor = fillColor == default ? Color.Transparent : fillColor;
        Opacity = opacity;
    }

    public double X => Vertices[0].X;
    public double Y => Vertices[0].Y;
    public double Width => Math.Abs(Vertices[2].X - Vertices[0].X);
    public double Height => Math.Abs(Vertices[2].Y - Vertices[0].Y);
    public double RadiusX => Width / 2;
    public double RadiusY => Height / 2;

    public override Point2D Center => new Point2D(X + Width / 2, Y + Height  / 2);

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
    }

    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        var center = Center;
        var dx = (point.X - center.X) / RadiusX;
        var dy = (point.Y - center.Y) / RadiusY;
        return (dx * dx + dy * dy) <= 1 + eps;
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
    
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
        this.RaisePropertyChanged(nameof(RadiusX));
        this.RaisePropertyChanged(nameof(RadiusY));
    }
}
