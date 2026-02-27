// ViewModels/ColorViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using Avalonia.Threading;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.Helpers;
    
namespace graphic_editor.ViewModels;

/// <summary>
/// Модель слоя холста/канваса, основывается на ViewModelBase.
/// </summary>
public class CanvasViewModel: ViewModelBase
{
	public ObservableCollection<LayerViewModel> Layers { get; } /// <summary>Публичная коллекция - рабочие слои.</summary>
	private static readonly ObservableCollection<FigureViewModel> _emptyFigures = new(); /// <summary>Инициализация приватной статичной коллекции пустых фигур.</summary>
    private FigureViewModel? _selectedFigure; /// <summary>Приватное свойство - выбранная фигура.</summary>
    private LayerViewModel? _activeLayer; /// <summary>Приватное свойство - активный слой.</summary>
	private FigureViewModel? _previewFigure; /// <summary>Приватное свойство - превью фигуры.</summary>
    private double _zoom = 1.0; /// <summary>Приватное свойство - коэффициент приближения.</summary>
    private double _offsetX; /// <summary>Приватное свойство - оффсет по оси X.</summary>
    private double _offsetY; /// <summary>Приватное свойство - оффсет по оси Y.</summary>
    private bool _isCanvasActive; /// <summary>Приватное свойство - флаг для проверки активности канваса.</summary>
	public bool HasSelection => _selectedFigure != null; /// <summary>Публичный флаг - проверка выбора фигуры.</summary>

	/// <summary>Конструктор CanvasViewModel.</summary>
    public CanvasViewModel()
    {
        DebugLog.Write("[DEBUG] CanvasViewModel constructor");
        DebugLog.Write($"[DEBUG] StackTrace: {Environment.StackTrace}");
        Layers = new ObservableCollection<LayerViewModel>();
        DebugLog.Write($"[DEBUG] CanvasViewModel created: GetHashCode={this.GetHashCode()}");
    }
   
	/// <summary>Публичное свойство для отображения фигуры.</summary>
    public FigureViewModel? PreviewFigure
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

