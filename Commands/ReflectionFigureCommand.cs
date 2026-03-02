// Commands/ReflectionFigureCommand.cs
using graphic_editor.Geometry;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

public enum ReflectionType { Horizontal, Vertical }

public class ReflectionFigureCommand : FigureCommandBase
{
    public List<Guid> FigureIds { get; }
    public ReflectionType Type { get; }
    
    public ReflectionFigureCommand(List<Guid> figureIds, ReflectionType type)
    {
        FigureIds = figureIds;
        Type = type;
    }
    public override string Description => $"Отражение: {Type}";
    
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