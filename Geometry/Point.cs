namespace graphic_editor;
public record Point(double X, double Y)
{
    public static Point operator +(Point left, Point right) => 
        new(left.X + right.X, left.Y + right.Y);

    public static Point operator -(Point left, Point right) => 
        new(left.X - right.X, left.Y - right.Y);
    
    public static Point operator *(Point p, double scale) => 
        new(p.X * scale, p.Y * scale);
    
    public static Point operator *(double scale, Point p) => 
        new(p.X * scale, p.Y * scale);
    
    public static Point operator /(Point p, double scale) => 
        new(p.X / scale, p.Y / scale);
}