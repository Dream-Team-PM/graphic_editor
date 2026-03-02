namespace graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using graphic_editor.Models;
public interface IFileService
{
    Task<bool> SaveProjectAsync(Project project, string path);
    Task<Project?> LoadProjectAsync(string path);
    Task<bool> ExportAsPngAsync(CanvasViewModel canvas, string path, ExportSettings settings);
}