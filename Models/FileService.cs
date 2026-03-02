// Services/FileService.cs
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using graphic_editor.Interfaces;
using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Services;

/// <summary>Временная реализация IFileService для тестирования.</summary>
public class FileService : IFileService
{
    private const string TempExtension = ".vec";
    
    public async Task<bool> SaveProjectAsync(Project project, string path)
    {
        try
        {
            // 🔥 ВРЕМЕННАЯ РЕАЛИЗАЦИЯ: просто пишем JSON
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            
            var json = JsonSerializer.Serialize(project, options);
            await File.WriteAllTextAsync(path + TempExtension, json);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileService] Save error: {ex.Message}");
            return false;
        }
    }
    
    public async Task<Project?> LoadProjectAsync(string path)
    {
        try
        {
            // 🔥 ВРЕМЕННАЯ РЕАЛИЗАЦИЯ: читаем JSON
            if (!File.Exists(path))
                path += TempExtension;
                
            if (!File.Exists(path))
                return null;
                
            var json = await File.ReadAllTextAsync(path);
            var project = JsonSerializer.Deserialize<Project>(json);
            
            // 🔥 ВНИМАНИЕ: это НЕ восстановит фигуры полностью!
            // Для полноценной загрузки нужно будет сериализовать FigureViewModel
            // через кастомный JsonConverter или использовать MessagePack/protobuf
            
            return project;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileService] Load error: {ex.Message}");
            return null;
        }
    }
    
    public async Task<bool> ExportAsPngAsync(CanvasViewModel canvas, string path, ExportSettings settings)
    {
        try
        {
            // 🔥 ВРЕМЕННАЯ ЗАГЛУШКА
            // Реальная реализация потребует:
            // 1. RenderTargetBitmap для отрисовки Canvas в bitmap
            // 2. PngEncoder для сохранения
            // 3. Учёт Zoom, Offset, TransparentBackground
            
            await Task.Delay(100); // имитация работы
            File.WriteAllText(path + ".png", "PNG-placeholder");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileService] Export error: {ex.Message}");
            return false;
        }
    }
}