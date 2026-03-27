// State/StyleSettings.cs (DTO для передачи стиля)
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
namespace graphic_editor.State;

public record StyleSettings(
    System.Drawing.Color StrokeColor,
    System.Drawing.Color FillColor,
    double StrokeWidth,
    double Opacity = 1.0)
{
    public static StyleSettings Default { get; } = new(
        System.Drawing.Color.Black,
        System.Drawing.Color.Transparent,
        2.0);
}