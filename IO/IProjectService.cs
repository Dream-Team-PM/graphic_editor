
using graphic_editor.ViewModels;

namespace graphic_editor.IO;

public interface IProjectService
{
    Task<bool> SaveProjectAsync(string fullPath, CanvasViewModel canvas);
    Task<bool> LoadProjectAsync(string fullPath, CanvasViewModel canvas);
}
