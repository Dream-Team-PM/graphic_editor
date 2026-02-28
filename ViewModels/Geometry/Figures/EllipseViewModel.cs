// ViewModels/Geometry/Figures/EllipseViewModel.cs

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
    private Point_1 _center;
    private double _radiusX, _radiusY;
    private double _rotation;
    public EllipseViewModel(): this(0, 0, 100, 100, Color.Black, 1, Color.Green, 1.0) {}

    public EllipseViewModel(double x, double y, double width, double height, Color lineColor, double thickness, Color fillColor, double opacity)
    {
        Name = "Эллипс";
        _center = new Point_1(x + width/2, y + height/2);
        _radiusX = width / 2;
        _radiusY = height / 2;
        _rotation = 0;
        Vertices.Add(new PointViewModel(x, y));
        Vertices.Add(new PointViewModel(x + width, y));
        Vertices.Add(new PointViewModel(x + width, y + height));
        Vertices.Add(new PointViewModel(x, y + height));
        UpdateVertices();
    }

    public override Point_1 Center => _center;

    // public double RadiusX => _radiusX;
    // public double RadiusY => _radiusY;
    
    public double Rotation 
    {
        get => _rotation;
        set => this.RaiseAndSetIfChanged(ref _rotation, value);
    }
    
    public double X => _center.X - _radiusX;
    public double Y => _center.Y - _radiusY;
    
    // public double X => Vertices[0].X;
    // public double Y => Vertices[0].Y;
    public double Width => Math.Abs(Vertices[2].X - Vertices[0].X);
    public double Height => Math.Abs(Vertices[2].Y - Vertices[0].Y);
    public double RadiusX => Width / 2;
    public double RadiusY => Height / 2;

    // public override Point_1 Center => new Point_1(X + Width / 2, Y + Height  / 2);

    public override void Rotate(double angle)
    {
        var center = Center;
    
        foreach (var vertex in Vertices)
        {
            var rotated = TransformHelpers.RotatePoint(vertex.ToPoint(), center, angle);
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
            var scaled = Point_1.ScalePoint(vertex.ToPoint(), center, sx, sy);
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
    
    private void UpdateVertices()
    {
        var x = Center.X - RadiusX;
        var y = Center.Y - RadiusY;
        var w = RadiusX * 2;
        var h = RadiusY * 2;
    
        if (Vertices.Count == 0)
        {
            Vertices.Add(new PointViewModel(x, y));                    // Top-Left
            Vertices.Add(new PointViewModel(x + w, y));                // Top-Right
            Vertices.Add(new PointViewModel(x + w, y + h));            // Bottom-Right
            Vertices.Add(new PointViewModel(x, y + h));                // Bottom-Left
        }
        else
        {
            Vertices[0].X = x; Vertices[0].Y = y;
            Vertices[1].X = x + w; Vertices[1].Y = y;
            Vertices[2].X = x + w; Vertices[2].Y = y + h;
            Vertices[3].X = x; Vertices[3].Y = y + h;
        }

        NotifyPropertyChanged();
    }

    
    private void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(Width));
        this.RaisePropertyChanged(nameof(Height));
        this.RaisePropertyChanged(nameof(RadiusX));
        this.RaisePropertyChanged(nameof(RadiusY));
    }
}