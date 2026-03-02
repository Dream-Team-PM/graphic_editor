using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using graphic_editor.Models;

namespace graphic_editor.Interfaces;

public interface IToolStrategyFactory
{
    IDrawingStrategy GetStrategy(DrawingTool tool);
    bool IsSupported(DrawingTool tool);
}