// Commands/RemoveFiguresFromLayerCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using graphic_editor.Helpers;

namespace graphic_editor.Commands;

/// <summary>Команда удаления фигур из слоя (для группировки).</summary>
public class RemoveFiguresFromLayerCommand : IHistoryAction
{
    private readonly List<Guid> _figureIds;
    private readonly Guid _layerId;
    private readonly List<FigureViewModel> _removedFigures = new();
    private CanvasViewModel? _canvas;  // ← Сохраняем canvas
    
    public string Description => $"Удаление {_removedFigures.Count} фигур из слоя";
    
    public RemoveFiguresFromLayerCommand(List<FigureViewModel> figures, Guid layerId)
    {
        _figureIds = figures.Select(f => f.Id).ToList();
        _layerId = layerId;
        _removedFigures.AddRange(figures);
    }
    
    public void StoreCanvas(CanvasViewModel canvas) => _canvas = canvas;
    
    public void Execute(CanvasViewModel canvas)
    {
        _canvas = canvas;  // ← Сохраняем для Undo/Redo
        
        // ✅ Защита от null
        if (canvas == null || canvas.Layers == null)
        {
            DebugLog.Write("[ERROR] RemoveFiguresFromLayerCommand: canvas or Layers is null");
            return;
        }
        
        var layer = canvas.Layers.FirstOrDefault(l => l.Id == _layerId);
        if (layer == null)
        {
            DebugLog.Write($"[ERROR] RemoveFiguresFromLayerCommand: Layer {_layerId} not found");
            return;
        }
        
        foreach (var figure in _removedFigures)
        {
            if (layer.Figures.Contains(figure))
            {
                layer.Figures.Remove(figure);
                DebugLog.Write($"[DEBUG] RemoveFiguresFromLayerCommand: Removed {figure.Name}");
            }
        }
    }
    
    public void Undo()
    {
        if (_canvas == null || _canvas.Layers == null)
        {
            DebugLog.Write("[ERROR] RemoveFiguresFromLayerCommand.Undo: canvas is null");
            return;
        }
        
        var layer = _canvas.Layers.FirstOrDefault(l => l.Id == _layerId);
        if (layer == null) return;
        
        // Восстанавливаем фигуры в обратном порядке
        for (int i = _removedFigures.Count - 1; i >= 0; i--)
        {
            var figure = _removedFigures[i];
            if (!layer.Figures.Contains(figure))
            {
                layer.Figures.Insert(0, figure);
                DebugLog.Write($"[DEBUG] RemoveFiguresFromLayerCommand.Undo: Restored {figure.Name}");
            }
        }
    }
    
    public void Redo() => Execute(_canvas);
}