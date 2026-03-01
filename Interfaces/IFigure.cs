// Interfaces/IFigure.cs

using graphic_editor.Models;
using graphic_editor.Geometry;

namespace graphic_editor.Interfaces;
/// <summary>
/// Публичный интерфейс фигуры.
/// </summary>
public interface IFigure
{
    // Основные свойства
    Point2D Center { get; } /// <summary>Метод центрирования фигуры.</summary>
    IEnumerable<Point2D> Vertices { get; } /// <summary>Точка-вершина.</summary>

    // Трансформации
    void Rotate(double angle); /// <summary>Функция вращения фигуры на определённый угол.</summary>
    void Scale(double sx, double sy); /// <summary>Функция мастабирования фигуры.</summary>
    void RadialScale(double sx); /// <summary>Функция радиального мастабирования фигуры.</summary>
    void Reflection(Point2D a, Point2D b); /// <summary>Функция рефлексирования фигуры.</summary>
    void Move(double dx,double dy); /// <summary>Функция перемещения фигуры.</summary>

    // Проверки
    bool IsIn(Point2D point, double eps); /// <summary>Функция проверки нахождения в фигуре с заданной точностью.</summary>
    bool HasIntersection(Point2D lefttop, Point2D rightbottom); /// <summary>Функция проверки пересечения фигур.</summary>
    //IFigure Intersection(IFigure figure); /// <summary>Пересечение фигур.</summary>
    
    // Клонирование
    IFigure Clone();
    //IEnumerable<IDrawFigure> Draw(); /// <summary>Отрисовка фигуры.</summary>
}