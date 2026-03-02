namespace graphic_editor.Interfaces;

/// <summary>
/// Публичный интерфейс для работы с операциями (Находится в разработке).
/// </summary>
public interface IHistoryAction
{
    string Description { get; } /// <summary>Описание операции.</summary>
    void Undo(); /// <summary>Отменить.</summary>
    void Redo(); /// <summary>Повторить.</summary>
}