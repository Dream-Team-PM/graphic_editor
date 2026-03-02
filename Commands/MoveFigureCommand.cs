// Commands/MoveFigureCommand.cs
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

public class MoveFigureCommand : FigureCommandBase
{
    public List<Guid> FigureIds { get; }
    public double Dx { get; }
    public double Dy { get; }
    
    public MoveFigureCommand(List<Guid> figureIds, double dx, double dy)
    {
        FigureIds = figureIds;
        Dx = dx;
        Dy = dy;
    }
    public override string Description => $"Перемещение на ({Dx}, {Dy})";
    
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