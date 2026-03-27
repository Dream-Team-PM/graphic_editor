using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;

namespace graphic_editor.IO.ProjectFormat;

/// <summary>
/// Сохранение и загрузка проекта в формате SVG.
/// Совместим с Inkscape, Illustrator, браузерами.
/// </summary>
public class SvgProjectFormat : IProjectFormat
{
    public string FileExtension => ".svg";

    // ── Сохранение ──────────────────────────────────────────────────────────

    public async Task SaveAsync(string fullPath, CanvasViewModel canvas)
    {
        double maxX = 800, maxY = 600;
        foreach (var layer in canvas.Layers)
            foreach (var f in layer.Figures)
                UpdateBounds(f, ref maxX, ref maxY);

        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape" width="{I(maxX)}" height="{I(maxY)}" viewBox="0 0 {I(maxX)} {I(maxY)}">""");
        sb.AppendLine("  <title>INKognida project</title>");

        int figId = 1;
        int layerIndex = 1;
        foreach (var layer in canvas.Layers)
        {
            if (!layer.IsVisible) continue;
            sb.AppendLine($"""  <g id="layer{layerIndex}" inkscape:label="{Esc(layer.Name)}" inkscape:groupmode="layer" data-id="{layer.Id}">""");
            foreach (var figure in layer.Figures)
                AppendFigure(sb, figure, "    ", ref figId);
            sb.AppendLine("  </g>");
            layerIndex++;
        }

