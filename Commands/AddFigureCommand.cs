// Commands/AddFigureCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Helpers;
using graphic_editor.Interfaces;
using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

/// <summary>
/// Команда добавления фигуры на холст с поддержкой Undo/Redo.
/// </summary>
public class AddFigureCommand : FigureCommandBase
{
    private readonly FigureViewModel _figure; /// <summary>Приватное свойство - фигура для перемещения.</summary>
    private readonly Guid? _layerId; /// <summary>Приватное свойство - ID текущего слоя.</summary>
    private bool _wasAdded = false; /// <summary>Приватное свойство - флаг добавления фигуры на слой.</summary>
    private Guid _addedToLayerId; /// <summary>Приватное свойство - ID добавленной фигуры на слое.</summary>
    
	/// <summary>
    /// Инициализирует новый экземпляр команды добавления фигуры.
    /// </summary>
    /// <param name="figure">Фигура для добавления.</param>
    /// <param name="layerId">Идентификатор целевого слоя (null = активный слой).</param>
    public AddFigureCommand(FigureViewModel figure, Guid? layerId = null)
    {
        _figure = figure;
        _layerId = layerId;
    }
    
	/// <inheritdoc/>
    public override string Description => $"Добавление: {_figure?.Name ?? "Figure"}";
    
	/// <summary>
    /// Выполняет команду: добавляет фигуру в указанный или активный слой.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для выполнения операции.</param>
    public override void Execute(CanvasViewModel canvas)
    {
        if (_wasAdded) return;
        var layer = FindLayer(canvas, _layerId);
        if (layer != null && _figure != null && !layer.Figures.Contains(_figure))
        {
            CaptureBefore(_figure);
            layer.Figures.Add(_figure);
			_addedToLayerId = layer.Id;
            _wasAdded = true;         
            this.canvas = canvas;
            CaptureAfter(_figure);
        }
    }
    
	/// <summary>
    /// Отменяет команду: удаляет добавленную фигуру из слоя.
    /// </summary>
    public override void Undo()
    {
        if (!_wasAdded || canvas == null || _figure == null) return;
        
        var layer = FindLayer(canvas, _addedToLayerId);
        if (layer != null && layer.Figures.Contains(_figure))
        {
            layer.Figures.Remove(_figure);
			_wasAdded = false;
        }
    }
    
	/// <summary>
    /// Повторяет команду: вызывает Execute для повторного добавления фигуры.
    /// </summary>
    public override void Redo() => Execute(canvas);
    
	/// <summary>
    /// Находит слой по идентификатору или возвращает активный слой.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel.</param>
    /// <param name="layerId">Идентификатор слоя или null.</param>
    /// <returns>Найденный слой или null.</returns>
    private LayerViewModel? FindLayer(CanvasViewModel canvas, Guid? layerId)
    {
        if (layerId != null)
            return canvas.Layers.FirstOrDefault(l => l.Id == layerId);
        return canvas.ActiveLayer;
    }
}