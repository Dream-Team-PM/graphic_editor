using graphic_editor.Models;
using graphic_editor.Geometry;
using System.Drawing;

namespace graphic_editor.Interfaces;

/// <summary>
/// Базовый интерфейс для всех фигур в редакторе.
/// Определяет доступ к вершинам фигуры для геометрических операций.
/// </summary>
public interface IFigure
{
    /// <summary>
    /// Коллекция вершин, определяющих геометрию фигуры.
    /// </summary>
    IEnumerable<Point2D> Vertices { get; }
}

/// <summary>
/// Интерфейс для фигур, поддерживающих геометрические трансформации.
/// Включает операции вращения, масштабирования, перемещения и отражения.
/// </summary>
public interface ITransformable
{
    /// <summary>
    /// Получает центральную точку фигуры для выполнения трансформаций относительно центра.
    /// </summary>
    Point2D Center { get; }
    
    /// <summary>
    /// Вращает фигуру на заданный угол относительно её центра.
    /// </summary>
    /// <param name="angle">Угол вращения в градусах (положительный — по часовой стрелке).</param>
    void Rotate(double angle);
    
    /// <summary>
    /// Масштабирует фигуру относительно центра с заданными коэффициентами по осям.
    /// </summary>
    /// <param name="sx">Коэффициент масштабирования по оси X.</param>
    /// <param name="sy">Коэффициент масштабирования по оси Y.</param>
    void Scale(double sx, double sy);
    
    /// <summary>
    /// Перемещает фигуру на заданный вектор смещения.
    /// </summary>
    /// <param name="dx">Смещение по оси X.</param>
    /// <param name="dy">Смещение по оси Y.</param>
    void Move(double dx, double dy);
    
    /// <summary>
    /// Выполняет радиальное (равномерное) масштабирование фигуры.
    /// </summary>
    /// <param name="scale">Коэффициент масштабирования (применяется к обеим осям).</param>
    void RadialScale(double scale) => Scale(scale, scale);
    
    /// <summary>
    /// Выполняет отражение фигуры относительно прямой, заданной двумя точками.
    /// </summary>
    /// <param name="a">Первая точка, определяющая ось отражения.</param>
    /// <param name="b">Вторая точка, определяющая ось отражения.</param>
    void Reflection(Point2D a, Point2D b);
}

/// <summary>
/// Интерфейс для фигур, поддерживающих выделение и проверку попадания указателя.
/// Используется для взаимодействия с пользователем и обработки событий мыши.
/// </summary>
public interface ISelectable
{
    /// <summary>
    /// Проверяет, находится ли заданная точка внутри фигуры или на её контуре.
    /// </summary>
    /// <param name="point">Проверяемая точка в координатах канваса.</param>
    /// <param name="eps">Допустимая погрешность для проверки попадания на контур.</param>
    /// <returns>True, если точка принадлежит фигуре; иначе false.</returns>
    bool IsIn(Point2D point, double eps = 0.001);
    
    /// <summary>
    /// Проверяет пересечение фигуры с заданной прямоугольной областью.
    /// Используется для выделения областью (marquee selection).
    /// </summary>
    /// <param name="leftTop">Левый верхний угол выделяющей области.</param>
    /// <param name="rightBottom">Правый нижний угол выделяющей области.</param>
    /// <returns>True, если фигура пересекается с областью; иначе false.</returns>
    bool HasIntersection(Point2D leftTop, Point2D rightBottom);
    
    /// <summary>
    /// Вычисляет ограничивающий прямоугольник (bounding box) фигуры.
    /// </summary>
    /// <returns>Кортеж с координатами границ: (MinX, MaxX, MinY, MaxY).</returns>
    (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox();
}

/// <summary>
/// Интерфейс для фигур, поддерживающих операцию клонирования.
/// Позволяет создавать независимые копии фигур для дублирования и отмены действий.
/// </summary>
public interface ICloneableFigure
{
    /// <summary>
    /// Создаёт глубокую копию фигуры с теми же свойствами и геометрией.
    /// </summary>
    /// <returns>Новый экземпляр фигуры, идентичный исходному.</returns>
    IFigure Clone();
}

/// <summary>
/// Интерфейс для фигур, предоставляющих данные для визуального рендеринга.
/// Используется контролами отрисовки для получения визуальных параметров фигуры.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Получает коллекцию вершин, используемых для отрисовки фигуры.
    /// Может отличаться от геометрических вершин (например, для сглаживания).
    /// </summary>
    /// <returns>Перечисление точек для рендеринга.</returns>
    IEnumerable<Point2D> GetRenderVertices();
    
    /// <summary>
    /// Получает цвет контура фигуры в формате System.Drawing.Color.
    /// </summary>
    System.Drawing.Color LineColor { get; }
    
    /// <summary>
    /// Получает цвет заливки фигуры в формате System.Drawing.Color.
    /// </summary>
    System.Drawing.Color FillColor { get; }
    
    /// <summary>
    /// Получает толщину контура фигуры в пикселях.
    /// </summary>
    double Thickness { get; }
    
    /// <summary>
    /// Получает коэффициент непрозрачности фигуры (от 0.0 до 1.0).
    /// </summary>
    double Opacity { get; }
}