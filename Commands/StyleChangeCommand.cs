// Commands/StyleChangeCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using System.Drawing;

namespace graphic_editor.Commands;


public class StyleChangeCommand : FigureCommandBase
{
    public List<Guid> FigureIds { get; }
    public Color? NewLineColor { get; }
    public Color? NewFillColor { get; }
    public double? NewThickness { get; }
    public double? NewOpacity { get; }
    
    public StyleChangeCommand(List<Guid> figureIds, Color? newLineColor, Color? newFillColor, double? newThickness, double? newOpacity)
    {
        FigureIds = figureIds;
        NewLineColor = newLineColor;
        NewFillColor = newFillColor;
        NewThickness = newThickness;
        NewOpacity = newOpacity;
    }
    
    public override string Description => "Изменение стиля";
    
    public override void Execute(CanvasViewModel canvas)
    {
        this.canvas = canvas;
        foreach (var id in FigureIds)
        {
            var figure = FindFigure(canvas, id);
            if (figure != null)
            {
                CaptureBefore(figure);
                
                if (NewLineColor.HasValue) figure.LineColor = NewLineColor.Value;
                if (NewFillColor.HasValue) figure.FillColor = NewFillColor.Value;
                if (NewThickness.HasValue) figure.Thickness = NewThickness.Value;
                if (NewOpacity.HasValue) figure.Opacity = NewOpacity.Value;
                
                CaptureAfter(figure);
            }
        }
    }
}