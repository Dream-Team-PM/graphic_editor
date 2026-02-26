using System;
using System.Collections.Generic;
using System.Drawing;

namespace graphic_editor;

public class Square : PolygonFigure
{
    public Square(Point center, double side,
        Color lineColor, Color fillColor, double thickness)
        : base(CreateVertices(center, side),
               lineColor, fillColor, thickness)
    {
    }

    private static IEnumerable<Point> CreateVertices(Point center, double side)
    {
        double h = side / 2;

        return new List<Point>
        {
            new Point(center.X - h, center.Y - h),
            new Point(center.X + h, center.Y - h),
            new Point(center.X + h, center.Y + h),
            new Point(center.X - h, center.Y + h)
        };
    }
}
