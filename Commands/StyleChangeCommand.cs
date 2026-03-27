// Commands/StyleChangeCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using graphic_editor.Geometry;
using System.Drawing;

namespace graphic_editor.Commands;

/// <summary>
/// Команда изменения стиля одной или нескольких фигур (цвет, толщина, непрозрачность).
/// </summary>
public class StyleChangeCommand : FigureCommandBase
{
    /// <summary>
    /// Список идентификаторов фигур для изменения стиля.
    /// </summary>
    public List<Guid> FigureIds { get; }
    
    /// <summary>
    /// Новый цвет обводки (null = не изменять).
    /// </summary>
    public System.Drawing.Color? NewLineColor { get; }
    
    /// <summary>
    /// Новый цвет заливки (null = не изменять).
    /// </summary>
    public System.Drawing.Color? NewFillColor { get; }
    
    /// <summary>
    /// Новая толщина обводки (null = не изменять).
    /// </summary>
    public double? NewThickness { get; }
    
    /// <summary>
    /// Новая непрозрачность (null = не изменять).
    /// </summary>
    public double? NewOpacity { get; }
    
    /// <summary>
    /// Инициализирует новый экземпляр команды изменения стиля.
    /// </summary>
    /// <param name="figureIds">Список идентификаторов фигур.</param>
    /// <param name="newLineColor">Новый цвет обводки или null.</param>
    /// <param name="newFillColor">Новый цвет заливки или null.</param>
    /// <param name="newThickness">Новая толщина обводки или null.</param>
    /// <param name="newOpacity">Новая непрозрачность или null.</param>
    public StyleChangeCommand(
        List<Guid> figureIds, 
        System.Drawing.Color? newLineColor, 
        System.Drawing.Color? newFillColor, 
        double? newThickness, 
        double? newOpacity)
    {
        FigureIds = figureIds;
        NewLineColor = newLineColor;
        NewFillColor = newFillColor;
        NewThickness = newThickness;
        NewOpacity = newOpacity;
    }
    /// <inheritdoc/>
    public override string Description => "Изменение стиля";
    
    /// <summary>
    /// Выполняет команду: применяет изменения стиля к указанным фигурам.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для выполнения операции.</param>
    public override void Execute(CanvasViewModel canvas)
    {
        this.canvas = canvas;
        foreach (var id in FigureIds)
        {
            var figure = FindFigure(canvas, id);
            if (figure != null)
            {
                CaptureBefore(figure);
                
                if (figure is GroupViewModel group)
                {
                    group.ApplyToAllChildren(child =>
                    {
                        if (NewLineColor.HasValue) child.LineColor = NewLineColor.Value;
                        if (NewFillColor.HasValue) child.FillColor = NewFillColor.Value;
                        if (NewThickness.HasValue) child.Thickness = NewThickness.Value;
                        if (NewOpacity.HasValue) child.Opacity = NewOpacity.Value;
                    });
                }
                else
                {
                    if (NewLineColor.HasValue) figure.LineColor = NewLineColor.Value;
                    if (NewFillColor.HasValue) figure.FillColor = NewFillColor.Value;
                    if (NewThickness.HasValue) figure.Thickness = NewThickness.Value;
                    if (NewOpacity.HasValue) figure.Opacity = NewOpacity.Value;
                }
                CaptureAfter(figure);
                figure.NotifyPropertyChanged();
            }
        }
    }
}