namespace graphic_editor.IO.ProjectFormat;
using graphic_editor.ViewModels;

public interface IProjectFormat
{
    string FileExtension { get; }
    Task SaveAsync(string fullPath, CanvasViewModel canvas);
    Task LoadAsync(string fullPath, CanvasViewModel canvas);
}