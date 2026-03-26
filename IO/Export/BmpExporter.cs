using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using graphic_editor.Helpers;
using SkiaSharp;

namespace graphic_editor.IO.Export;

/// <summary>
/// Экспортер холста в формат BMP (без сжатия, максимальное качество).
/// </summary>
public class BmpExporter : ImageExporterBase
{
    /// <summary>
    /// Экспортирует контрол канваса в BMP-файл.
    /// </summary>
    public static async Task ExportAsync(string fullPath, Control canvasControl)
    {
        //await Task.Yield();
        
        DebugLog.Write($"[BMP] Starting export to: {fullPath}");
        
        using var avaloniaBitmap = RenderControl(canvasControl);
        DebugLog.Write($"[BMP] Rendered: {avaloniaBitmap.PixelSize.Width}x{avaloniaBitmap.PixelSize.Height}");
        
        using var skBitmap = ConvertToSkia(avaloniaBitmap);
        DebugLog.Write($"[BMP] Converted SKBitmap: {skBitmap.Width}x{skBitmap.Height}, ColorType: {skBitmap.ColorType}, AlphaType: {skBitmap.AlphaType}");
        
        if (skBitmap.Width <= 0 || skBitmap.Height <= 0)
            throw new InvalidOperationException($"Invalid bitmap size: {skBitmap.Width}x{skBitmap.Height}");
        
        using var bmpBitmap = ConvertToBmpCompatible(skBitmap);
        DebugLog.Write($"[BMP] BMP-compatible: {bmpBitmap.ColorType}, AlphaType: {bmpBitmap.AlphaType}, BytesPerPixel: {bmpBitmap.BytesPerPixel}");
        
        bool success = TryEncodeWithFileWStream(fullPath, bmpBitmap);
        
        if (!success)
        {
            DebugLog.Write("[BMP] Fallback: trying EncodeToData...");
            success = TryEncodeWithData(bmpBitmap, fullPath);
        }
        
        if (!success)
        {
            DebugLog.Write("[BMP] Fallback: saving as PNG with .bmp extension...");
            await FallbackSaveAsPng(fullPath, bmpBitmap);
            success = true;
        }
        
        if (!success)
            throw new InvalidOperationException($"Failed to encode BMP after all attempts");
        
        var fileSize = new FileInfo(fullPath).Length;
        DebugLog.Write($"[BMP] ✅ Export successful: {fullPath} ({fileSize} bytes)");
    }
    
    /// <summary>
    /// Попытка кодирования через SKFileWStream (прямая запись в файл).
    /// </summary>
    private static bool TryEncodeWithFileWStream(string path, SKBitmap bitmap)
    {
        try
        {
            using var fileStream = new SKFileWStream(path);
            var success = bitmap.Encode(fileStream, SKEncodedImageFormat.Bmp, 100);
            fileStream.Flush();
            DebugLog.Write($"[BMP] SKFileWStream encode: {success}");
            return success;
        }
        catch (Exception ex)
        {
            DebugLog.Write($"[BMP] SKFileWStream failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Попытка кодирования через EncodeToData + File.WriteAllBytes.
    /// </summary>
    private static bool TryEncodeWithData(SKBitmap bitmap, string path)
    {
        try
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image?.Encode(SKEncodedImageFormat.Bmp, 100);
            
            if (data != null && data.Size > 0)
            {
                File.WriteAllBytes(path, data.ToArray());
                DebugLog.Write($"[BMP] EncodeToData success: {data.Size} bytes");
                return true;
            }
            DebugLog.Write("[BMP] EncodeToData returned null or empty");
            return false;
        }
        catch (Exception ex)
        {
            DebugLog.Write($"[BMP] EncodeToData failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Фолбэк: сохраняем как PNG, но с расширением .bmp (работает в большинстве просмотрщиков).
    /// </summary>
    private static async Task FallbackSaveAsPng(string path, SKBitmap bitmap)
    {
        // var pngPath = Path.ChangeExtension(path, ".png");
        //
        // using var image = SKImage.FromBitmap(bitmap);
        // using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        //
        // if (data != null)
        // {
        //     await File.WriteAllBytesAsync(pngPath, data.ToArray());
        //     if (Path.GetExtension(path).ToLower() == ".bmp" && pngPath != path)
        //     {
        //         if (File.Exists(path)) File.Delete(path);
        //         File.Move(pngPath, path);
        //     }
        // }
        // else
        // {
        //     throw new InvalidOperationException("Failed to save fallback PNG");
        // }
        // Конвертируем SKBitmap → System.Drawing.Bitmap
        using var ms = new MemoryStream();
        bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
        ms.Position = 0;
    
        using var drawingBitmap = new System.Drawing.Bitmap(ms);
    
        // Сохраняем как настоящий BMP
        drawingBitmap.Save(path, System.Drawing.Imaging.ImageFormat.Bmp);
    
        DebugLog.Write($"[BMP] ✅ Saved via System.Drawing: {new FileInfo(path).Length} bytes");
    }
    
    /// <summary>
    /// Конвертирует битмап в формат, максимально совместимый с BMP-энкодером.
    /// </summary>
    private static SKBitmap ConvertToBmpCompatible(SKBitmap source)
    {
        // BMP лучше всего работает с:
        // - ColorType: Bgra8888 или Rgb888x
        // - AlphaType: Opaque (без прозрачности)
        
        if (source.ColorType == SKColorType.Bgra8888 && source.AlphaType == SKAlphaType.Opaque)
            return source;
        var result = new SKBitmap(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var canvas = new SKCanvas(result);
        using var bgPaint = new SKPaint { Color = SKColors.White };
        canvas.DrawRect(0, 0, result.Width, result.Height, bgPaint);
        using var srcPaint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
        canvas.DrawBitmap(source, 0, 0, srcPaint);
        return result;
    }
}