using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using graphic_editor.State;

namespace graphic_editor.Interfaces;

/// <summary>Стратегия создания и обновления фигур для конкретного инструмента.</summary>
public interface IDrawingStrategy
{
    /// <summary>Создать предварительную фигуру для отображения в процессе рисования.</summary>
    FigureViewModel? CreatePreview(Point2D start, Point2D current, StyleSettings style);
    
    /// <summary>Обновить координаты предварительной фигуры.</summary>
    void UpdatePreview(FigureViewModel preview, Point2D start, Point2D current);
    
    /// <summary>Создать финальную фигуру после завершения рисования.</summary>
    FigureViewModel? CreateFinal(Point2D start, Point2D end, StyleSettings style);
    
    /// <summary>Требуется ли режим drag-to-create (мышь зажата).</summary>
    bool RequiresDrag { get; }
    
    /// <summary>Требуется ли режим multi-click (как для Pen).</summary>
    bool RequiresMultiClick => false;
}