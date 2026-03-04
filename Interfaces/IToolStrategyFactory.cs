using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using graphic_editor.Models;

namespace graphic_editor.Interfaces;

/// <summary>
/// Фабрика стратегий рисования для инструментов редактора.
/// Реализует паттерн Factory Method для создания IDrawingStrategy по типу инструмента.
/// </summary>
public interface IToolStrategyFactory
{
    /// <summary>
    /// Возвращает стратегию рисования для заданного инструмента.
    /// </summary>
    /// <param name="tool">Перечисление DrawingTool, определяющее тип инструмента.</param>
    /// <returns>Экземпляр IDrawingStrategy для обработки инструмента.</returns>
    /// <exception cref="NotSupportedException">Если инструмент не поддерживается.</exception>
    IDrawingStrategy GetStrategy(DrawingTool tool);
    
    /// <summary>
    /// Проверяет, поддерживается ли заданный инструмент фабрикой.
    /// </summary>
    /// <param name="tool">Перечисление DrawingTool для проверки.</param>
    /// <returns>True, если стратегия для инструмента существует; иначе false.</returns>
    bool IsSupported(DrawingTool tool);
}