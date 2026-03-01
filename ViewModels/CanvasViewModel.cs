// ViewModels/ColorViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using Avalonia.Threading;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.Helpers;
using graphic_editor.Geometry;
    
namespace graphic_editor.ViewModels;

/// <summary>
/// Модель слоя холста/канваса, основывается на ViewModelBase.
/// </summary>
public class CanvasViewModel: ViewModelBase
{
	public ObservableCollection<LayerViewModel> Layers { get; } /// <summary>Публичная коллекция - рабочие слои.</summary>
	private static readonly ObservableCollection<FigureViewModel> _emptyFigures = new(); /// <summary>Инициализация приватной статичной коллекции пустых фигур.</summary>
    public ObservableCollection<FigureViewModel> SelectedFigures = new ObservableCollection<FigureViewModel>();
    private FigureViewModel? _selectedFigure; /// <summary>Приватное свойство - выбранная фигура.</summary>
    private double _zoom = 1.0; /// <summary>Приватное свойство - коэффициент приближения.</summary>
	public bool HasSelection => SelectedFigures.Any(); /// <summary>Публичный флаг - проверка выбора фигуры.</summary>

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
            this.RaisePropertyChanged(nameof(IsCanvasActive));
        }
    }

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
            if (_selectedFigure != null)
                _selectedFigure.IsSelected = false;
            // Меняем выбранную фигуру
            this.RaiseAndSetIfChanged(ref _selectedFigure, value);
            // Выделяем новую фигуру
            if (_selectedFigure != null)
            {
                _selectedFigure.IsSelected = true;
                SelectedFigures.Clear();
                SelectedFigures.Add(_selectedFigure);
                this.RaisePropertyChanged(nameof(SelectedFigures));
            }
            else
            {
                SelectedFigures.Clear();
                this.RaisePropertyChanged(nameof(SelectedFigures));
            }
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
        }
        IsCanvasActive = true;
        this.RaisePropertyChanged(nameof(IsCanvasActive));
        DebugLog.Write($"[DEBUG] After ActivateCanvas: IsCanvasActive={IsCanvasActive}");
    }

	/// <summary>Публичный метод для добавления фигуры на активный слой.</summary>
    /// <param name="figure">Фигура для добавления на слой.</param>
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
        DebugLog.Write($"[DEBUG] AddFigure: ActiveLayer.Figures.Count={ActiveLayer?.Figures.Count}");
    }

	/// <summary>Публичная функция - удаление выбранной фигуры.</summary>
    public void RemoveSelectedFigure()
    {
        if (SelectedFigure != null && ActiveLayer != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            SelectedFigure = null;
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
        this.RaisePropertyChanged(nameof(PreviewFigure));
    }

	/// <summary>Публичный метод для выбора фигуры в точке.</summary>
    /// <param name="point">Точка на канвасе, в которой происходит выбор.</param>
    /// <param name="addToSelection">Флаг: добавить к текущему выделению (Ctrl+Click).</param>
    public void SelectFigureAt(Point2D point, bool addToSelection = false)
    {
        if (ActiveLayer == null) return;
        var figure = ActiveLayer.Figures
            .LastOrDefault(f => f.IsIn(point));
        if (addToSelection)
        {
            // Добавляем/удаляем из мульти-выделения (Ctrl+Click)
            if (figure != null)
            {
                if (SelectedFigures.Contains(figure))
                {
                    figure.IsSelected = false;
                    SelectedFigures.Remove(figure);
                }
                else
                {
                    figure.IsSelected = true;
                    SelectedFigures.Add(figure);
                }
            }
            this.RaisePropertyChanged(nameof(SelectedFigures));
        }
        else
        {
            // Обычное выделение одной фигуры (снимает остальные)
            foreach (var f in SelectedFigures)
                f.IsSelected = false;
            SelectedFigures.Clear();
            if (figure != null)
            {
                figure.IsSelected = true;
                SelectedFigures.Add(figure);
                SelectedFigure = figure;
                DebugLog.Write("[DEBUG] I am not null");
            }
            else
            {
                SelectedFigure = null;
                DebugLog.Write("[DEBUG] I am null");
            }
            this.RaisePropertyChanged(nameof(SelectedFigures));
        }
        this.RaisePropertyChanged(nameof(HasSelection));
    }

	/// <summary>Публичный метод очистки выбранной фигуры.</summary>
    public void ClearFigure()
    {
        SelectedFigure = null;
    }

	/// <summary>Публичный метод для перемещения выбранной фигуры.</summary>
    /// <param name="dx">Смещение по оси X.</param>
    /// <param name="dy">Смещение по оси Y.</param>
    public void MoveSelectedFigure(double dx, double dy)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Move(dx, dy);
            this.RaisePropertyChanged(nameof(SelectedFigure));
        }
    }

	/// <summary>Публичный метод для поворота выбранной фигуры.</summary>
    /// <param name="angle">Угол поворота в градусах.</param>
    public void RotateSelectedFigure(double angle)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Rotate(angle);
            this.RaisePropertyChanged(nameof(SelectedFigure));
        }
    }
    
	/// <summary>Публичный метод для масштабирования выбранной фигуры.</summary>
    /// <param name="sx">Коэффициент масштабирования по оси X.</param>
    /// <param name="sy">Коэффициент масштабирования по оси Y.</param>
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