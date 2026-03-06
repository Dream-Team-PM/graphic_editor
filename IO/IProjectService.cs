namespace graphic_editor.IO;
using graphic_editor.ViewModels;

public interface IProjectService
{
    Task<bool> SaveProjectAsync(string fullPath, CanvasViewModel canvas);
    Task<bool> LoadProjectAsync(string fullPath, CanvasViewModel canvas);
}