// Commands/RotateFigureCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

/// <summary>
/// Команда поворота одной или нескольких фигур на заданный угол.
/// </summary>
public class RotateFigureCommand : FigureCommandBase
{
    /// <summary>
    /// Список идентификаторов поворачиваемых фигур.
    /// </summary>
    public List<Guid> FigureIds { get; }
    
    /// <summary>
    /// Угол поворота в градусах (положительный = по часовой стрелке).
    /// </summary>
    public double Angle { get; }
    
    /// <summary>
    /// Инициализирует новый экземпляр команды поворота.
    /// </summary>
    /// <param name="figureIds">Список идентификаторов фигур для поворота.</param>
    /// <param name="angle">Угол поворота в градусах.</param>
    public RotateFigureCommand(List<Guid> figureIds, double angle)
    {
        FigureIds = figureIds;
        Angle = angle;
    }
    
    /// <inheritdoc/>
    public override string Description => $"Поворот на {Angle}°";
    
    /// <summary>
    /// Выполняет команду: поворачивает указанные фигуры на заданный угол вокруг их центра.
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
                figure.Rotate(Angle);
                CaptureAfter(figure);
            }
        }
    }
}