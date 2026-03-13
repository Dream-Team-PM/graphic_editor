using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;

namespace graphic_editor.IO.ProjectFormat;

public class SvgProjectFormat : IProjectFormat
{
    public string FileExtension => ".svg";

    public async Task SaveAsync(string fullPath, CanvasViewModel canvas)
    {
        double maxX = 800, maxY = 600;
        foreach (var layer in canvas.Layers)
        {
            foreach (var f in layer.Figures)
                UpdateBounds(f, ref maxX, ref maxY);
        }

        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        sb.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{I(maxX)}" height="{I(maxY)}" viewBox="0 0 {I(maxX)} {I(maxY)}">""");

        foreach (var layer in canvas.Layers)
        {
            if (!layer.IsVisible) continue;
            sb.AppendLine($"""  <g id="{layer.Id}" data-name="{Esc(layer.Name)}">""");
            foreach (var figure in layer.Figures)
                AppendFigure(sb, figure, "    ");
            sb.AppendLine("  </g>");
        }

        sb.AppendLine("</svg>");
        await File.WriteAllTextAsync(fullPath, sb.ToString());
    }

    public async Task LoadAsync(string fullPath, CanvasViewModel canvas)
    {
        var xml = await File.ReadAllTextAsync(fullPath);
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidDataException("Пустой SVG файл");

        canvas.Layers.Clear();

        var groups = root.Elements().Where(e => e.Name.LocalName == "g").ToList();

        if (groups.Count == 0)
        {
            var layer = new LayerViewModel("Слой 1");
            foreach (var el in root.Elements())
            {
                var fig = ParseElement(el);
                if (fig != null) layer.Figures.Add(fig);
            }
            canvas.Layers.Add(layer);
        }
        else
        {
            foreach (var g in groups)
            {
                var name = g.Attribute("data-name")?.Value
                    ?? g.Attribute("id")?.Value
                    ?? "Слой";
                var idStr = g.Attribute("id")?.Value;
                var layer = Guid.TryParse(idStr, out var gid)
                    ? new LayerViewModel(gid, name)
                    : new LayerViewModel(name);

                foreach (var el in g.Elements())
                {
                    var fig = ParseElement(el);
                    if (fig != null) layer.Figures.Add(fig);
                }
                canvas.Layers.Add(layer);
            }
        }

        if (canvas.Layers.Count > 0)
            canvas.ActiveLayer = canvas.Layers[0];
    }

    // ── Сохранение ──────────────────────────────────────────────────────────

    private static void AppendFigure(StringBuilder sb, FigureViewModel figure, string indent)
    {
        var fill = SvgColor(figure.FillColor);
        var stroke = SvgColor(figure.LineColor);
        var sw = F(figure.Thickness);
        var op = F(figure.Opacity);
        var tr = figure.Rotation != 0
            ? $""" transform="rotate({F(figure.Rotation)} {F(figure.Center.X)} {F(figure.Center.Y)})"""
            : "";

        string line = figure switch
        {
            RectangleViewModel r =>
                $"""<rect x="{F(r.X)}" y="{F(r.Y)}" width="{F(r.Width)}" height="{F(r.Height)}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" opacity="{op}"{tr}/>""",

            CircleViewModel c =>
                $"""<circle cx="{F(c.Center.X)}" cy="{F(c.Center.Y)}" r="{F(c.Radius)}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" opacity="{op}"{tr}/>""",

            EllipseViewModel e =>
                $"""<ellipse cx="{F(e.X + e.Width / 2)}" cy="{F(e.Y + e.Height / 2)}" rx="{F(e.Width / 2)}" ry="{F(e.Height / 2)}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" opacity="{op}"{tr}/>""",

            LineViewModel l =>
                $"""<line x1="{F(l.X1)}" y1="{F(l.Y1)}" x2="{F(l.X2)}" y2="{F(l.Y2)}" stroke="{stroke}" stroke-width="{sw}" opacity="{op}"/>""",

            PenPointViewModel p =>
                $"""<circle cx="{F(p.X)}" cy="{F(p.Y)}" r="{F(p.Thickness / 2.0)}" fill="{stroke}" opacity="{op}"/>""",

            GroupViewModel g => null!,

            _ => $"""<!-- {Esc(figure.GetType().Name)} -->"""
        };

        if (figure is GroupViewModel grp)
        {
            sb.AppendLine($"""{indent}<g opacity="{op}"{tr}>""");
            foreach (var child in grp.Children)
                AppendFigure(sb, child, indent + "  ");
            sb.AppendLine($"{indent}</g>");
        }
        else
        {
            sb.AppendLine(indent + line);
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

    // ── Загрузка ─────────────────────────────────────────────────────────────

    private static FigureViewModel? ParseElement(XElement el)
    {
        var fill = ParseColor(el.Attribute("fill")?.Value);
        var stroke = ParseColor(el.Attribute("stroke")?.Value);
        var sw = ParseDouble(el.Attribute("stroke-width")?.Value, 1.0);
        var opacity = ParseDouble(el.Attribute("opacity")?.Value, 1.0);

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
                    .Select(ParseElement)
                    .Where(f => f != null)
                    .Select(f => f!)),

            _ => null
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string SvgColor(Color c) =>
        c.A == 0 ? "none" : $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static Color ParseColor(string? value)
    {
        if (string.IsNullOrEmpty(value) || value == "none")
            return Color.Transparent;

        if (value.StartsWith('#'))
        {
            var hex = value.TrimStart('#');
            if (hex.Length == 6)
            {
                return Color.FromArgb(255,
                    Convert.ToInt32(hex[..2], 16),
                    Convert.ToInt32(hex[2..4], 16),
                    Convert.ToInt32(hex[4..6], 16));
            }
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