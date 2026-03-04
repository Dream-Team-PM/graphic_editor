// Commands/CompositeCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;

namespace graphic_editor.Commands;

/// <summary>
/// Составная команда — объединяет несколько действий в одно для атомарного Undo/Redo.
/// </summary>
public class CompositeCommand : IHistoryAction
{
	/// <summary>
    /// Описание составной команды для отображения в истории.
    /// </summary>
    public string Description { get; } /// <summary>Публичное свойство - описание команды.</summary>
    private readonly IHistoryAction[] _commands; /// <summary>Приватное свойство - массив команд.</summary>
    
	/// <summary>
    /// Инициализирует новый экземпляр составной команды.
    /// </summary>
    /// <param name="description">Человекочитаемое описание команды.</param>
    /// <param name="commands">Массив команд для выполнения в составе группы.</param>
    public CompositeCommand(string description, params IHistoryAction[] commands)
    {
        Description = description;
        _commands = commands;
    }
    
    /// <summary>
    /// Выполняет все команды в прямом порядке.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для выполнения операций.</param>
    public void Execute(CanvasViewModel canvas)
    {
        foreach (var cmd in _commands)
        {
            // Если команда — IFigureCommand, вызываем её Execute
            if (cmd is IFigureCommand figureCmd)
                figureCmd.Execute(canvas);
            // Иначе используем Redo как аналог Execute (для Add/Delete)
            else
                cmd.Redo();
        }
    }
    
	/// <summary>
    /// Отменяет все команды в обратном порядке (LIFO).
    /// </summary>
    public void Undo()
    {
        for (int i = _commands.Length - 1; i >= 0; i--)
            _commands[i].Undo();
    }
    
	/// <summary>
    /// Повторяет все команды в прямом порядке.
    /// </summary>
    public void Redo()
    {
        foreach (var cmd in _commands)
            cmd.Redo();
    }
}