// Commands/DeleteFigureCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;


public class DeleteFigureCommand : FigureCommandBase
{
    public List<FigureViewModel> Figures { get; }
    
    public DeleteFigureCommand(List<FigureViewModel> figures)
    {
        Figures = figures;
    }
    private readonly List<(FigureViewModel Figure, Guid LayerId)> _deleted = new();
    
    public override string Description => $"Удаление {Figures.Count} фигур(ы)";
    
    public override void Execute(CanvasViewModel canvas)
    {
        this.canvas = canvas;
        foreach (var figure in Figures)
        {
            var layer = canvas.Layers.FirstOrDefault(l => l.Figures.Contains(figure));
            if (layer != null)
            {
                layer.Figures.Remove(figure);
                _deleted.Add((figure, layer.Id));
            }
        }
    }
    
    public override void Undo()
    {
        foreach (var (figure, layerId) in _deleted)
        {
            var layer = canvas.Layers.FirstOrDefault(l => l.Id == layerId);
            layer?.Figures.Add(figure);
        }
    }
    
    public override void Redo() => Execute(canvas);
}