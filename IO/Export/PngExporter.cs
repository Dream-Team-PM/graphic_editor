using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace graphic_editor.IO.Export;

/// <summary>
/// Экспортер холста в формат PNG (без потерь, с поддержкой прозрачности).
/// </summary>
public class PngExporter : ImageExporterBase
{
    /// <summary>
    /// Экспортирует канвас в PNG-файл.
    /// </summary>
    /// <param name="path">Путь к выходному файлу.</param>
    /// <param name="canvasControl">Экземпляр canvasControl для рендеринга.</param>
    public static async Task ExportAsync(string fullPath, Control canvasControl)
    {
        await Task.Yield();
        using var bitmap = RenderControl(canvasControl);
        await using var stream = File.Create(fullPath);
        bitmap.Save(stream);
    }
}