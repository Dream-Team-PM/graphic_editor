
namespace graphic_editor;
public interface IPolygon : IGraphicFigure, IFigure
{
    IReadOnlyList<Point> Vertices { get; }

}