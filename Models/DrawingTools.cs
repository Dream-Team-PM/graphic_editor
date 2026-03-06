// Models/DrawingTools.cs

using DynamicData.Diagnostics;

namespace graphic_editor.Models;

/// <summary>
/// Перечисление типов инструментов рисования в графическом редакторе.
/// </summary>
public enum DrawingTool
{
    Select,        // "Выделение"
    Rectangle,     // "Прямоугольник"
    Square,        // "Квадрат"
    Ellipse,       // "Эллипс"
    Circle,        // "Круг"
    Line,          // "Линия"
    Polygon,       // "Многоугольник"
    Pentagon,      // "Пятиугольник"
    Hexagon,       // "Шестиугольник"
    Heptagon,      // "Семиугольник"
    Octagon,       // "Восьмиугольник"
    Pentagram,     // "Пентаграмма"
    Triangle,      // "Треугольник"
    Pen,           // "Перо"
    Text,          // "Текст"
    Hand,          // "Рука"
    Zoom           // "Масштаб"
}

/// <summary>
/// Extension-методы для работы с перечислением <see cref="DrawingTool"/>.
/// </summary>
public static class DrawingToolExtensions
{
    /// <summary>
    /// Словарь для отображения инструментов в человеко-читаемые имена.
    /// </summary>
    private static readonly Dictionary<DrawingTool, string> _displayNames = new()
    {
        { DrawingTool.Select, "Выделение" },
        { DrawingTool.Rectangle, "Прямоугольник" },
        { DrawingTool.Square, "Квадрат" },
        { DrawingTool.Ellipse, "Эллипс" },
        { DrawingTool.Circle, "Круг" },
        { DrawingTool.Pentagon, "Пятиугольник" },
        { DrawingTool.Hexagon, "Шестиугольник" },
        { DrawingTool.Heptagon, "Семиугольник" },
        { DrawingTool.Octagon, "Восьмиугольник" },
        { DrawingTool.Pentagram, "Пентаграмма" },
        { DrawingTool.Triangle, "Треугольник" },
        { DrawingTool.Line, "Линия" },
        { DrawingTool.Polygon, "Многоугольник" },
        { DrawingTool.Pen, "Перо" },
        { DrawingTool.Text, "Текст" },
        { DrawingTool.Hand, "Рука" },
        { DrawingTool.Zoom, "Масштаб" }
    };

    /// <summary>
    /// Обратный словарь для парсинга строковых имён в <see cref="DrawingTool"/>.
    /// </summary>
    private static readonly Dictionary<string, DrawingTool> _parseMap = 
        _displayNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    /// <summary>
    /// Возвращает отображаемое (локализованное) имя инструмента.
    /// </summary>
    /// <param name="tool">Экземпляр <see cref="DrawingTool"/>.</param>
    /// <returns>Человекочитаемое имя инструмента на русском языке.</returns>
    public static string ToDisplayName(this DrawingTool tool) => 
        _displayNames.TryGetValue(tool, out var name) ? name : tool.ToString();

    /// <summary>
    /// Пытается распарсить строковое имя инструмента в значение <see cref="DrawingTool"/>.
    /// </summary>
    /// <param name="displayName">Отображаемое имя инструмента (case-insensitive).</param>
    /// <param name="tool">Выходной параметр: найденное значение <see cref="DrawingTool"/>.</param>
    /// <returns>
    /// <see langword="true"/>, если парсинг успешен; иначе <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string displayName, out DrawingTool tool) => 
        _parseMap.TryGetValue(displayName, out tool);

    /// <summary>
    /// Определяет, является ли инструмент геометрическим примитивом, рисуемым мышью.
    /// </summary>
    /// <param name="tool">Экземпляр <see cref="DrawingTool"/>.</param>
    /// <returns>
    /// <see langword="true"/>, если инструмент является примитивом (прямоугольник, эллипс, линия и т.д.);
    /// иначе <see langword="false"/>.
    /// </returns>
    public static bool IsPrimitive(this DrawingTool tool) => 
        tool is DrawingTool.Rectangle or DrawingTool.Ellipse or DrawingTool.Line or DrawingTool.Square or DrawingTool.Circle or DrawingTool.Pentagon or DrawingTool.Hexagon 
        or DrawingTool.Heptagon or DrawingTool.Octagon or DrawingTool.Pentagram or DrawingTool.Triangle;

    /// <summary>
    /// Определяет, требует ли инструмент режима рисования "перетаскиванием" (drag-to-create).
    /// </summary>
    /// <param name="tool">Экземпляр <see cref="DrawingTool"/>.</param>
    /// <returns>
    /// <see langword="true"/>, если инструмент требует режима рисования (примитивы или перо);
    /// иначе <see langword="false"/>.
    /// </returns>
    public static bool RequiresDrawingMode(this DrawingTool tool) => 
        tool.IsPrimitive() || tool == DrawingTool.Pen;


}