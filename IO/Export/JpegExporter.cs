using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using graphic_editor.Controls;
using  SkiaSharp;

namespace graphic_editor.IO.Export;

/// <summary>
/// Экспортер в формат JPEG (сжатие с потерями, без альфа-канала (без поддержки прозрачности)).
/// </summary>
public class JpegExporter : ImageExporterBase
{
    /// <summary>
    /// Экспортирует контрол канваса в JPEG-файл с заданным качеством.
    /// </summary>
    /// <param name="fullPath">Полный путь к выходному файлу.</param>
    /// <param name="canvasControl">Контрол канваса для рендеринга.</param>
    /// <param name="quality">Качество сжатия от 1 до 100 (по умолчанию 90).</param>
    public static async Task ExportAsync(string fullPath, Control canvasControl, int quality = 90)
    {
        //await Task.Yield();
        quality = Math.Clamp(quality, 1, 100);
        using var avaloniaBitmap = RenderControl(canvasControl);
        using var skBitmap = ConvertToSkia(avaloniaBitmap);
        using var jpegBitmap = WithWhiteBackground(skBitmap);
        await using var stream = File.Create(fullPath);
        using var skStream = new SKManagedWStream(stream);
        var success = jpegBitmap.Encode(skStream, SKEncodedImageFormat.Jpeg, quality);
        skStream.Flush();
        
        if (!success)
            throw new InvalidOperationException("Failed to encode JPEG");
    }
}