using System.Drawing;

namespace graphic_editor;


public interface IGraphicFigure
{
    Color LineColor { get; }
    Color FillColor { get; }
    double Thickness { get; }
}

public interface IDrawFigure { }






