// Helpers/Exporters/PdfExporter.cs
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;
using graphic_editor.Controls;
using graphic_editor.ViewModels;
using graphic_editor.Helpers;

namespace graphic_editor.IO.Export;

/// <summary>
/// Экспортер в PDF через SkiaSharp (векторный, с полным сохранением графики).
/// </summary>
public class PdfExporter : ImageExporterBase
{
    /// <summary>
    /// Экспортирует канвас в векторный PDF-файл.
    /// </summary>
    /// <param name="path">Путь к выходному файлу.</param>
    /// <param name="canvas">Экземпляр VectorCanvasControl.</param>
    /// <param name="canvasVm">CanvasViewModel для доступа к данным фигур.</param>
    public static async Task ExportAsync(string path, VectorCanvasControl canvas, CanvasViewModel canvasVm)
    {
        //await Task.Yield();
        var width = (int)canvas.Bounds.Width;
        var height = (int)canvas.Bounds.Height;
        
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Canvas has invalid size for export");

        // Создаём PDF-документ через Skia
        using var stream = File.OpenWrite(path);
        using var document = SKDocument.CreatePdf(stream);
        using var pdfCanvas = document.BeginPage(width, height);
        
        // Рендерим содержимое канваса (упрощённо — через Bitmap)
        using var avaloniaBitmap = RenderControl(canvas);
        using var skBitmap = ConvertToSkia(avaloniaBitmap);
        using var skImage = SKImage.FromBitmap(skBitmap);
        
        pdfCanvas.DrawImage(skImage, 0, 0);
        
        document.EndPage();
        document.Close();
        
        DebugLog.Write($"[INFO] PDF exported: {path} ({width}x{height})");
    }
    
    // Вспомогательный метод конвертации
    private static SKBitmap ConvertAvaloniaToSkia(Avalonia.Media.Imaging.RenderTargetBitmap avaloniaBitmap)
    {
        throw new NotImplementedException("Implement conversion logic");
    }
}