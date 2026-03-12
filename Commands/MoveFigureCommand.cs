// Commands/MoveFigureCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

/// <summary>
/// Команда перемещения одной или нескольких фигур на заданный вектор.
/// </summary>
public class MoveFigureCommand : FigureCommandBase
{
    /// <summary>
    /// Список идентификаторов перемещаемых фигур.
    /// </summary>
    public List<Guid> FigureIds { get; }
    
    /// <summary>
    /// Смещение по оси X.
    /// </summary>
    public double Dx { get; }
    
    /// <summary>
    /// Смещение по оси Y.
    /// </summary>
    public double Dy { get; }
    
    /// <summary>
    /// Инициализирует новый экземпляр команды перемещения.
    /// </summary>
    /// <param name="figureIds">Список идентификаторов фигур для перемещения.</param>
    /// <param name="dx">Смещение по оси X.</param>
    /// <param name="dy">Смещение по оси Y.</param>
    public MoveFigureCommand(List<Guid> figureIds, double dx, double dy)
    {
        FigureIds = figureIds;
        Dx = dx;
        Dy = dy;
    }
    /// <inheritdoc/>
    public override string Description => $"Перемещение на ({Dx}, {Dy})";
    
    /// <summary>
    /// Выполняет команду: перемещает указанные фигуры на заданный вектор.
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
                figure.Move(Dx, Dy);
                CaptureAfter(figure);
            }
        }
    }
}