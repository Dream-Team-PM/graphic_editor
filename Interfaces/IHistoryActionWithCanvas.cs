// Interfaces/IHistoryActionWithCanvas.cs
using graphic_editor.ViewModels;

namespace graphic_editor.Interfaces;

/// <summary>Интерфейс для команд, которым нужен доступ к Canvas.</summary>
public interface IHistoryActionWithCanvas : IHistoryAction
{
    void SetCanvas(CanvasViewModel canvas);
}