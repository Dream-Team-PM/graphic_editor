// Controls/Rendering/IFigureRenderStrategy.cs
using Avalonia.Controls;
using graphic_editor.Controls;
using graphic_editor.ViewModels;

namespace graphic_editor.Controls.Rendering;

/// <summary>Стратегия создания визуального представления для фигуры.</summary>
public interface IFigureRenderStrategy
{
    /// <summary>Создать Control для отображения фигуры.</summary>
    Control? CreateControl(FigureViewModel figure);
    
    /// <summary>Тип фигуры, который обрабатывает стратегия.</summary>
    Type SupportedFigureType { get; }
}