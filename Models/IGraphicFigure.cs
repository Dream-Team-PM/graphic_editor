using System.Drawing;

/// <summary>
/// Публичный интерфейс графической фигуры.
/// </summary>
public interface IGraphicFigure
{
    Color LineColor { get; } /// <summary>Свойство цвета для линии.</summary>
    Color FillColor { get; } /// <summary>Свойство цвета для заполнения фигуры.</summary>
    double Thickness { get; } /// <summary>Свойство толщины линии.</summary>
}