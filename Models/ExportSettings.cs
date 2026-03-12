// Models/ExportSettings.cs
namespace graphic_editor.Models;

/// <summary>Настройки экспорта изображения.</summary>
public record ExportSettings(
    int Width,
    int Height,
    int Dpi = 96,
    bool TransparentBackground = false,
    string Format = "png" // "png", "jpg", "bmp"
);