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
public class CanvasViewModel: ViewModelBase
{
    private FigureViewModel? _selectedFigure;
    private LayerViewModel? _activeLayer;
    private double _zoom = 1.0;
    private double _offsetX;
    private double _offsetY;
    private bool _isCanvasActive;

    public CanvasViewModel()
    {
        DebugLog.Write("[DEBUG] CanvasViewModel constructor");
        DebugLog.Write($"[DEBUG] StackTrace: {Environment.StackTrace}");
        Console.Out.Flush();
        Layers = new ObservableCollection<LayerViewModel>();
        DebugLog.Write($"[DEBUG] CanvasViewModel created: GetHashCode={this.GetHashCode()}");
    }
    public ObservableCollection<LayerViewModel> Layers { get; }
    
    public LayerViewModel? ActiveLayer
    {
        get => _activeLayer;
        set
        {
            this.RaiseAndSetIfChanged(ref _activeLayer, value, nameof(ActiveLayer));
            this.RaisePropertyChanged(nameof(ActiveLayerFigures));
            this.RaisePropertyChanged(nameof(IsCanvasActive));
        }
    }
    
    private static readonly ObservableCollection<FigureViewModel> _emptyFigures = new();

    public ObservableCollection<FigureViewModel> ActiveLayerFigures => 
        ActiveLayer?.Figures ?? _emptyFigures;
    
    public bool IsCanvasActive
    {
        get => _isCanvasActive;
        set => this.RaiseAndSetIfChanged(ref _isCanvasActive, value);
    }
    public FigureViewModel? SelectedFigure
    {
        get => _selectedFigure;
        set
        {
            if (_selectedFigure != null)
                _selectedFigure.Deselect();

            this.RaiseAndSetIfChanged(ref _selectedFigure, value, nameof(SelectedFigure));
            if (_selectedFigure != null)
                _selectedFigure.Select();

            this.RaisePropertyChanged(nameof(HasSelection));
            this.RaisePropertyChanged(nameof(SelectedFigureProperties));
        }
    }

    public bool HasSelection => _selectedFigure != null;
    public object? SelectedFigureProperties => SelectedFigure;
    public double Zoom 
    {
        get => _zoom;
        set => this.RaiseAndSetIfChanged(ref _zoom, Math.Max(0.1, Math.Min(10.0, value)));
    }

    public double OffsetX
    {
        get => _offsetX;
        set => this.RaiseAndSetIfChanged(ref _offsetX, value);
    }
    
    public double OffsetY
    {
        get => _offsetY;
        set => this.RaiseAndSetIfChanged(ref _offsetY, value);
    }

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

    public void RemoveSelectedFigure()
    {
        if (SelectedFigure != null && ActiveLayer != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            SelectedFigure = null;
            this.RaisePropertyChanged(nameof(ActiveLayerFigures));
        }
    }

    public void DuplicateSelectedFigure()
    {
        if (SelectedFigure != null)
        {
            var clone = SelectedFigure.Clone();
            clone.Move(10, 10);
            AddFigure(clone);
        }
    }

    public void SelectFigureAt(Point_1 point)
    {
        if (ActiveLayer == null) return;
        var figure = ActiveLayer.Figures
            .Reverse()
            .FirstOrDefault(f => f.IsIn(point));
        SelectedFigure = figure;
    }

    public void ClearFigure()
    {
        SelectedFigure = null;
    }

    public void MoveSelectedFigure(double dx, double dy)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Move(dx, dy);
            this.RaisePropertyChanged(nameof(SelectedFigureProperties));
        }
    }

    public void RotateSelectedFigure(double angle)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Rotate(angle);
            this.RaisePropertyChanged(nameof(SelectedFigureProperties));
        }
    }
    
    public void ScaleSelectedFigure(double sx, double sy)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Scale(sx, sy);
            this.RaisePropertyChanged(nameof(SelectedFigureProperties));
        }
    }

    public void BringToFront()
    {
        if (SelectedFigure != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            ActiveLayer.Figures.Add(SelectedFigure);
        }
    }
    
    public void SendToBack()
    {
        if (SelectedFigure != null)
        {
            ActiveLayer.Figures.Remove(SelectedFigure);
            ActiveLayer.Figures.Insert(0, SelectedFigure);
        }
    }
}