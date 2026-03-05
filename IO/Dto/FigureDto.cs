using System.Text.Json.Serialization;

namespace graphic_editor.IO.Dto;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(RectangleDto), "Rectangle")]
[JsonDerivedType(typeof(EllipseDto), "Ellipse")]
[JsonDerivedType(typeof(CircleDto), "Circle")]
[JsonDerivedType(typeof(LineDto), "Line")]
[JsonDerivedType(typeof(PenPointDto), "PenPoint")]
[JsonDerivedType(typeof(GroupDto), "Group")]
public abstract class FigureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Opacity { get; set; } = 1.0;
    public double Rotation { get; set; }
    public int LineColorArgb { get; set; } = System.Drawing.Color.Black.ToArgb();
    public int FillColorArgb { get; set; } = System.Drawing.Color.Transparent.ToArgb();
    public double Thickness { get; set; } = 1.0;
    public bool IsSelected { get; set; }
}

public class RectangleDto : FigureDto
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class EllipseDto : FigureDto
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class CircleDto : FigureDto
{
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double Radius { get; set; }
}

public class LineDto : FigureDto
{
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
}

public class PenPointDto : FigureDto
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class GroupDto : FigureDto
{
    public List<FigureDto> Children { get; set; } = new();
}
