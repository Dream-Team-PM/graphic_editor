using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;

namespace graphic_editor.IO.ProjectFormat;

/// <summary>
/// Сохранение и загрузка проекта в формате SVG.
/// Совместим с Inkscape, Illustrator, браузерами:
///   — стили в атрибуте style="fill:...;stroke:...;" (CSS inline)
///   — слои: inkscape:groupmode="layer" + читаемые id (layer1, layer2)
///   — парсинг понимает и style-атрибут, и отдельные атрибуты
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

        // Парсим встроенные CSS-классы из <defs><style>…</style></defs>
        var cssClasses = ParseEmbeddedCss(root);

        // Элементы верхнего уровня, игнорируем <defs> и <title>
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

    // ── Запись фигур (Inkscape-совместимый формат) ────────────────────────────

    private static void AppendFigure(StringBuilder sb, FigureViewModel figure, string indent, ref int figId)
    {
        // fill и stroke с поддержкой альфа-канала через fill-opacity / stroke-opacity
        var (fill,   fillOp)   = SvgColorWithOpacity(figure.FillColor,  "fill");
        var (stroke, strokeOp) = SvgColorWithOpacity(figure.LineColor,  "stroke");
        // Минимум 1px — так же как VectorCanvasControl (Math.Max(1, Thickness))
        var sw = F(Math.Max(1.0, figure.Thickness));
        // figure.Opacity — прозрачность (0=непрозрачный, 1=прозрачный),
        // SVG opacity — непрозрачность (0=прозрачный, 1=непрозрачный) → инвертируем
        var svgOpacity = 1.0 - figure.Opacity;
        var opProp = svgOpacity < 0.9999 ? $";opacity:{F(svgOpacity)}" : "";
        var style  = $"fill:{fill}{fillOp};stroke:{stroke}{strokeOp};stroke-width:{sw};stroke-linejoin:round;paint-order:markers fill stroke{opProp}";
        var tr     = figure.Rotation != 0
            ? $"\n{indent}  transform=\"rotate({F(figure.Rotation)} {F(figure.Center.X)} {F(figure.Center.Y)})\""
            : "";

        if (figure is GroupViewModel grp)
        {
            var grpSvgOp  = 1.0 - figure.Opacity;
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
                // Точки пера — маленькие закрашенные круги; цвет берём из LineColor как fill
                var (dotColor, dotOp) = SvgColorWithOpacity(p.LineColor, "fill");
                sb.AppendLine($"{indent}<circle");
                sb.AppendLine($"{indent}  style=\"fill:{dotColor}{dotOp};stroke:none\"");
                sb.AppendLine($"{indent}  id=\"dot{id}\"");
                sb.AppendLine($"{indent}  cx=\"{F(p.X)}\"");
                sb.AppendLine($"{indent}  cy=\"{F(p.Y)}\"");
                sb.AppendLine($"{indent}  r=\"{F(p.Thickness / 2.0)}\" />");
                break;

            default:
                sb.AppendLine($"{indent}<!-- {Esc(figure.GetType().Name)} -->");
                break;
        }
    }

    private static void UpdateBounds(FigureViewModel f, ref double maxX, ref double maxY)
    {
        switch (f)
        {
            case RectangleViewModel r:
                maxX = Math.Max(maxX, r.X + r.Width + 20);
                maxY = Math.Max(maxY, r.Y + r.Height + 20);
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
            case GroupViewModel g:
                foreach (var child in g.Children)
                    UpdateBounds(child, ref maxX, ref maxY);
                break;
        }
    }

    // ── Парсинг фигур ─────────────────────────────────────────────────────────

    private static FigureViewModel? ParseElement(XElement el,
        Dictionary<string, string>? cssClasses = null)
    {
        cssClasses ??= new Dictionary<string, string>();

        // Приоритет стилей: inline style > CSS-класс > отдельный атрибут
        var inlineCss = ParseStyle(el.Attribute("style")?.Value);
        var classCss  = ResolveClassStyle(el.Attribute("class")?.Value, cssClasses);

        string? Get(string prop) =>
            inlineCss.GetValueOrDefault(prop)
            ?? classCss.GetValueOrDefault(prop)
            ?? el.Attribute(prop)?.Value;

        // Цвет + отдельный opacity канала (fill-opacity / stroke-opacity)
        var fill    = ParseColorWithOpacity(Get("fill"),   Get("fill-opacity"));
        var stroke  = ParseColorWithOpacity(Get("stroke"), Get("stroke-opacity"));
        var sw = ParseDouble(Get("stroke-width"), 1.0);
        // SVG opacity (0=прозрачный, 1=непрозрачный) → figure.Opacity (0=непрозрачный, 1=прозрачный)
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

            "g" => new GroupViewModel(
                el.Elements()
                    .Select(e => ParseElement(e, cssClasses))
                    .Where(f => f != null)
                    .Select(f => f!)),

            // <path> — Illustrator конвертирует все фигуры в кривые Безье.
            // Их нельзя вернуть в rect/circle без трассировки, пропускаем.
            "path" => null,

            _ => null
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Вытаскивает CSS-классы из &lt;defs&gt;&lt;style&gt;…&lt;/style&gt;&lt;/defs&gt;.
    /// Возвращает словарь: ".className" → "prop1:val1;prop2:val2"
    /// </summary>
    private static Dictionary<string, string> ParseEmbeddedCss(XElement root)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var styleElements = root.Descendants()
            .Where(e => e.Name.LocalName == "style");

        foreach (var styleEl in styleElements)
        {
            var css = styleEl.Value;
            // Ищем паттерн: .className { prop: value; ... }
            var regex = new System.Text.RegularExpressions.Regex(
                @"\.([^{,\s]+)\s*\{([^}]+)\}",
                System.Text.RegularExpressions.RegexOptions.Singleline);
            foreach (System.Text.RegularExpressions.Match m in regex.Matches(css))
            {
                var key   = "." + m.Groups[1].Value.Trim();
                var props = m.Groups[2].Value.Trim();
                result[key] = props;
            }
        }
        return result;
    }

    /// <summary>
    /// Разрешает значение атрибута class="st0 st1 …" в словарь CSS-свойств.
    /// Inline style перекрывает класс — приоритет решается в ParseElement.
    /// </summary>
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

    /// <summary>Разбирает style="prop1:val1;prop2:val2" в словарь.</summary>
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

    /// <summary>
    /// Возвращает (colorString, opacityProp) для использования в SVG style.
    /// Пример: FillColor с A=128 → ("#FF0000", ";fill-opacity:0.50")
    /// </summary>
    private static (string color, string opacityProp) SvgColorWithOpacity(Color c, string propName)
    {
        if (c.A == 0) return ("none", "");
        var rgb = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        var op  = c.A < 255 ? $";{propName}-opacity:{F(c.A / 255.0)}" : "";
        return (rgb, op);
    }

    /// <summary>
    /// Парсит SVG-цвет с учётом отдельного атрибута fill-opacity / stroke-opacity.
    /// </summary>
    private static Color ParseColorWithOpacity(string? colorValue, string? opacityValue)
    {
        var c = ParseColor(colorValue);
        if (c.A == 0) return c;                          // "none" → прозрачный
        if (string.IsNullOrEmpty(opacityValue)) return c; // нет opacity → как есть
        var op = ParseDouble(opacityValue, 1.0);
        return Color.FromArgb((int)Math.Round(op * 255), c.R, c.G, c.B);
    }

    private static Color ParseColor(string? value)
    {
        if (string.IsNullOrEmpty(value) || value == "none")
            return Color.Transparent;

        if (value.StartsWith('#'))
        {
            var hex = value.TrimStart('#');
            if (hex.Length == 6)
                return Color.FromArgb(255,
                    Convert.ToInt32(hex[..2], 16),
                    Convert.ToInt32(hex[2..4], 16),
                    Convert.ToInt32(hex[4..6], 16));
            if (hex.Length == 3)
                return Color.FromArgb(255,
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
                return Color.FromArgb(255, r, g, b);
        }

        var named = Color.FromName(value);
        if (named.A != 0) return named;

        return Color.Transparent;
    }

    private static double ParseDouble(string? value, double fallback) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : fallback;

    private static string F(double v) =>
        v.ToString("F2", CultureInfo.InvariantCulture);

    private static string I(double v) =>
        ((int)Math.Ceiling(v)).ToString(CultureInfo.InvariantCulture);

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
