using graphic_editor.Models;
namespace graphic_editor;

public interface IPolygon : IGraphicFigure, IFigure
{
    IReadOnlyList<Point_1> Vertices { get; }

}