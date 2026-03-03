// Interfaces/IFigure.cs

using graphic_editor.Models;
using graphic_editor.Geometry;
using System.Drawing;

namespace graphic_editor.Interfaces;
/// <summary>
/// Публичный интерфейс фигуры.
/// </summary>
public interface IFigure
{
    IEnumerable<Point2D> Vertices { get; } /// <summary>Точка-вершина.</summary>
}

/// <summary>Фигура, поддерживающая геометрические трансформации.</summary>
public interface ITransformable
{
    Point2D Center { get; } /// <summary>Метод центрирования фигуры.</summary>
    void Rotate(double angle); /// <summary>Функция вращения фигуры на определённый угол.</summary>
    void Scale(double sx, double sy); /// <summary>Функция мастабирования фигуры.</summary>
    void Move(double dx, double dy); /// <summary>Функция перемещения фигуры.</summary>
    void RadialScale(double scale) => Scale(scale, scale); /// <summary>Функция радиального мастабирования фигуры.</summary>
    
    void Reflection(Point2D a, Point2D b); /// <summary>Функция рефлексирования фигуры.</summary>
}


/// <summary>Фигура, поддерживающая выделение и хит-тестинг.</summary>
public interface ISelectable
{
    bool IsIn(Point2D point, double eps = 0.001); /// <summary>Функция проверки нахождения в фигуре с заданной точностью.</summary>
    bool HasIntersection(Point2D leftTop, Point2D rightBottom); /// <summary>Функция проверки пересечения фигур.</summary>
    (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox();
}

/// <summary>Фигура, поддерживающая клонирование.</summary>
public interface ICloneableFigure
{
    IFigure Clone(); /// <summary>Клонирование фигуры.</summary>
}

/// <summary>Фигура, предоставляющая данные для отрисовки.</summary>
public interface IRenderable
{
    IEnumerable<Point2D> GetRenderVertices();
    System.Drawing.Color LineColor { get; }
    System.Drawing.Color FillColor { get; }
    double Thickness { get; }
    double Opacity { get; }
}