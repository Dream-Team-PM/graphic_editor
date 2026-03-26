// Commands/FigureCommandBase.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using graphic_editor.Interfaces;

namespace graphic_editor.Commands;

/// <summary>
/// Базовый класс для команд, работающих с фигурами, с сохранением состояния для Undo/Redo.
/// </summary>
public abstract class FigureCommandBase : IHistoryAction
{
	/// <summary>
    /// Состояние фигур до выполнения команды (для Undo).
    /// </summary>
    protected readonly Dictionary<Guid, FigureState> _before = new();

	/// <summary>
    /// Состояние фигур после выполнения команды (для Redo).
    /// </summary>
    protected readonly Dictionary<Guid, FigureState> _after = new();
    
	/// <summary>
    /// Запись состояния фигуры: координаты, стиль, трансформации.
    /// </summary>
    /// <param name="MinX">Минимальная координата X ограничивающего прямоугольника.</param>
    /// <param name="MaxX">Максимальная координата X ограничивающего прямоугольника.</param>
    /// <param name="MinY">Минимальная координата Y ограничивающего прямоугольника.</param>
    /// <param name="MaxY">Максимальная координата Y ограничивающего прямоугольника.</param>
    /// <param name="Rotation">Угол поворота фигуры в градусах.</param>
    /// <param name="LineColor">Цвет обводки фигуры.</param>
    /// <param name="FillColor">Цвет заливки фигуры.</param>
    /// <param name="Thickness">Толщина обводки.</param>
    /// <param name="Opacity">Непрозрачность фигуры (0.0–1.0).</param>
    /// <param name="VertexCoordinates">Список координат вершин фигуры.</param>
    protected record FigureState(
        double MinX, double MaxX, double MinY, double MaxY, double Rotation,
        Color LineColor, Color FillColor, double Thickness, double Opacity,
        List<(double X, double Y)> VertexCoordinates);
    
	/// <inheritdoc/>
    public abstract string Description { get; }

	/// <inheritdoc/>
    public abstract void Execute(CanvasViewModel canvas);
    
	/// <summary>
    /// Отменяет команду: восстанавливает сохранённое состояние фигур "до".
    /// </summary>
    public virtual void Undo()
    {
        foreach (var (id, state) in _before)
            ApplyState(canvas, id, state);
    }
    
	/// <summary>
    /// Повторяет команду: применяет сохранённое состояние фигур "после".
    /// </summary>
    public virtual void Redo()
    {
        foreach (var (id, state) in _after)
            ApplyState(canvas, id, state);
    }
    
	/// <summary>
    /// Сохраняет текущее состояние фигуры в словарь _before.
    /// </summary>
    /// <param name="figure">Фигура для захвата состояния.</param>
    protected void CaptureBefore(FigureViewModel figure)
    {
        if (!_before.ContainsKey(figure.Id))
        {
            var bbox = figure.GetBoundingBox();
            var vertices = figure.Vertices.Select(v => (v.X, v.Y)).ToList();
            _before[figure.Id] = new FigureState(
                bbox.MinX, bbox.MaxX, bbox.MinY, bbox.MaxY, 
                figure.Rotation,
                figure.LineColor, figure.FillColor,
                figure.Thickness, figure.Opacity,
                vertices);
        }
    }
    
	/// <summary>
    /// Сохраняет текущее состояние фигуры в словарь _after.
    /// </summary>
    /// <param name="figure">Фигура для захвата состояния.</param>
    protected void CaptureAfter(FigureViewModel figure)
    {
        var bbox = figure.GetBoundingBox();
        var vertices = figure.Vertices.Select(v => (v.X, v.Y)).ToList();
        _after[figure.Id] = new FigureState(
            bbox.MinX, bbox.MaxX, bbox.MinY, bbox.MaxY, 
            figure.Rotation,
            figure.LineColor, figure.FillColor,
            figure.Thickness, figure.Opacity,
            vertices);
    }
    
	/// <summary>
    /// Применяет сохранённое состояние к фигуре по идентификатору.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для поиска фигуры.</param>
    /// <param name="id">Идентификатор целевой фигуры.</param>
    /// <param name="state">Состояние для применения.</param>
    private void ApplyState(CanvasViewModel canvas, Guid id, FigureState state)
    {
        var figure = FindFigure(canvas, id);
        if (figure == null) return;
        
        // Восстанавливаем позицию через координаты вершин
       if (state.VertexCoordinates != null && state.VertexCoordinates.Count == figure.Vertices.Count)
        {
            for (int i = 0; i < figure.Vertices.Count; i++)
            {
                figure.Vertices[i].X = state.VertexCoordinates[i].X;
                figure.Vertices[i].Y = state.VertexCoordinates[i].Y;
				figure.Vertices[i].NotifyPropertyChanged();
            }
        }

		// Восстанавливаем угол поворота
    	var angleDiff = state.Rotation - figure.Rotation;
    	if (Math.Abs(angleDiff) > 0.01 && figure is not GroupViewModel)
    	{
        	figure.Rotate(angleDiff);
    	}
    	else
    	{
        	figure.Rotation = state.Rotation;
        	figure.NotifyPropertyChanged();
    	}
        
        // Восстанавливаем стиль
        figure.LineColor = state.LineColor;
        figure.FillColor = state.FillColor;
        figure.Thickness = state.Thickness;
        figure.Opacity = state.Opacity;
        figure.NotifyPropertyChanged();
    }
    
	/// <summary>
    /// Находит фигуру по идентификатору во всех слоях холста.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для поиска.</param>
    /// <param name="id">Идентификатор искомой фигуры.</param>
    /// <returns>Найденная фигура или null.</returns>
    protected FigureViewModel? FindFigure(CanvasViewModel canvas, Guid id) =>
        canvas.Layers.SelectMany(l => l.Figures).FirstOrDefault(f => f.Id == id);
    
    /// <summary>
    /// Ссылка на CanvasViewModel для доступа в методах Undo/Redo.
    /// </summary>
    protected CanvasViewModel? canvas;
}