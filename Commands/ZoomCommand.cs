// Commands/ZoomCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

/// <summary>
/// Команда изменения масштаба отображения холста.
/// </summary>
public class ZoomCommand : IHistoryAction
{
    private readonly double _oldZoom; /// <summary>Приватное свойство - старое значение зума.</summary>
    private readonly double _newZoom; /// <summary>Приватное свойство - новое значение зума.</summary>
    private CanvasViewModel? _canvas; /// <summary>Приватное свойство - текущий канвас.</summary>
    /// <inheritdoc/>
    public string Description => $"Масштаб: {_newZoom:P0}";
    
    /// <summary>
    /// Инициализирует новый экземпляр команды изменения масштаба.
    /// </summary>
    /// <param name="oldZoom">Предыдущее значение масштаба.</param>
    /// <param name="newZoom">Новое значение масштаба.</param>
    public ZoomCommand(double oldZoom, double newZoom)
    {
        _oldZoom = oldZoom;
        _newZoom = newZoom;
    }
    
    /// <summary>
    /// Отменяет команду: восстанавливает предыдущее значение масштаба.
    /// </summary>
    public void Undo()
    {
        if (_canvas != null)
        {
            _canvas.Zoom = _oldZoom;
        }
    }
    
    /// <summary>
    /// Повторяет команду: устанавливает новое значение масштаба.
    /// </summary>
    public void Redo()
    {
        if (_canvas != null)
        {
            _canvas.Zoom = _newZoom;
        }
    }
    
    /// <summary>
    /// Устанавливает ссылку на CanvasViewModel для выполнения команды.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel.</param>
    public void SetCanvas(CanvasViewModel canvas) => _canvas = canvas;
}