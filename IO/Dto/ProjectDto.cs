
namespace graphic_editor.IO.Dto;

public class ProjectDto
{
    public string Version { get; set; } = "1.0";
    public double Zoom { get; set; } = 1.0;
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public List<LayerDto> Layers { get; set; } = new();
}
