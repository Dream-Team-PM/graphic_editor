using graphic_editor.ViewModels;

namespace graphic_editor.Interfaces;

/// <summary>
/// Интерфейс для команд истории, требующих доступа к CanvasViewModel.
/// Расширяет IHistoryAction методом инъекции зависимости канваса.
/// </summary>
public interface IHistoryActionWithCanvas : IHistoryAction
{
    /// <summary>
    /// Устанавливает ссылку на CanvasViewModel для выполнения команды.
    /// Вызывается перед Undo/Redo для инициализации контекста.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для работы команды.</param>
    void SetCanvas(CanvasViewModel canvas);
}