	/// <summary>Публичное свойство - активный слой.</summary>
    public LayerViewModel? ActiveLayer
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value, nameof(ActiveLayer));
            this.RaisePropertyChanged(nameof(ActiveLayerFigures));
            this.RaisePropertyChanged(nameof(IsCanvasActive));
        }
    }

	/// <summary>Публичная коллекция активных фигур на слое.</summary>
    public ObservableCollection<FigureViewModel> ActiveLayerFigures => 
        ActiveLayer?.Figures ?? _emptyFigures;
    
	/// <summary>Публичное свойство - проверка активности канваса.</summary>
    public bool IsCanvasActive
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

	/// <summary>Публичное свойство - выбор фигуры на холсте.</summary>
    public FigureViewModel? SelectedFigure
    {
        get => _selectedFigure;
        set
        {
            // Снимаем выделение с предыдущей фигуры
        	if (_selectedFigure != null)
           		 _selectedFigure.IsSelected = false;

        	// Меняем выбранную фигуру
        	this.RaiseAndSetIfChanged(ref _selectedFigure, value);
            // Выделяем новую фигуру (если есть)
            if (_selectedFigure != null)
               	_selectedFigure.IsSelected = true;
            
            // Уведомляем о изменении HasSelection
            this.RaisePropertyChanged(nameof(HasSelection));
        	
        }
    }
    
	/// <summary>Публичное свойство - Zoom.</summary>
	public double Zoom 
    {
        get => _zoom;
        set => this.RaiseAndSetIfChanged(ref _zoom, Math.Max(0.1, Math.Min(10.0, value)));
    }

	/// <summary>Публичное свойство - OffsetX.</summary>
    public double OffsetX
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
	/// <summary>Публичное свойство - OffsetY.</summary>
    public double OffsetY
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

	/// <summary>Публичная функция - проверка активности канваса.</summary>
    public void ActivateCanvas()
    {
        DebugLog.Write($"[DEBUG] ActivateCanvas: ActiveLayer={ActiveLayer?.Name ?? "null"}, IsCanvasActive={IsCanvasActive}");
        if (ActiveLayer == null)
        {
            var newLayer = new LayerViewModel($"Слой {Layers.Count + 1}");
            Layers.Add(newLayer);
            ActiveLayer = newLayer;
            DebugLog.Write($"[DEBUG] Created layer: {newLayer.Name}");
            this.RaisePropertyChanged(nameof(Layers));
            this.RaisePropertyChanged(nameof(ActiveLayerFigures)); 
        }
        IsCanvasActive = true;
        this.RaisePropertyChanged(nameof(IsCanvasActive));
        DebugLog.Write($"[DEBUG] After ActivateCanvas: IsCanvasActive={IsCanvasActive}");
    }

	/// <summary>Публичная функция - добавление фигуры на слой.</summary>
    public void AddFigure(FigureViewModel figure)
    {
        DebugLog.Write($"[DEBUG] AddFigure: ActiveLayer={ActiveLayer?.Name ?? "null"}, Figure={figure?.Name}");
        DebugLog.Write($"[DEBUG] AddFigure in VM: {this.GetHashCode()}, ActiveLayer={ActiveLayer?.Name}");
    
        if (ActiveLayer == null) 
        {
            DebugLog.Write("[DEBUG] AddFigure: Calling ActivateCanvas");
            ActivateCanvas();
        }
    
        ActiveLayer?.Figures.Add(figure);
        SelectedFigure = figure;
        this.RaisePropertyChanged(nameof(ActiveLayerFigures));
    
        DebugLog.Write($"[DEBUG] AddFigure: ActiveLayer.Figures.Count={ActiveLayer?.Figures.Count}");
    }

	/// <summary>Публичная функция - удаление выбранной фигуры.</summary>
    public void RemoveSelectedFigure()
    {
        if (SelectedFigure != null && ActiveLayer != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            SelectedFigure = null;
            this.RaisePropertyChanged(nameof(ActiveLayerFigures));
        }
    }

	/// <summary>Публичная функция - дублирование выбранной фигуры.</summary>
    public void DuplicateSelectedFigure()
    {
        if (SelectedFigure != null)
        {
            var clone = SelectedFigure.Clone();
            clone.Move(10, 10);
            AddFigure(clone);
        }
    }

	/// <summary>Публичный метод для установки предварительной фигуры.</summary>
    public void SetPreviewFigure(FigureViewModel? figure)
    {
        PreviewFigure = figure;
        // Уведомляем контрол об изменении
        this.RaisePropertyChanged(nameof(PreviewFigure));
    }

	/// <summary>Публичный метод для выбора фигуры в точке.</summary>
    public void SelectFigureAt(Point_1 point)
    {
        if (ActiveLayer == null) return;
        var figure = ActiveLayer.Figures
            .LastOrDefault(f => f.IsIn(point));
        SelectedFigure = figure;
    }

	/// <summary>Публичный метод очистки выбранной фигуры.</summary>
    public void ClearFigure()
    {
        SelectedFigure = null;
    }

	/// <summary>Публичный метод переноса выбранной фигуры.</summary>
    public void MoveSelectedFigure(double dx, double dy)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Move(dx, dy);
            this.RaisePropertyChanged(nameof(SelectedFigure));
        }
    }

	/// <summary>Публичный метод поворота выбранной фигуры.</summary>
    public void RotateSelectedFigure(double angle)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Rotate(angle);
            this.RaisePropertyChanged(nameof(SelectedFigure));
        }
    }
    
	/// <summary>Публичный метод масштабирования выбранной фигуры.</summary>
    public void ScaleSelectedFigure(double sx, double sy)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Scale(sx, sy);
            this.RaisePropertyChanged(nameof(SelectedFigure));
        }
    }

	/// <summary>Публичный метод - перенос фигуры на передний фон.</summary>
    public void BringToFront()
    {
        if (SelectedFigure != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            ActiveLayer.Figures.Add(SelectedFigure);
        }
    }
    
	/// <summary>Публичный метод - перенос фигуры на задний фон.</summary>
    public void SendToBack()
    {
        if (SelectedFigure != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            ActiveLayer.Figures.Insert(0, SelectedFigure);
        }
    }
}