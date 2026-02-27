using graphic_editor.Models;

/// <summary>
/// Публичный интерфейс фигуры.
/// </summary>
public interface IFigure
{
    // Основные свойства
    Point_1 Center { get; } /// <summary>Метод центрирования фигуры.</summary>
    IEnumerable<Point_1> Vertices { get; } /// <summary>Точка-вершина.</summary>

    // Трансформации
    void Rotate(double angle); /// <summary>Функция вращения фигуры на определённый угол.</summary>
    void Scale(double sx, double sy); /// <summary>Функция мастабирования фигуры.</summary>
    void RadialScale(double sx); /// <summary>Функция радиального мастабирования фигуры.</summary>
    void Reflection(Point_1 a, Point_1 b); /// <summary>Функция рефлексирования фигуры.</summary>
    void Move(double dx,double dy); /// <summary>Функция перемещения фигуры.</summary>

    // Проверки
    bool IsIn(Point_1 point, double eps); /// <summary>Функция проверки нахождения в фигуре с заданной точностью.</summary>
    bool HasIntersection(Point_1 lefttop, Point_1 rightbottom); /// <summary>Функция проверки пересечения фигур.</summary>
    //IFigure Intersection(IFigure figure); /// <summary>Пересечение фигур.</summary>
    
    // Клонирование
    IFigure Clone();
    //IEnumerable<IDrawFigure> Draw(); /// <summary>Отрисовка фигуры.</summary>
}