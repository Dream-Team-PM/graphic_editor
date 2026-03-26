using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using graphic_editor.Helpers;
using SkiaSharp;

namespace graphic_editor.IO.Export;

/// <summary>
/// Базовый класс для экспортеров растровых изображений.
/// </summary>
public abstract class ImageExporterBase
{
    /// <summary>
    /// Рендерит Control в RenderTargetBitmap.
    /// </summary>
    protected static RenderTargetBitmap RenderControl(Control control)
    {
        var bounds = control.Bounds;
        var width = (int)Math.Max(bounds.Width, 1); var height = (int)Math.Max(bounds.Height, 1);
        
        var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        bitmap.Render(control);
        return bitmap;
    }
    
    /// <summary>
    /// Конвертирует Avalonia RenderTargetBitmap в SkiaSharp SKBitmap через PNG в памяти.
    /// </summary>
    protected static SKBitmap ConvertToSkia(RenderTargetBitmap avaloniaBitmap)
    {
        // Сохраняем в PNG в памяти (надёжно и кроссплатформенно)
        using var ms = new MemoryStream();
        avaloniaBitmap.Save(ms);
        ms.Position = 0;
        
        using var skStream = new SKManagedStream(ms);
        using var codec = SKCodec.Create(skStream);
        if (codec == null)
            throw new InvalidOperationException("Failed to create SKCodec from PNG data");
        
        var info = codec.Info;
        DebugLog.Write($"[CONVERT] Codec info: {info.Width}x{info.Height}, ColorType: {info.ColorType}, AlphaType: {info.AlphaType}");
        var skBitmap = new SKBitmap(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        
        var result = codec.GetPixels(skBitmap.Info, skBitmap.GetPixels());
        if (result == SKCodecResult.Success)
        {
            DebugLog.Write($"[CONVERT] ✅ Decoded successfully");
            return skBitmap;
        }
    
        DebugLog.Write($"[CONVERT] ❌ Decode failed: {result}");
        throw new InvalidOperationException($"Failed to decode bitmap: {result}");
    }
    
    /// <summary>
    /// Создаёт SKBitmap с белым фоном и копирует в него исходный (для форматов без альфа-канала).
    /// </summary>
    protected static SKBitmap WithWhiteBackground(SKBitmap source)
    {
        var result = new SKBitmap(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var canvas = new SKCanvas(result);
        using var whitePaint = new SKPaint { Color = SKColors.White };
        canvas.DrawRect(0, 0, result.Width, result.Height, whitePaint);
        
        // Рисуем исходный поверх (с учётом прозрачности)
        using var srcPaint = new SKPaint { IsAntialias = true };
        canvas.DrawBitmap(source, 0, 0, srcPaint);
        
        return result;
    }
}