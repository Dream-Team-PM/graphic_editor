using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using graphic_editor.State;

namespace graphic_editor.Interfaces;

/// <summary>
/// Стратегия создания и обновления фигур для конкретного инструмента рисования.
/// Реализует паттерн Strategy для инкапсуляции логики разных инструментов.
/// </summary>
public interface IDrawingStrategy
{
    /// <summary>
    /// Создаёт предварительную фигуру для визуализации в процессе рисования.
    /// </summary>
    /// <param name="start">Начальная точка рисования.</param>
    /// <param name="current">Текущая позиция указателя.</param>
    /// <param name="style">Настройки стиля (цвет, толщина, прозрачность).</param>
    /// <returns>Модель фигуры для предварительного отображения или null.</returns>
    FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style);
    
    /// <summary>
    /// Обновляет координаты предварительной фигуры при перемещении указателя.
    /// </summary>
    /// <param name="preview">Экземпляр предварительной фигуры для обновления.</param>
    /// <param name="start">Начальная точка рисования.</param>
    /// <param name="current">Текущая позиция указателя.</param>
    void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current);
    
    /// <summary>
    /// Создаёт финальную фигуру после завершения операции рисования.
    /// </summary>
    /// <param name="start">Начальная точка рисования.</param>
    /// <param name="end">Конечная точка рисования.</param>
    /// <param name="style">Настройки стиля для финальной фигуры.</param>
    /// <returns>Готовая модель фигуры или null, если создание невозможно.</returns>
    FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style);
    
    /// <summary>
    /// Получает значение, указывающее, требует ли инструмент режима drag-to-create
    /// (непрерывное рисование при зажатой кнопке мыши).
    /// </summary>
    bool RequiresDrag { get; }
    
    /// <summary>
    /// Получает значение, указывающее, требует ли инструмент режима multi-click
    /// (последовательное добавление точек, как в инструменте Pen).
    /// По умолчанию возвращает false.
    /// </summary>
    bool RequiresMultiClick => false;
}