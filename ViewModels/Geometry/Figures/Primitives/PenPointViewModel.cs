// ViewModels/Geometry/Figures/Primitives/PenPointViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

public class PenPointViewModel : FigureViewModel
{
    public PenPointViewModel(): this(100, 100, Color.Black, 1, Color.Transparent, 1.0) {}
    public PenPointViewModel(double x, double y, Color lineColor, double thickness, Color fillColor, double opacity = 1.0)
    {
        Name = "Точка пера";
        Vertices.Add(new PointViewModel(x, y));
        LineColor = lineColor;
        Thickness = thickness;
        FillColor = Color.Transparent;
        Opacity = opacity;
    }

    public double X => Vertices[0].X;
    public double Y => Vertices[0].Y;

    public override Point2D Center => new Point2D(X, Y);

    public override void Rotate(double angle) 
    {
        NotifyPropertyChanged(); 
    }

    public override void Scale(double sx, double sy)
    {
        NotifyPropertyChanged(); 
    }
    
    public override void Move(double dx, double dy)
    {
        Vertices[0].X += dx;
        Vertices[0].Y += dy;
    }

    public override bool IsIn(Point2D point, double eps = 5.0)
    {
        // Проверяем попадание в радиус eps вокруг точки
        var dx = point.X - X;
        var dy = point.Y - Y;
        return (dx * dx + dy * dy) <= (eps * eps);
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        yield return new Point2D(X, Y);
    }
    
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }
}