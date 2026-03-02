// Commands/AddFigureCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

public class AddFigureCommand : FigureCommandBase
{
    public FigureViewModel Figure { get; }
    public Guid? TargetLayerId { get; }
    
    public AddFigureCommand(FigureViewModel figure, Guid? targetLayerId)
    {
        Figure = figure;
        TargetLayerId = targetLayerId;
    }
    private bool _wasAdded = false;
    private Guid _addedToLayerId;
    
    public override string Description => $"Добавление: {Figure?.Name ?? "Figure"}";
    
    public override void Execute(CanvasViewModel canvas)
    {
        if (_wasAdded) return;
        
        var layer = TargetLayerId != null 
            ? canvas.Layers.FirstOrDefault(l => l.Id == TargetLayerId)
            : canvas.ActiveLayer;
            
        if (layer != null && Figure != null)
        {
            layer.Figures.Add(Figure);
            _addedToLayerId = layer.Id;
            _wasAdded = true;
        }
    }
    
    public override void Undo()
    {
        if (_wasAdded)
        {
            var layer = canvas.Layers.FirstOrDefault(l => l.Id == _addedToLayerId);
            layer?.Figures.Remove(Figure);
            _wasAdded = false;
        }
    }
    
    public override void Redo() => Execute(canvas);
}