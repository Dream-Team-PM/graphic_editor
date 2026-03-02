// Commands/DeleteFigureCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using graphic_editor.Helpers;

namespace graphic_editor.Commands;

public class DeleteFigureCommand : FigureCommandBase  // ← Наследуемся от FigureCommandBase
{
    private readonly List<FigureViewModel> _figures;
    private readonly List<(FigureViewModel Figure, Guid LayerId)> _deleted = new();
    
    public DeleteFigureCommand(List<FigureViewModel> figures)
    {
        _figures = new List<FigureViewModel>(figures);
    }
    
    public override string Description => $"Удаление {_figures.Count} фигур(ы)";
    
    public override void Execute(CanvasViewModel canvas)  // ← Обязательно: override
    {
        this.canvas = canvas;  // ← Сохраняем canvas для Undo/Redo
        
        if (canvas == null || canvas.Layers == null)
        {
            DebugLog.Write("[ERROR] DeleteFigureCommand: canvas or Layers is null");
            return;
        }
        
        foreach (var figure in _figures)
        {
            var layer = canvas.Layers.FirstOrDefault(l => l.Figures.Contains(figure));
            if (layer != null)
            {
                layer.Figures.Remove(figure);
                _deleted.Add((figure, layer.Id));
                DebugLog.Write($"[DEBUG] DeleteFigureCommand: Removed {figure.Name} from {layer.Name}");
            }
            else
            {
                DebugLog.Write($"[WARN] DeleteFigureCommand: Figure {figure.Name} not found in any layer");
            }
        }
    }
    
    public override void Undo()  // ← override
    {
        if (canvas == null || canvas.Layers == null) return;
        
        for (int i = _deleted.Count - 1; i >= 0; i--)
        {
            var (figure, layerId) = _deleted[i];
            var layer = canvas.Layers.FirstOrDefault(l => l.Id == layerId);
            if (layer != null && !layer.Figures.Contains(figure))
            {
                layer.Figures.Add(figure);
                DebugLog.Write($"[DEBUG] DeleteFigureCommand.Undo: Restored {figure.Name}");
            }
        }
    }
    
    public override void Redo() => Execute(canvas);  // ← override
}