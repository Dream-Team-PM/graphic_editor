namespace graphic_editor;

public static class Transform2D
{
    public static Point Rotate(Point p, Point center, double angleDeg)
    {
        double rad = angleDeg * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        double x = p.X - center.X;
        double y = p.Y - center.Y;

        return new Point(
            center.X + x * cos - y * sin,
            center.Y + x * sin + y * cos
        );
    }

    public static Point Scale(Point p, Point center, double sx, double sy)
    {
        return new Point(
            center.X + (p.X - center.X) * sx,
            center.Y + (p.Y - center.Y) * sy
        );
    }
}
