// Models/DrawingTools.cs

namespace graphic_editor.Models;

/// <summary>Типы инструментов рисования</summary>
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

/// <summary>Extension-методы для DrawingTool</summary>
public static class DrawingToolExtensions
{
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

    private static readonly Dictionary<string, DrawingTool> _parseMap = 
        _displayNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    /// <summary>Получить отображаемое имя инструмента</summary>
    public static string ToDisplayName(this DrawingTool tool) => 
        _displayNames.TryGetValue(tool, out var name) ? name : tool.ToString();

    /// <summary>Распарсить строку в DrawingTool (case-insensitive)</summary>
    public static bool TryParse(string displayName, out DrawingTool tool) => 
        _parseMap.TryGetValue(displayName, out tool);

    /// <summary>Является ли инструмент примитивом для рисования мышью</summary>
    public static bool IsPrimitive(this DrawingTool tool) => 
        tool is DrawingTool.Rectangle or DrawingTool.Ellipse or DrawingTool.Line or DrawingTool.Square or DrawingTool.Circle or DrawingTool.Pentagon or DrawingTool.Hexagon 
        or DrawingTool.Heptagon or DrawingTool.Octagon or DrawingTool.Pentagram or DrawingTool.Triangle;

    /// <summary>Требует ли инструмент режима рисования (drag-to-create)</summary>
    public static bool RequiresDrawingMode(this DrawingTool tool) => 
        tool.IsPrimitive() || tool == DrawingTool.Pen;
}