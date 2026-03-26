
namespace graphic_editor.IO.Dto;

public class LayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Слой 1";
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
    public List<FigureDto> Figures { get; set; } = new();
}

