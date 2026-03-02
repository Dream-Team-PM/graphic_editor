// Commands/CompositeCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;

namespace graphic_editor.Commands;

/// <summary>Составная команда — объединяет несколько действий в одно для Undo/Redo.</summary>
public class CompositeCommand : IHistoryAction // ← Обычный класс, не record!
{
    public string Description { get; }
    private readonly IHistoryAction[] _commands;
    
    public CompositeCommand(string description, params IHistoryAction[] commands)
    {
        Description = description;
        _commands = commands;
    }
    
    // ← Обязательная реализация Execute
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
    
    public void Undo()
    {
        // Отменяем в ОБРАТНОМ порядке (LIFO)
        for (int i = _commands.Length - 1; i >= 0; i--)
            _commands[i].Undo();
    }
    
    public void Redo()
    {
        // Повторяем в ПРЯМОМ порядке
        foreach (var cmd in _commands)
            cmd.Redo();
    }
}