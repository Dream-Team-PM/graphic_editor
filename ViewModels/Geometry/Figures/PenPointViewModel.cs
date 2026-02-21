// ViewModels/Geometry/Figures/PenPointViewModel.cs
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
    public PenPointViewModel(double x, double y)
    {
        Name = "Точка пера";
        // Создаём единственную вершину для точки
        Vertices.Add(new PointViewModel(x, y));
    }

    public double X => Vertices[0].X;
    public double Y => Vertices[0].Y;

    public override Point_1 Center => new Point_1(X, Y);

    public override void Rotate(double angle) { /* Точка не вращается */ }
    
    public override void Scale(double sx, double sy) { /* Точка не масштабируется */ }
    
    public override void Move(double dx, double dy)
    {
        Vertices[0].X += dx;
        Vertices[0].Y += dy;
    }

    public override bool IsIn(Point_1 point, double eps = 5.0)
    {
        // Проверяем попадание в радиус eps вокруг точки
        var dx = point.X - X;
        var dy = point.Y - Y;
        return (dx * dx + dy * dy) <= (eps * eps);
    }

    public override IEnumerable<Point_1> GetVertexPoint()
    {
        yield return new Point_1(X, Y);
    }
}