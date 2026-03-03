// ViewModels/Geometry/Figures/Primitives/LineViewModel.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Модель точки, основывается на FigureViewModel.
/// </summary>
public class LineViewModel: FigureViewModel
{
    public LineViewModel(): this(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1.0) {}

    public LineViewModel(double x1, double y1, double x2, double y2, Color lineColor, double thickness, Color fillColor, double opacity)
    {
        Name = "Линия";
        Vertices.Add(new PointViewModel(x1, y1));
        Vertices.Add(new PointViewModel(x2, y2));
		LineColor = lineColor;
		Thickness = thickness;
		FillColor = Color.Transparent;
        Opacity = opacity;
    }
    public double X1 => Vertices[0].X;
    public double Y1 => Vertices[0].Y;
    public double X2 => Vertices[1].X;
    public double Y2 => Vertices[1].Y;
    public double Length => Math.Sqrt(Math.Pow(X2 - X1, 2) + Math.Pow(Y2 - Y1, 2));
    public double Angle => Math.Atan2(Y2 - Y1, X2 - X1) * 180 / Math.PI;
    public override Point2D Center => new Point2D((X1 + X2) / 2, (Y1 + Y2)  / 2);

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

    public override bool IsIn(Point2D point, double eps = 0.05)
    {
        //return Point2D.DistancePointToSegment(point, 
        //   new Point2D(X1, Y1), new Point2D(X2, Y2)) <= eps;
		return Point2D.IsPointNearSegment(point, 
           new Point2D(X1, Y1), new Point2D(X2, Y2), eps);
    }

    public override IEnumerable<Point2D> GetVertexPoint()
    {
        yield return new Point2D(X1, Y1);
        yield return new Point2D(X2, Y2);
    }
    
    public override FigureViewModel Clone()
    {
        var clone = new LineViewModel(X1, Y1, X2, Y2, LineColor, Thickness, FillColor, Opacity / 100.0)
        {
            IsSelected = IsSelected
        };
        return clone;
    }

    public void NotifyPropertyChanged()
	{
    	this.RaisePropertyChanged(nameof(X1));
    	this.RaisePropertyChanged(nameof(Y1));
    	this.RaisePropertyChanged(nameof(X2));
    	this.RaisePropertyChanged(nameof(Y2));
    	this.RaisePropertyChanged(nameof(Angle));
    	this.RaisePropertyChanged(nameof(Length));
        this.RaisePropertyChanged(nameof(Vertices));
	}
}