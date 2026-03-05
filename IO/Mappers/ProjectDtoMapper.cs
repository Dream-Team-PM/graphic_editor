using graphic_editor.IO.Dto;
using graphic_editor.ViewModels;

namespace graphic_editor.IO.Mappers;

public static class ProjectDtoMapper
{
    public static ProjectDto ToDto(CanvasViewModel canvas)
    {
        return new ProjectDto
        {
            Zoom = canvas.Zoom,
            OffsetX = canvas.OffsetX,
            OffsetY = canvas.OffsetY,
            Layers = canvas.Layers.Select(layer => new LayerDto
            {
                Id = layer.Id,
                Name = layer.Name,
                IsVisible = layer.IsVisible,
                IsLocked = layer.IsLocked,
                Figures = layer.Figures.Select(FigureDtoMapper.ToDto).ToList()
            }).ToList()
        };
    }
}
