// Commands/DeleteFigureCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using graphic_editor.Helpers;

namespace graphic_editor.Commands;

/// <summary>
/// Команда удаления одной или нескольких фигур с поддержкой Undo/Redo.
/// </summary>
public class DeleteFigureCommand : FigureCommandBase
{
    private readonly List<FigureViewModel> _figures; /// <summary>Приватное свойство - массив фигур.</summary>
    private readonly List<(FigureViewModel Figure, Guid LayerId)> _deleted = new(); /// <summary>Приватное свойство - коллекция удалённых фигур.</summary>
    
	/// <summary>
    /// Инициализирует новый экземпляр команды удаления фигур.
    /// </summary>
    /// <param name="figures">Список фигур для удаления.</param>
    public DeleteFigureCommand(List<FigureViewModel> figures)
    {
        _figures = new List<FigureViewModel>(figures);
    }
    
	/// <inheritdoc/>
    public override string Description => $"Удаление {_figures.Count} фигур(ы)";
    
	/// <summary>
    /// Выполняет команду: удаляет фигуры из их слоёв.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для выполнения операции.</param>
    public override void Execute(CanvasViewModel canvas)
    {
        this.canvas = canvas;
        
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
    
	/// <summary>
    /// Отменяет команду: восстанавливает удалённые фигуры в исходные слои.
    /// </summary>
    public override void Undo()
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
    
	/// <summary>
    /// Повторяет команду: вызывает Execute для повторного удаления фигур.
    /// </summary>
    public override void Redo() => Execute(canvas);
}