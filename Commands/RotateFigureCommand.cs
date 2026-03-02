// Commands/RotateFigureCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;


public class RotateFigureCommand : FigureCommandBase
{
    public List<Guid> FigureIds { get; }
    public double Angle { get; }
    
    public RotateFigureCommand(List<Guid> figureIds, double angle)
    {
        FigureIds = figureIds;
        Angle = angle;
    }
    public override string Description => $"Поворот на {Angle}°";
    
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