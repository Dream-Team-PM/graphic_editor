using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace graphic_editor.IO.Export;

/// <summary>Экспорт холста в PNG через рендер Avalonia Visual.</summary>
public static class PngExporter
{
    public static async Task ExportAsync(string fullPath, Control canvasControl)
    {
        var bounds = canvasControl.Bounds;
        var width = (int)Math.Max(bounds.Width, 1);
        var height = (int)Math.Max(bounds.Height, 1);

        var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        bitmap.Render(canvasControl);

        await using var stream = File.Create(fullPath);
        bitmap.Save(stream);
    }
}
