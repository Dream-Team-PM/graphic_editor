using graphic_editor.ViewModels;

namespace graphic_editor.IO.ProjectFormat;

public interface IProjectFormat
{
    string FileExtension { get; }
    Task SaveAsync(string fullPath, CanvasViewModel canvas);
    Task LoadAsync(string fullPath, CanvasViewModel canvas);
}
