// Commands/AddFigureCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;

namespace graphic_editor.Commands;

public class AddFigureCommand : FigureCommandBase  // ← Наследуемся от FigureCommandBase
{
    private readonly FigureViewModel _figure;
    private readonly Guid? _layerId;
    private bool _wasAdded = false;
    private Guid _addedToLayerId;
    
    public AddFigureCommand(FigureViewModel figure, Guid? layerId = null)
    {
        _figure = figure;
        _layerId = layerId;
    }
    
    public override string Description => $"Добавление: {_figure?.Name ?? "Figure"}";
    
    public override void Execute(CanvasViewModel canvas)  // ← override
    {
        if (_wasAdded) return;
        
        var layer = FindLayer(canvas, _layerId);
        if (layer != null && _figure != null)
        {
            layer.Figures.Add(_figure);
            _addedToLayerId = layer.Id;
            _wasAdded = true;
            this.canvas = canvas;  // ← Сохраняем в базовом классе
        }
    }
    
    public override void Undo()  // ← override
    {
        if (!_wasAdded || canvas == null || _figure == null) return;
        
        var layer = FindLayer(canvas, _addedToLayerId);
        if (layer != null && layer.Figures.Contains(_figure))
        {
            layer.Figures.Remove(_figure);
            _wasAdded = false;
        }
    }
    
    public override void Redo() => Execute(canvas);  // ← override
    
    private LayerViewModel? FindLayer(CanvasViewModel canvas, Guid? layerId)
    {
        if (layerId != null)
            return canvas.Layers.FirstOrDefault(l => l.Id == layerId);
        return canvas.ActiveLayer;
    }
}