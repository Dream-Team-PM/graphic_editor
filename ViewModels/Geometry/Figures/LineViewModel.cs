// ViewModels/Geometry/Figures/LineViewModel.cs

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
    public LineViewModel(): this(0, 0, 100, 100, Color.Black, 1) {}

    public LineViewModel(double x1, double y1, double x2, double y2, Color lineColor, double thickness, Color fillColor = default)
    {
        Name = "Линия";
        Vertices.Add(new PointViewModel(x1, y1));
        Vertices.Add(new PointViewModel(x2, y2));
		LineColor = lineColor;
		Thickness = thickness;
		FillColor = fillColor == default ? Color.Transparent : fillColor;
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

    public override bool IsIn(Point_1 point, double eps = 0.05)
    {
        //return Point_1.DistancePointToSegment(point, 
        //   new Point_1(X1, Y1), new Point_1(X2, Y2)) <= eps;
		return Point_1.IsPointNearSegment(point, 
           new Point_1(X1, Y1), new Point_1(X2, Y2), eps);
    }

    public override IEnumerable<Point_1> GetVertexPoint()
    {
        yield return new Point_1(X1, Y1);
        yield return new Point_1(X2, Y2);
    }
    
    public override FigureViewModel Clone()
    {
        var clone = new LineViewModel(X1, Y1, X2, Y2, LineColor, Thickness, FillColor)
        {
            IsSelected = IsSelected
        };
        return clone;
    }

	public virtual void Reflection(Point_1 a, Point_1 b)
	{
    	var center = Center;
    	foreach (var vertex in Vertices)
    	{
        	var reflected = ReflectPoint(vertex.ToPoint(), a, b);
        	vertex.X = reflected.X;
        	vertex.Y = reflected.Y;
    	}
    	NotifyPropertyChanged();
	}

	private Point_1 ReflectPoint(Point_1 p, Point_1 a, Point_1 b)
	{
    	var d = b - a;
    	double A = d.Y;
    	double B = -d.X;
    	double C = d.X * a.Y - d.Y * a.X;
    	double D = (A * p.X + B * p.Y + C) / (A * A + B * B);
    
    	return new Point_1(
        	p.X - 2 * A * D,
        	p.Y - 2 * B * D
    	);
	}

	public virtual bool HasIntersection(Point_1 leftTop, Point_1 rightBottom)
	{
    	double minX = Math.Min(leftTop.X, rightBottom.X);
   	 	double maxX = Math.Max(leftTop.X, rightBottom.X);
    	double minY = Math.Min(leftTop.Y, rightBottom.Y);
    	double maxY = Math.Max(leftTop.Y, rightBottom.Y);
    
    	var bounds = GetBoundingBox();
    	return !(bounds.MaxX < minX || bounds.MinX > maxX || 
             bounds.MaxY < minY || bounds.MinY > maxY);
	}

	protected virtual (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox()
	{
    	var vertices = Vertices.Select(v => v.ToPoint()).ToList();
    	return (
        	vertices.Min(p => p.X),
        	vertices.Max(p => p.X),
        	vertices.Min(p => p.Y),
        	vertices.Max(p => p.Y)
    	);
	}

	private void NotifyPropertyChanged()
	{
    	this.RaisePropertyChanged(nameof(X1));
    	this.RaisePropertyChanged(nameof(Y1));
    	this.RaisePropertyChanged(nameof(X2));
    	this.RaisePropertyChanged(nameof(Y2));
    	this.RaisePropertyChanged(nameof(Angle));
    	this.RaisePropertyChanged(nameof(Length));
	}
}