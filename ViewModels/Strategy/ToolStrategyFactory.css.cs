// ViewModels/Tools/ToolStrategyFactory.cs

using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;
using graphic_editor.State;
using graphic_editor.Models;

namespace graphic_editor.Tools;
public class ToolStrategyFactory : IToolStrategyFactory
{
    private readonly Dictionary<DrawingTool, IDrawingStrategy> _strategies;
    
    public ToolStrategyFactory(StyleSettings defaultStyle)
    {
        _strategies = new()
        {
            // Прямоугольные примитивы
            { DrawingTool.Rectangle, new RectangleStrategy() },
            { DrawingTool.Square, new SquareStrategy() },
            
            // Эллиптические примитивы
            { DrawingTool.Ellipse, new EllipseStrategy(defaultStyle) },
            { DrawingTool.Circle, new CircleStrategy(defaultStyle) },
            
            // Линия
            { DrawingTool.Line, new LineStrategy() },
            
            // Многоугольники
            { DrawingTool.Pentagon, new PentagonStrategy() },
            { DrawingTool.Hexagon, new HexagonStrategy() },
            { DrawingTool.Heptagon, new HeptagonStrategy() },
            { DrawingTool.Octagon, new OctagonStrategy() },
            
            // Треугольник и пентаграмма — отдельные
            { DrawingTool.Triangle, new TriangleStrategy() },
            { DrawingTool.Pentagram, new PentagramStrategy() },
            
            // Перо
            { DrawingTool.Pen, new PenStrategy() },
        };
    }
    
    public IDrawingStrategy GetStrategy(DrawingTool tool) =>
        _strategies.TryGetValue(tool, out var strategy) 
            ? strategy 
            : throw new NotSupportedException($"Strategy for {tool} not registered");
    
    public bool IsSupported(DrawingTool tool) => _strategies.ContainsKey(tool);
}