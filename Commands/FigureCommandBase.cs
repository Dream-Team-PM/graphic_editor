// // Commands/FigureCommandBase.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;
//
// namespace graphic_editor.Commands;
//
// /// <summary>Базовый класс для команд, сохраняющих состояние фигур.</summary>
// public abstract class FigureCommandBase : IFigureCommand
// {
//     protected readonly Dictionary<Guid, FigureState> _beforeStates = new();
//     protected readonly Dictionary<Guid, FigureState> _afterStates = new();
//     
//     /// <summary>Снимок состояния фигуры для Undo/Redo.</summary>
//     protected record FigureState(
//         double X, double Y, double Width, double Height,
//         double Rotation, Color LineColor, Color FillColor, 
//         double Thickness, double Opacity, bool IsSelected);
//     
//     protected record FigureState(
//         double MinX, double MaxX, double MinY, double MaxY,
//         Color LineColor, Color FillColor, double Thickness, double Opacity);
//     
//     public abstract string Description { get; }
//     public abstract void Execute(CanvasViewModel canvas);
//     
//     public virtual void Undo()
//     {
//         foreach (var (id, state) in _beforeStates)
//         {
//             ApplyState(canvas, id, state);
//         }
//     }
//     
//     public virtual void Redo()
//     {
//         foreach (var (id, state) in _afterStates)
//         {
//             ApplyState(canvas, id, state);
//         }
//     }
//     
//     protected void CaptureBefore(FigureViewModel figure)
//     {
//         if (!_beforeStates.ContainsKey(figure.Id))
//         {
//             _beforeStates[figure.Id] = CaptureState(figure);
//         }
//     }
//     
//     protected void CaptureAfter(FigureViewModel figure)
//     {
//         _afterStates[figure.Id] = CaptureState(figure);
//     }
//     
//     private FigureState CaptureState(FigureViewModel figure)
//     {
//         var bbox = figure.GetBoundingBox();
//         return new FigureState(
//             bbox.MinX, bbox.MinY, bbox.MaxX - bbox.MinX, bbox.MaxY - bbox.MinY,
//             0, // Rotation: можно добавить в FigureViewModel если нужно
//             figure.LineColor, figure.FillColor,
//             figure.Thickness, figure.Opacity, figure.IsSelected);
//     }
//     
//     private void ApplyState(CanvasViewModel canvas, Guid id, FigureState state)
//     {
//         var figure = FindFigure(canvas, id);
//         if (figure == null) return;
//         
//         // Применяем сохранённое состояние
//         var dx = state.X - figure.GetBoundingBox().MinX;
//         var dy = state.Y - figure.GetBoundingBox().MinY;
//         figure.Move(dx, dy);
//         
//         figure.LineColor = state.LineColor;
//         figure.FillColor = state.FillColor;
//         figure.Thickness = state.Thickness;
//         figure.Opacity = state.Opacity;
//         figure.IsSelected = state.IsSelected;
//         
//         figure.RaisePropertyChanged(nameof(FigureViewModel.Center));
//         figure.RaisePropertyChanged(nameof(FigureViewModel.Vertices));
//     }
//     
//     protected FigureViewModel? FindFigure(CanvasViewModel canvas, Guid id)
//     {
//         return canvas.Layers
//             .SelectMany(l => l.Figures)
//             .FirstOrDefault(f => f.Id == id);
//     }
//     protected CanvasViewModel? canvas;
// }
// Commands/FigureCommandBase.cs

namespace graphic_editor.Commands;

/// <summary>Базовый класс для команд с сохранением состояния фигур.</summary>
public abstract class FigureCommandBase : IHistoryAction
{
    protected readonly Dictionary<Guid, FigureState> _before = new();
    protected readonly Dictionary<Guid, FigureState> _after = new();
    
    protected record FigureState(
        double MinX, double MaxX, double MinY, double MaxY, double Rotation,
        Color LineColor, Color FillColor, double Thickness, double Opacity);
    
    public abstract string Description { get; }
    public abstract void Execute(CanvasViewModel canvas);
    
    public virtual void Undo()
    {
        foreach (var (id, state) in _before)
            ApplyState(canvas, id, state);
    }
    
    public virtual void Redo()
    {
        foreach (var (id, state) in _after)
            ApplyState(canvas, id, state);
    }
    
    protected void CaptureBefore(FigureViewModel figure)
    {
        if (!_before.ContainsKey(figure.Id))
        {
            var bbox = figure.GetBoundingBox();
            _before[figure.Id] = new FigureState(
                bbox.MinX, bbox.MaxX, bbox.MinY, bbox.MaxY, figure.Rotation,
                figure.LineColor, figure.FillColor, figure.Thickness, figure.Opacity);
        }
    }
    
    protected void CaptureAfter(FigureViewModel figure)
    {
        var bbox = figure.GetBoundingBox();
        _after[figure.Id] = new FigureState(
            bbox.MinX, bbox.MaxX, bbox.MinY, bbox.MaxY, figure.Rotation,
            figure.LineColor, figure.FillColor, figure.Thickness, figure.Opacity);
    }
    
    private void ApplyState(CanvasViewModel canvas, Guid id, FigureState state)
    {
        var figure = FindFigure(canvas, id);
        if (figure == null) return;
        
        // Восстанавливаем позицию через Move
        var currentBbox = figure.GetBoundingBox();
        var dx = state.MinX - currentBbox.MinX;
        var dy = state.MinY - currentBbox.MinY;
        figure.Move(dx, dy);

		// ✅ Восстанавливаем угол поворота
    var angleDiff = state.Rotation - figure.Rotation;
    if (Math.Abs(angleDiff) > 0.01)
    {
        figure.Rotate(angleDiff);
    }
        
        // Восстанавливаем стиль
        figure.LineColor = state.LineColor;
        figure.FillColor = state.FillColor;
        figure.Thickness = state.Thickness;
        figure.Opacity = state.Opacity;
        figure.NotifyPropertyChanged();
    }
    
    protected FigureViewModel? FindFigure(CanvasViewModel canvas, Guid id) =>
        canvas.Layers.SelectMany(l => l.Figures).FirstOrDefault(f => f.Id == id);
    
    // Для доступа к canvas в Undo/Redo
    protected CanvasViewModel? canvas;
}