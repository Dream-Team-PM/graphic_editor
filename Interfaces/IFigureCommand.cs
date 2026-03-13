using graphic_editor.ViewModels;

namespace graphic_editor.Interfaces;

/// <summary>
/// Интерфейс команды для изменения фигуры с поддержкой системы Undo/Redo.
/// Реализует паттерн Command для инкапсуляции действий над фигурами.
/// </summary>
public interface IFigureCommand : IHistoryAction
{
    /// <summary>
    /// Выполняет команду над указанным канвасом.
    /// Изменяет состояние фигур и может обновлять ViewModel.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для применения изменений.</param>
    void Execute(CanvasViewModel canvas);
}