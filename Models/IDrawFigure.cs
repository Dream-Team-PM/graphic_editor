// Models/IDrawable.cs
using Avalonia.Media;

namespace graphic_editor.Models;

/// <summary>
/// Публичный интерфейс отрисовки фигуры (не реализован и пока не используется).
/// </summary>
public interface IDrawFigure {
	/// <summary>Отрисовка контура</summary>
    void DrawStroke(DrawingContext context, Pen pen);
    
    /// <summary>Отрисовка заливки</summary>
    void DrawFill(DrawingContext context, IBrush brush);
    
    /// <summary>Получение геометрии для хит-тестинга</summary>
    //Geometry? GetHitTestGeometry();
}