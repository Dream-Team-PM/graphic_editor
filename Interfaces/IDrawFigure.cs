// Interfaces/IDrawable.cs
using Avalonia.Media;

namespace graphic_editor.Interfaces;

/// <summary>
/// Интерфейс для отрисовки фигуры на холсте.
/// Определяет методы для рендеринга контура и заливки фигуры с использованием DrawingContext.
/// </summary>
public interface IDrawFigure {
	/// <summary>
    /// Отрисовывает контур фигуры с заданным пером.
    /// </summary>
    /// <param name="context">Контекст отрисовки Avalonia.</param>
    /// <param name="pen">Перо для отрисовки контура.</param>
    void DrawStroke(DrawingContext context, Pen pen);
    
    /// <summary>
    /// Отрисовывает заливку фигуры с заданной кистью.
    /// </summary>
    /// <param name="context">Контекст отрисовки Avalonia.</param>
    /// <param name="brush">Кисть для заливки фигуры.</param>
    void DrawFill(DrawingContext context, IBrush brush);
    
    /// <summary>
    /// Получает геометрию фигуры для обработки событий попадания указателя (hit-testing).
    /// </summary>
    /// <returns>Геометрия для хит-тестинга или null, если не поддерживается.</returns>
    // Geometry? GetHitTestGeometry();
}