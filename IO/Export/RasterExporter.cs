
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace graphic_editor.IO.Export;

/// <summary>

/// Растровый экспорт холста в PNG, JPEG, BMP и PDF.

/// Все форматы реализованы через одну библиотеку — SkiaSharp.
/// Workflow: Avalonia.RenderTargetBitmap → PNG-байты → SKBitmap → целевой формат.
/// </summary>
public static class RasterExporter
{

    public enum Format { Png, Jpeg, Bmp, Pdf }

    // ── Публичный метод ───────────────────────────────────────────────────────

    /// <summary>
    /// Экспортирует <paramref name="canvasControl"/> в файл <paramref name="fullPath"/>.
    /// Формат определяется параметром <paramref name="format"/>.
    /// </summary>
    public static async Task ExportAsync(string fullPath, Control canvasControl, Format format)
    {
        // 1. Рендерим Avalonia-контрол в PNG через RenderTargetBitmap
        var avBitmap = RenderControl(canvasControl);

        // 2. Переводим в SkiaSharp-битмап
        using var skBitmap = await ToSkBitmapAsync(avBitmap);

        // 3. Сохраняем в нужном формате
        await using var stream = File.Create(fullPath);

        if (format == Format.Pdf)
            SavePdf(stream, skBitmap);
        else
            SaveRaster(stream, skBitmap, format);
    }

    // ── Вспомогательные методы ────────────────────────────────────────────────

    private static RenderTargetBitmap RenderControl(Control control)
    {
        var bounds = control.Bounds;
        var width  = Math.Max((int)bounds.Width,  1);
        var height = Math.Max((int)bounds.Height, 1);

        var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        bitmap.Render(control);
        return bitmap;
    }

    private static async Task<SKBitmap> ToSkBitmapAsync(RenderTargetBitmap avBitmap)
    {
        // Сохраняем Avalonia-битмап как PNG в памяти
        using var ms = new MemoryStream();
        avBitmap.Save(ms);
        ms.Position = 0;

        // SkiaSharp декодирует PNG в SKBitmap
        var skBitmap = SKBitmap.Decode(ms)
            ?? throw new InvalidOperationException("Не удалось декодировать битмап.");
        return await Task.FromResult(skBitmap);
    }

    private static void SaveRaster(Stream stream, SKBitmap skBitmap, Format format)
    {
        using var image = SKImage.FromBitmap(skBitmap);

        var (encFormat, quality) = format switch
        {
            Format.Jpeg => (SKEncodedImageFormat.Jpeg, 95),
            Format.Bmp  => (SKEncodedImageFormat.Bmp,  100),
            _           => (SKEncodedImageFormat.Png,  100),
        };

        using var data = image.Encode(encFormat, quality);
        data.SaveTo(stream);
    }

    private static void SavePdf(Stream stream, SKBitmap skBitmap)
    {
        using var document = SKDocument.CreatePdf(stream);
        // Страница размером с изображение (в пунктах; 1 px ≈ 1 pt при 96 dpi → 72/96 = 0.75)
        float w = skBitmap.Width  * 72f / 96f;
        float h = skBitmap.Height * 72f / 96f;

        using var skCanvas = document.BeginPage(w, h);
        // Масштабируем, чтобы пиксели совпали с пунктами
        skCanvas.Scale(72f / 96f, 72f / 96f);
        skCanvas.DrawBitmap(skBitmap, 0, 0);
        document.EndPage();
        document.Close();
    }
}
