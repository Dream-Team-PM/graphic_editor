using graphic_editor.IO.ProjectFormat;

namespace graphic_editor.IO.Services;

public class ProjectService : IProjectService
{
    private readonly Dictionary<string, IProjectFormat> _formats =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { ".vec",  new JsonProjectFormat() },
            { ".json", new JsonProjectFormat() },
            { ".svg",  new SvgProjectFormat()  },
        };

    public async Task<bool> SaveProjectAsync(string fullPath, CanvasViewModel canvas)
    {
        var ext = Path.GetExtension(fullPath);
        if (!_formats.TryGetValue(ext, out var format))
            format = _formats[".vec"];

        try
        {
            await format.SaveAsync(fullPath, canvas);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoadProjectAsync(string fullPath, CanvasViewModel canvas)
    {
        var ext = Path.GetExtension(fullPath);
        if (!_formats.TryGetValue(ext, out var format))
            return false;

        try
        {
            await format.LoadAsync(fullPath, canvas);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
