
using System.Text.Json;
using System.Text.Json.Serialization;
using graphic_editor.IO.Dto;
using graphic_editor.IO.Mappers;
using graphic_editor.ViewModels;

namespace graphic_editor.IO.ProjectFormat;

public class JsonProjectFormat : IProjectFormat
{
    public string FileExtension => ".vec";

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(string fullPath, CanvasViewModel canvas)
    {
        var dto = ProjectDtoMapper.ToDto(canvas);
        var json = JsonSerializer.Serialize(dto, _options);
        await File.WriteAllTextAsync(fullPath, json);
    }

    public async Task LoadAsync(string fullPath, CanvasViewModel canvas)
    {
        if (!File.Exists(fullPath)) return;

        var json = await File.ReadAllTextAsync(fullPath);
        var dto = JsonSerializer.Deserialize<ProjectDto>(json, _options)!;

        canvas.Layers.Clear();

        foreach (var layerDto in dto.Layers)
        {
            var layer = new LayerViewModel(layerDto.Id, layerDto.Name)
            {
                IsVisible = layerDto.IsVisible,
                IsLocked = layerDto.IsLocked
            };

            foreach (var figDto in layerDto.Figures)
            {
                var figure = FigureDtoMapper.ToViewModel(figDto);
                layer.Figures.Add(figure);
            }

            canvas.Layers.Add(layer);
        }

        canvas.Zoom = dto.Zoom;
        canvas.OffsetX = dto.OffsetX;
        canvas.OffsetY = dto.OffsetY;

        if (canvas.Layers.Count > 0)
            canvas.ActiveLayer = canvas.Layers[0];
    }
}
