// ViewModels/State/DrawingSession.cs
using ReactiveUI;
using graphic_editor.Geometry;
using graphic_editor.Tools;
using graphic_editor.ViewModels;
using graphic_editor.Models;
using graphic_editor.Interfaces;

namespace graphic_editor.State;

/// <summary>Инкапсулирует состояние процесса рисования.</summary>
public class DrawingSession : ReactiveObject
{
    private bool _isActive;
    private FigureViewModel? _preview;
    private readonly List<Point2D> _points = new();
    
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }
    
    public DrawingTool Tool { get; private set; }
    public Point2D StartPoint { get; private set; }
    public IDrawingStrategy? Strategy { get; private set; }
    
    public FigureViewModel? Preview
    {
        get => _preview;
        private set => this.RaiseAndSetIfChanged(ref _preview, value);
    }
    
    public IReadOnlyList<Point2D> Points => _points;
    
    /// <summary>Начать новую сессию рисования.</summary>
    public void Start(Point2D startPoint, DrawingTool tool, IDrawingStrategy strategy)
    {
        if (IsActive) Cancel();
        
        Tool = tool;
        StartPoint = startPoint;
        Strategy = strategy;
        _points.Clear();
        _points.Add(startPoint);
        IsActive = true;
        
        Preview = strategy.CreatePreview(startPoint, startPoint, GetCurrentStyle());
    }
    
    /// <summary>Обновить предварительный просмотр при движении мыши.</summary>
    public void Update(Point2D currentPoint)
    {
        if (!IsActive || Strategy == null || Preview == null) return;
        
        if (Strategy.RequiresMultiClick)
        {
            // Для Pen: preview = последняя точка
            Strategy.UpdatePreview(Preview, _points.Last(), currentPoint);
        }
        else if (Strategy.RequiresDrag)
        {
            // Для примитивов: preview = start → current
            Strategy.UpdatePreview(Preview, StartPoint, currentPoint);
        }
    }
    
    /// <summary>Добавить точку (для multi-click инструментов).</summary>
    public void AddPoint(Point2D point)
    {
        if (!IsActive || !Strategy?.RequiresMultiClick == true) return;
        _points.Add(point);
    }
    
    /// <summary>Завершить рисование и вернуть финальную фигуру.</summary>
    public FigureViewModel? Finish(Point2D endPoint)
    {
        if (!IsActive || Strategy == null) return null;
        
        if (Strategy.RequiresMultiClick)
        {
            Strategy.UpdatePreview(Preview, _points.First(), endPoint);
        }
        else
        {
            Strategy.UpdatePreview(Preview, StartPoint, endPoint);
        }
        
        Reset();
        return Preview;
    }
    
    /// <summary>Отменить текущую сессию.</summary>
    public void Cancel()
    {
        Reset();
    }
    
    private void Reset()
    {
        IsActive = false;
        Preview = null;
        Strategy = null;
        _points.Clear();
    }
    
    // В реальной реализации: получать стиль из сервиса/ViewModel
    private StyleSettings GetCurrentStyle() => StyleSettings.Default; 
}