        sb.AppendLine("</svg>");
        await File.WriteAllTextAsync(fullPath, sb.ToString());
    }

    // ── Загрузка ─────────────────────────────────────────────────────────────

    public async Task LoadAsync(string fullPath, CanvasViewModel canvas)
    {
        var xml = await File.ReadAllTextAsync(fullPath);
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidDataException("Пустой SVG файл");

        canvas.Layers.Clear();
        var cssClasses = ParseEmbeddedCss(root);

        var topLevel = root.Elements()
            .Where(e => e.Name.LocalName != "defs" && e.Name.LocalName != "title")
            .ToList();

        var groups = topLevel.Where(e => e.Name.LocalName == "g").ToList();

        if (groups.Count == 0)
        {
            var layer = new LayerViewModel("Слой 1");
            foreach (var el in topLevel)
            {
                var fig = ParseElement(el, cssClasses);
                if (fig != null) layer.Figures.Add(fig);
            }
            canvas.Layers.Add(layer);
        }
        else
        {
            var inkNs = XNamespace.Get("http://www.inkscape.org/namespaces/inkscape");
            foreach (var g in groups)
            {
                var name = g.Attribute(inkNs + "label")?.Value
                    ?? g.Attribute("data-name")?.Value
                    ?? g.Attribute("id")?.Value
                    ?? "Слой";

                var dataId = g.Attribute("data-id")?.Value ?? g.Attribute("id")?.Value;
                var layer = Guid.TryParse(dataId, out var gid)
                    ? new LayerViewModel(gid, name)
                    : new LayerViewModel(name);

                foreach (var el in g.Elements())
                {
                    var fig = ParseElement(el, cssClasses);
                    if (fig != null) layer.Figures.Add(fig);
                }
                canvas.Layers.Add(layer);
            }
        }

        if (canvas.Layers.Count > 0)
            canvas.ActiveLayer = canvas.Layers[0];
    }

    // ── Запись фигур ─────────────────────────────────────────────────────────

    private static void AppendFigure(StringBuilder sb, FigureViewModel figure, string indent, ref int figId)
    {
        var (fill, fillOp) = SvgColorWithOpacity(figure.FillColor, "fill");
        var (stroke, strokeOp) = SvgColorWithOpacity(figure.LineColor, "stroke");
        var sw = F(Math.Max(1.0, figure.Thickness));
        var svgOpacity = 1.0 - figure.Opacity;
        var opProp = svgOpacity < 0.9999 ? $";opacity:{F(svgOpacity)}" : "";
        var style = $"fill:{fill}{fillOp};stroke:{stroke}{strokeOp};stroke-width:{sw};stroke-linejoin:round;paint-order:markers fill stroke{opProp}";
        var tr = figure.Rotation != 0
            ? $"\n{indent}  transform=\"rotate({F(figure.Rotation)} {F(figure.Center.X)} {F(figure.Center.Y)})\""
            : "";

        if (figure is GroupViewModel grp)
        {
            var grpSvgOp = 1.0 - figure.Opacity;
            var grpStyle = grpSvgOp < 0.9999 ? $" style=\"opacity:{F(grpSvgOp)}\"" : "";
            sb.AppendLine($"{indent}<g id=\"g{figId++}\"{grpStyle}{tr}>");
            foreach (var child in grp.Children)
                AppendFigure(sb, child, indent + "  ", ref figId);
            sb.AppendLine($"{indent}</g>");
            return;
        }

        var id = figId++;
        switch (figure)
        {
            case RectangleViewModel r:
                sb.AppendLine($"{indent}<rect");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"rect{id}\"");
                sb.AppendLine($"{indent}  width=\"{F(r.Width)}\"");
                sb.AppendLine($"{indent}  height=\"{F(r.Height)}\"");
                sb.AppendLine($"{indent}  x=\"{F(r.X)}\"");
                sb.AppendLine($"{indent}  y=\"{F(r.Y)}\"{tr} />");
                break;

            case CircleViewModel c:
                sb.AppendLine($"{indent}<circle");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"circle{id}\"");
                sb.AppendLine($"{indent}  cx=\"{F(c.Center.X)}\"");
                sb.AppendLine($"{indent}  cy=\"{F(c.Center.Y)}\"");
                sb.AppendLine($"{indent}  r=\"{F(c.Radius)}\"{tr} />");
                break;

            case EllipseViewModel e:
                sb.AppendLine($"{indent}<ellipse");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"ellipse{id}\"");
                sb.AppendLine($"{indent}  cx=\"{F(e.X + e.Width / 2)}\"");
                sb.AppendLine($"{indent}  cy=\"{F(e.Y + e.Height / 2)}\"");
                sb.AppendLine($"{indent}  rx=\"{F(e.Width / 2)}\"");
                sb.AppendLine($"{indent}  ry=\"{F(e.Height / 2)}\"{tr} />");
                break;

            case LineViewModel l:
                sb.AppendLine($"{indent}<line");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"line{id}\"");
                sb.AppendLine($"{indent}  x1=\"{F(l.X1)}\"");
                sb.AppendLine($"{indent}  y1=\"{F(l.Y1)}\"");
                sb.AppendLine($"{indent}  x2=\"{F(l.X2)}\"");
                sb.AppendLine($"{indent}  y2=\"{F(l.Y2)}\"{tr} />");
                break;

            case PenPointViewModel p:
                var (dotColor, dotOp) = SvgColorWithOpacity(p.LineColor, "fill");
                var radius = Math.Max(2, p.Thickness / 2.0);
                sb.AppendLine($"{indent}<circle");
                sb.AppendLine($"{indent}  style=\"fill:{dotColor}{dotOp};stroke:none\"");
                sb.AppendLine($"{indent}  id=\"dot{id}\"");
                sb.AppendLine($"{indent}  cx=\"{F(p.X)}\"");
                sb.AppendLine($"{indent}  cy=\"{F(p.Y)}\"");
                sb.AppendLine($"{indent}  r=\"{F(radius)}\" />");
                break;

            // ✅ Полигоны: треугольник, пяти-, шести-, семи-, восьмиугольник
            case PolygonViewModel polygon when polygon is not RegularPolygonViewModel && polygon is not PentagramViewModel:
                var polyPoints = string.Join(" ", polygon.Vertices.Select(v => $"{F(v.X)},{F(v.Y)}"));
                sb.AppendLine($"{indent}<polygon");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"poly{id}\"");
                sb.AppendLine($"{indent}  points=\"{polyPoints}\"{tr} />");
                break;

            // ✅ Правильные многоугольники (пяти-, шести-, семи-, восьмиугольник)
            case RegularPolygonViewModel regular:
                var regPoints = string.Join(" ", regular.Vertices.Select(v => $"{F(v.X)},{F(v.Y)}"));
                sb.AppendLine($"{indent}<polygon");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"regpoly{id}\"");
                sb.AppendLine($"{indent}  points=\"{regPoints}\"{tr} />");
                break;

            // ✅ Пентаграмма (звезда)
            case PentagramViewModel star:
                var starPoints = string.Join(" ", star.Vertices.Select(v => $"{F(v.X)},{F(v.Y)}"));
                sb.AppendLine($"{indent}<polygon");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"star{id}\"");
                sb.AppendLine($"{indent}  points=\"{starPoints}\"{tr} />");
                break;
            
            case RhombusViewModel rhombus:
                var rhombusPoints = string.Join(" ", rhombus.Vertices.Select(v => $"{F(v.X)},{F(v.Y)}"));
                sb.AppendLine($"{indent}<polygon");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"rhombus{id}\"");
                sb.AppendLine($"{indent}  points=\"{rhombusPoints}\"{tr} />");
                break;
            
            case RightTriangleViewModel rt:
                var rtPoints = string.Join(" ", rt.Vertices.Take(3).Select(v => $"{F(v.X)},{F(v.Y)}"));
                sb.AppendLine($"{indent}<polygon");
                sb.AppendLine($"{indent}  style=\"{style}\"");
                sb.AppendLine($"{indent}  id=\"righttri{id}\"");
                sb.AppendLine($"{indent}  points=\"{rtPoints}\"{tr} />");
                break;

            default:
                sb.AppendLine($"{indent}<!-- {Esc(figure.GetType().Name)} -->");
                break;
        }
    }

    // ── Обновление границ для viewBox ───────────────────────────────────────

    private static void UpdateBounds(FigureViewModel f, ref double maxX, ref double maxY)
    {
        switch (f)
        {
            case RectangleViewModel rr:
                maxX = Math.Max(maxX, rr.X + rr.Width + 20);
                maxY = Math.Max(maxY, rr.Y + rr.Height + 20);
                break;
            case CircleViewModel c:
                maxX = Math.Max(maxX, c.Center.X + c.Radius + 20);
                maxY = Math.Max(maxY, c.Center.Y + c.Radius + 20);
                break;
            case EllipseViewModel e:
                maxX = Math.Max(maxX, e.X + e.Width + 20);
                maxY = Math.Max(maxY, e.Y + e.Height + 20);
                break;
            case LineViewModel l:
                maxX = Math.Max(maxX, Math.Max(l.X1, l.X2) + 20);
                maxY = Math.Max(maxY, Math.Max(l.Y1, l.Y2) + 20);
                break;
            case PenPointViewModel p:
                var r = Math.Max(2, p.Thickness / 2.0);
                maxX = Math.Max(maxX, p.X + r + 20);
                maxY = Math.Max(maxY, p.Y + r + 20);
                break;
            case PolygonViewModel poly:
                foreach (var v in poly.Vertices)
                {
                    maxX = Math.Max(maxX, v.X + 20);
                    maxY = Math.Max(maxY, v.Y + 20);
                }
                break;
            case GroupViewModel g:
                foreach (var child in g.Children)
                    UpdateBounds(child, ref maxX, ref maxY);
                break;
        }
    }

    // ── Парсинг фигур ────────────────────────────────────────────────────────

    private static FigureViewModel? ParseElement(XElement el,
        Dictionary<string, string>? cssClasses = null)
    {
        cssClasses ??= new Dictionary<string, string>();

        var inlineCss = ParseStyle(el.Attribute("style")?.Value);
        var classCss = ResolveClassStyle(el.Attribute("class")?.Value, cssClasses);

        string? Get(string prop) =>
            inlineCss.GetValueOrDefault(prop)
            ?? classCss.GetValueOrDefault(prop)
            ?? el.Attribute(prop)?.Value;

        var fill = ParseColorWithOpacity(Get("fill"), Get("fill-opacity"));
        var stroke = ParseColorWithOpacity(Get("stroke"), Get("stroke-opacity"));
        var sw = ParseDouble(Get("stroke-width"), 1.0);
        var opacity = 1.0 - ParseDouble(Get("opacity"), 1.0);

        return el.Name.LocalName switch
        {
            "rect" => new RectangleViewModel(
                ParseDouble(el.Attribute("x")?.Value, 0),
                ParseDouble(el.Attribute("y")?.Value, 0),
                ParseDouble(el.Attribute("width")?.Value, 100),
                ParseDouble(el.Attribute("height")?.Value, 100),
                stroke, sw, fill, opacity),

            "circle" => new CircleViewModel(
                ParseDouble(el.Attribute("cx")?.Value, 50),
                ParseDouble(el.Attribute("cy")?.Value, 50),
                ParseDouble(el.Attribute("r")?.Value, 50),
                stroke, sw, fill, opacity),

            "ellipse" => new EllipseViewModel(
                ParseDouble(el.Attribute("cx")?.Value, 50) - ParseDouble(el.Attribute("rx")?.Value, 50),
                ParseDouble(el.Attribute("cy")?.Value, 50) - ParseDouble(el.Attribute("ry")?.Value, 50),
                ParseDouble(el.Attribute("rx")?.Value, 50) * 2,
                ParseDouble(el.Attribute("ry")?.Value, 50) * 2,
                stroke, sw, fill, opacity),

            "line" => new LineViewModel(
                ParseDouble(el.Attribute("x1")?.Value, 0),
                ParseDouble(el.Attribute("y1")?.Value, 0),
                ParseDouble(el.Attribute("x2")?.Value, 100),
                ParseDouble(el.Attribute("y2")?.Value, 100),
                stroke, sw, fill, opacity),

            "polygon" => ParsePolygon(el, fill, stroke, sw, opacity),

            "g" => new GroupViewModel(
                el.Elements()
                    .Select(e => ParseElement(e, cssClasses))
                    .Where(f => f != null)
                    .Select(f => f!)),

            "path" => null, // Игнорируем пути — их нельзя надёжно конвертировать обратно

            _ => null
        };
    }

    /// <summary>
    /// Парсит &lt;polygon&gt; и определяет тип фигуры по количеству вершин.
    /// </summary>
    private static FigureViewModel? ParsePolygon(XElement el, System.Drawing.Color fill, System.Drawing.Color stroke, double sw, double opacity)
    {
        var pointsAttr = el.Attribute("points")?.Value;
        if (string.IsNullOrEmpty(pointsAttr)) return null;

        var points = ParsePoints(pointsAttr);
        if (points.Count < 3) return null;

        // 🔍 Определяем тип по количеству вершин и геометрии
        return points.Count switch
        {
            3 when IsRightTriangle(points) => new RightTriangleViewModel(
                points.Min(p => p.X), 
                points.Min(p => p.Y),
                points.Max(p => p.X) - points.Min(p => p.X),
                points.Max(p => p.Y) - points.Min(p => p.Y),
                stroke, sw, fill, opacity),
            3 => new TriangleViewModel(points[0], points[1], points[2], stroke, sw, fill, opacity),
            4 when IsRhombus(points) => new RhombusViewModel(
                AverageCenter(points).X, AverageCenter(points).Y,
                MaxHorizontalDistance(points), MaxVerticalDistance(points),
                stroke, sw, fill, opacity),
			4 => CreateGenericPolygon(points, stroke, sw, fill, opacity),
            5 => new PentagonViewModel(AverageCenter(points), DistanceToCenter(points, 0), stroke, sw, fill, opacity),
            6 => new HexagonViewModel(AverageCenter(points), DistanceToCenter(points, 0), stroke, sw, fill, opacity),
            7 => new HeptagonViewModel(AverageCenter(points), DistanceToCenter(points, 0), stroke, sw, fill, opacity),
            8 => new OctagonViewModel(AverageCenter(points), DistanceToCenter(points, 0), stroke, sw, fill, opacity),
            10 when IsPentagram(points) => new PentagramViewModel(AverageCenter(points), DistanceToCenter(points, 0), stroke, sw, fill, opacity),
            _ => CreateGenericPolygon(points, stroke, sw, fill, opacity)
        };
    }

    /// <summary>
    /// Создаёт произвольный полигон, если тип не определён.
    /// </summary>
    private static FigureViewModel CreateGenericPolygon(List<Point2D> points, System.Drawing.Color stroke, double sw, System.Drawing.Color fill, double opacity)
    {
        // Создаём базовый PolygonViewModel через динамический вызов
        var polygon = new DynamicPolygonViewModel(points, stroke, sw, fill, opacity);
        return polygon;
    }

    /// <summary>
    /// Временный класс для произвольных полигонов при загрузке.
    /// </summary>
    private class DynamicPolygonViewModel : PolygonViewModel
    {
        public DynamicPolygonViewModel(IEnumerable<Point2D> points, System.Drawing.Color lineColor, double thickness, System.Drawing.Color fillColor, double opacity)
            : base(points, lineColor, thickness, fillColor, opacity)
        {
            Name = "Полигон";
        }

        public override FigureViewModel Clone()
        {
            return new DynamicPolygonViewModel(
                Vertices.Select(v => v.ToPoint()),
                LineColor, Thickness, FillColor, Opacity);
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static List<Point2D> ParsePoints(string pointsAttr)
    {
        var result = new List<Point2D>();
        foreach (var pair in pointsAttr.Split(new[] { ' ', '\t', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var coords = pair.Split(',');
            if (coords.Length == 2 &&
                double.TryParse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                double.TryParse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                result.Add(new Point2D(x, y));
            }
        }
        return result;
    }

    private static Point2D AverageCenter(List<Point2D> points) =>
        new Point2D(points.Average(p => p.X), points.Average(p => p.Y));

    private static double DistanceToCenter(List<Point2D> points, int index)
    {
        var center = AverageCenter(points);
        return Math.Sqrt(Math.Pow(points[index].X - center.X, 2) + Math.Pow(points[index].Y - center.Y, 2));
    }

    /// <summary>
    /// Проверяет, является ли набор из 10 точек пентаграммой (чередование радиусов).
    /// </summary>
    private static bool IsPentagram(List<Point2D> points)
    {
        if (points.Count != 10) return false;
        var center = AverageCenter(points);
        var distances = points.Select(p => p.DistanceTo(center)).OrderBy(d => d).ToList();
        // В пентаграмме 5 коротких и 5 длинных радиусов
        var shortAvg = distances.Take(5).Average();
        var longAvg = distances.Skip(5).Average();
        return longAvg / shortAvg > 2.0; // Коэффициент ~2.618 для правильной звезды
    }
    
    /// <summary>
    /// Проверяет, является ли треугольник прямоугольным (по теореме Пифагора).
    /// </summary>
    private static bool IsRightTriangle(List<Point2D> points)
    {
        if (points.Count != 3) return false;
    
        var a = points[0].DistanceTo(points[1]);
    	var b = points[1].DistanceTo(points[2]);
    	var c = points[2].DistanceTo(points[0]);
    
        var sides = new[] { a, b, c }.OrderBy(x => x).ToArray();
        // Проверка: a² + b² ≈ c² (с допуском 1%)
        return Math.Abs(sides[0]*sides[0] + sides[1]*sides[1] - sides[2]*sides[2]) < 0.01 * sides[2]*sides[2];
    }
    
    /// <summary>
    /// Проверяет, является ли набор из 4 точек ромбом (симметрия относительно центра).
    /// </summary>
    private static bool IsRhombus(List<Point2D> points)
    {
        if (points.Count != 4) return false;
        var center = AverageCenter(points);
    
        // Проверяем симметрию: противоположные вершины должны быть на равном расстоянии от центра
        var d0 = points[0].DistanceTo(center);
        var d2 = points[2].DistanceTo(center);
        var d1 = points[1].DistanceTo(center);
        var d3 = points[3].DistanceTo(center);
    
        // Допуск 10% для погрешностей парсинга
        return Math.Abs(d0 - d2) / Math.Max(d0, d2) < 0.1 && 
               Math.Abs(d1 - d3) / Math.Max(d1, d3) < 0.1;
    }

    private static Dictionary<string, string> ParseEmbeddedCss(XElement root)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var styleElements = root.Descendants()
            .Where(e => e.Name.LocalName == "style");

        foreach (var styleEl in styleElements)
        {
            var css = styleEl.Value;
            var regex = new Regex(
                @"\.([^{,\s]+)\s*\{([^}]+)\}",
                RegexOptions.Singleline);
            foreach (Match m in regex.Matches(css))
            {
                var key = "." + m.Groups[1].Value.Trim();
                var props = m.Groups[2].Value.Trim();
                result[key] = props;
            }
        }
        return result;
    }

    private static Dictionary<string, string> ResolveClassStyle(
        string? classAttr, Dictionary<string, string> cssClasses)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(classAttr)) return result;

        foreach (var cls in classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var key = "." + cls;
            if (cssClasses.TryGetValue(key, out var props))
            {
                foreach (var kv in ParseStyle(props))
                    result.TryAdd(kv.Key, kv.Value);
            }
        }
        return result;
    }

    private static Dictionary<string, string> ParseStyle(string? style)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(style)) return dict;
        foreach (var part in style.Split(';'))
        {
            var idx = part.IndexOf(':');
            if (idx > 0)
                dict[part[..idx].Trim()] = part[(idx + 1)..].Trim();
        }
        return dict;
    }

    private static (string color, string opacityProp) SvgColorWithOpacity(System.Drawing.Color c, string propName)
    {
        if (c.A == 0) return ("none", "");
        var rgb = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        var op = c.A < 255 ? $";{propName}-opacity:{F(c.A / 255.0)}" : "";
        return (rgb, op);
    }

    private static System.Drawing.Color ParseColorWithOpacity(string? colorValue, string? opacityValue)
    {
        var c = ParseColor(colorValue);
        if (c.A == 0) return c;
        if (string.IsNullOrEmpty(opacityValue)) return c;
        var op = ParseDouble(opacityValue, 1.0);
        return System.Drawing.Color.FromArgb((int)Math.Round(op * 255), c.R, c.G, c.B);
    }

    private static System.Drawing.Color ParseColor(string? value)
    {
        if (string.IsNullOrEmpty(value) || value == "none")
            return System.Drawing.Color.Transparent;

        if (value.StartsWith('#'))
        {
            var hex = value.TrimStart('#');
            if (hex.Length == 6)
                return System.Drawing.Color.FromArgb(255,
                    Convert.ToInt32(hex[..2], 16),
                    Convert.ToInt32(hex[2..4], 16),
                    Convert.ToInt32(hex[4..6], 16));
            if (hex.Length == 3)
                return System.Drawing.Color.FromArgb(255,
                    Convert.ToInt32(new string(hex[0], 2), 16),
                    Convert.ToInt32(new string(hex[1], 2), 16),
                    Convert.ToInt32(new string(hex[2], 2), 16));
        }

        if (value.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
        {
            var parts = value[4..^1].Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[0].Trim(), out int r) &&
                int.TryParse(parts[1].Trim(), out int g) &&
                int.TryParse(parts[2].Trim(), out int b))
                return System.Drawing.Color.FromArgb(255, r, g, b);
        }

        var named = System.Drawing.Color.FromName(value);
        if (named.A != 0) return named;
        return System.Drawing.Color.Transparent;
    }

    private static double ParseDouble(string? value, double fallback) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : fallback;

    private static string F(double v) =>
        v.ToString("F2", CultureInfo.InvariantCulture);

    private static string I(double v) =>
        ((int)Math.Ceiling(v)).ToString(CultureInfo.InvariantCulture);

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    
    private static double MaxHorizontalDistance(List<Point2D> points) =>
        points.Max(p => p.X) - points.Min(p => p.X);

    private static double MaxVerticalDistance(List<Point2D> points) =>
        points.Max(p => p.Y) - points.Min(p => p.Y);
}