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
/// ViewModel для управления состоянием канваса: слои, фигуры, выделение, масштабирование.
/// Реализует реактивные свойства для привязки к UI через ReactiveUI.
/// </summary>
public class CanvasViewModel: ViewModelBase
{
    /// <summary>
    /// Коллекция слоёв проекта, доступная для привязки в UI.
    /// </summary>
    public ObservableCollection<LayerViewModel> Layers { get; }
    
    private static readonly ObservableCollection<FigureViewModel> _emptyFigures = new(); 
    /// <summary>Статическая пустая коллекция фигур для fallback-значений.</summary>
    
    /// <summary>
    /// Коллекция выделенных фигур для мульти-выделения (Ctrl+Click).
    /// </summary>
    public ObservableCollection<FigureViewModel> SelectedFigures = new ObservableCollection<FigureViewModel>();
    
    private FigureViewModel? _selectedFigure; 
    /// <summary>Единственная выбранная фигура для операций редактирования.</summary>
    
    private double _zoom = 1.0; 
    /// <summary>Текущий коэффициент масштабирования канваса (диапазон: 0.1–10.0).</summary>
    
    /// <summary>
    /// Проверяет, есть ли выделенные фигуры на канвасе.
    /// </summary>
    public bool HasSelection => SelectedFigures.Any();

    /// <summary>
    /// Инициализирует новый экземпляр CanvasViewModel с пустой коллекцией слоёв.
    /// </summary>
    public CanvasViewModel()
    {
        DebugLog.Write("[DEBUG] CanvasViewModel constructor");
        DebugLog.Write($"[DEBUG] StackTrace: {Environment.StackTrace}");
        Layers = new ObservableCollection<LayerViewModel>();
        DebugLog.Write($"[DEBUG] CanvasViewModel created: GetHashCode={this.GetHashCode()}");
    }
   
    /// <summary>
    /// Предварительная фигура для визуализации в процессе рисования.
    /// </summary>
    public FigureViewModel? PreviewFigure
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Активный слой, на котором выполняются операции добавления/удаления фигур.
    /// </summary>
    public LayerViewModel? ActiveLayer
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value, nameof(ActiveLayer));
            this.RaisePropertyChanged(nameof(IsCanvasActive));
        }
    }

    /// <summary>
    /// Флаг, указывающий, активен ли канвас для рисования (есть активный слой).
    /// </summary>
    public bool IsCanvasActive
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Выбранная фигура для редактирования. При смене автоматически обновляет коллекцию SelectedFigures.
    /// </summary>
    public FigureViewModel? SelectedFigure
    {
        get => _selectedFigure;
        set
        {
            if (_selectedFigure != null)
                _selectedFigure.IsSelected = false;
            this.RaiseAndSetIfChanged(ref _selectedFigure, value);
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
    
    /// <summary>
    /// Коэффициент масштабирования канваса с ограничением диапазона [0.1, 10.0].
    /// </summary>
    public double Zoom 
    {
        get => _zoom;
        set => this.RaiseAndSetIfChanged(ref _zoom, Math.Max(0.1, Math.Min(10.0, value)));
    }

    /// <summary>
    /// Смещение канваса по горизонтальной оси для реализации панорамирования.
    /// </summary>
    public double OffsetX
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Смещение канваса по вертикальной оси для реализации панорамирования.
    /// </summary>
    public double OffsetY
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Активирует канвас: создаёт новый слой, если активный отсутствует.
    /// </summary>
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

    /// <summary>
    /// Добавляет фигуру на активный слой, создавая слой при необходимости.
    /// </summary>
    /// <param name="figure">Экземпляр FigureViewModel для добавления.</param>
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

    /// <summary>
    /// Удаляет выбранную фигуру с активного слоя.
    /// </summary>
    public void RemoveSelectedFigure()
    {
        if (SelectedFigure != null && ActiveLayer != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            SelectedFigure = null;
        }
    }

    /// <summary>
    /// Дублирует выбранную фигуру со смещением (10, 10) и добавляет на активный слой.
    /// </summary>
    public void DuplicateSelectedFigure()
    {
        if (SelectedFigure != null)
        {
            var clone = SelectedFigure.Clone();
            clone.Move(10, 10);
            AddFigure(clone);
        }
    }

    /// <summary>
    /// Устанавливает предварительную фигуру для визуализации в процессе рисования.
    /// </summary>
    /// <param name="figure">Фигура для отображения или null для скрытия.</param>
    public void SetPreviewFigure(FigureViewModel? figure)
    {
        PreviewFigure = figure;
        this.RaisePropertyChanged(nameof(PreviewFigure));
    }

    /// <summary>
    /// Выбирает фигуру в заданной точке канваса с поддержкой мульти-выделения.
    /// </summary>
    /// <param name="point">Точка выбора в координатах канваса.</param>
    /// <param name="addToSelection">Если true, добавляет фигуру к текущему выделению (Ctrl+Click).</param>
    public void SelectFigureAt(Point2D point, bool addToSelection = false)
    {
        if (ActiveLayer == null) return;
        var figure = ActiveLayer.Figures.LastOrDefault(f => f.IsIn(point));
        if (addToSelection)
        {
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

    /// <summary>
    /// Снимает выделение с текущей фигуры.
    /// </summary>
    public void ClearFigure()
    {
        SelectedFigure = null;
    }

    /// <summary>
    /// Перемещает выбранную фигуру на заданный вектор.
    /// </summary>
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

    /// <summary>
    /// Вращает выбранную фигуру на заданный угол относительно центра.
    /// </summary>
    /// <param name="angle">Угол вращения в градусах.</param>
    public void RotateSelectedFigure(double angle)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Rotate(angle);
            this.RaisePropertyChanged(nameof(SelectedFigure));
        }
    }
    
    /// <summary>
    /// Масштабирует выбранную фигуру относительно центра с заданными коэффициентами.
    /// </summary>
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

    /// <summary>
    /// Перемещает выбранную фигуру на передний план (в конец коллекции отрисовки).
    /// </summary>
    public void BringToFront()
    {
        if (SelectedFigure != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            ActiveLayer.Figures.Add(SelectedFigure);
        }
    }
    
    /// <summary>
    /// Перемещает выбранную фигуру на задний план (в начало коллекции отрисовки).
    /// </summary>
    public void SendToBack()
    {
        if (SelectedFigure != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            ActiveLayer.Figures.Insert(0, SelectedFigure);
        }
    }
}