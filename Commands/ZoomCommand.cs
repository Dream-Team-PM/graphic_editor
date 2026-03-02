// Commands/ZoomCommand.cs
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;

namespace graphic_editor.Commands;

public class ZoomCommand : IHistoryAction
{
    private readonly double _oldZoom;
    private readonly double _newZoom;
    private CanvasViewModel? _canvas;
    
    public string Description => $"Масштаб: {_newZoom:P0}";
    
    public ZoomCommand(double oldZoom, double newZoom)
    {
        _oldZoom = oldZoom;
        _newZoom = newZoom;
    }
    
    public void Undo()
    {
        if (_canvas != null)
        {
            _canvas.Zoom = _oldZoom;
        }
    }
    
    public void Redo()
    {
        if (_canvas != null)
        {
            _canvas.Zoom = _newZoom;
        }
    }
    
    public void SetCanvas(CanvasViewModel canvas) => _canvas = canvas;
}