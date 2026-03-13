// Commands/ReflectionFigureCommand.cs
using graphic_editor.Geometry;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

/// <summary>
/// Тип отражения фигуры: по горизонтали или вертикали.
/// </summary>
public enum ReflectionType { Horizontal, Vertical }

/// <summary>
/// Команда отражения одной или нескольких фигур относительно центра ограничивающего прямоугольника.
/// </summary>
public class ReflectionFigureCommand : FigureCommandBase
{
    /// <summary>
    /// Список идентификаторов отражаемых фигур.
    /// </summary>
    public List<Guid> FigureIds { get; }
    
    /// <summary>
    /// Тип отражения: Horizontal (по вертикальной оси) или Vertical (по горизонтальной оси).
    /// </summary>
    public ReflectionType Type { get; }
    
    /// <summary>
    /// Инициализирует новый экземпляр команды отражения.
    /// </summary>
    /// <param name="figureIds">Список идентификаторов фигур для отражения.</param>
    /// <param name="type">Тип отражения.</param>
    public ReflectionFigureCommand(List<Guid> figureIds, ReflectionType type)
    {
        FigureIds = figureIds;
        Type = type;
    }
    
    /// <inheritdoc/>
    public override string Description => $"Отражение: {Type}";
    
    /// <summary>
    /// Выполняет команду: отражает указанные фигуры относительно их центра.
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
                
                var bbox = figure.GetBoundingBox();
                var center = new Point2D(
                    (bbox.MinX + bbox.MaxX) / 2,
                    (bbox.MinY + bbox.MaxY) / 2);
                
                if (Type == ReflectionType.Horizontal)
                {
                    // Отражение по вертикальной оси (меняем X)
                    foreach (var vertex in figure.Vertices)
                    {
                        vertex.X = center.X * 2 - vertex.X;
                    }
                }
                else
                {
                    // Отражение по горизонтальной оси (меняем Y)
                    foreach (var vertex in figure.Vertices)
                    {
                        vertex.Y = center.Y * 2 - vertex.Y;
                    }
                }
                CaptureAfter(figure);
                figure.NotifyPropertyChanged();
            }
        }
    }
}