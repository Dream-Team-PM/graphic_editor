// Commands/IFigureCommand.cs
using graphic_editor.ViewModels;

namespace graphic_editor.Interfaces;

/// <summary>Команда для изменения фигуры с поддержкой Undo/Redo.</summary>
public interface IFigureCommand : IHistoryAction
{
    void Execute(CanvasViewModel canvas);
}