using graphic_editor.IO.ProjectFormat;

namespace graphic_editor.IO.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectFormat _format;

    public ProjectService(IProjectFormat format)
    {
        _format = format;
    }

    public async Task<bool> SaveProjectAsync(string fullPath, CanvasViewModel canvas)
    {
        if (!fullPath.EndsWith(_format.FileExtension, StringComparison.OrdinalIgnoreCase))
            fullPath += _format.FileExtension;

        try
        {
            await _format.SaveAsync(fullPath, canvas);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoadProjectAsync(string fullPath, CanvasViewModel canvas)
    {
        if (!fullPath.EndsWith(_format.FileExtension, StringComparison.OrdinalIgnoreCase))
            fullPath += _format.FileExtension;

        try
        {
            await _format.LoadAsync(fullPath, canvas);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
