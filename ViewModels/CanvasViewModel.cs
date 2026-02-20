// ViewModels/ColorViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using graphic_editor.Models;
using System.Drawing;
    
namespace graphic_editor.ViewModels;
public class CanvasViewModel: ViewModelBase
{
    private FigureViewModel? _selectedFigure;
    private double _zoom = 1.0;
    private double _offsetX;
    private double _offsetY;

    public CanvasViewModel()
    {
        Figures = new ObservableCollection<FigureViewModel>();
    }
    public ObservableCollection<FigureViewModel> Figures { get; }
    public FigureViewModel? SelectedFigure
    {
        get => _selectedFigure;
        set
        {
            if (_selectedFigure != null)
                _selectedFigure.Deselect();

            if (SetProperty(ref _selectedFigure, value))
            {
                if (_selectedFigure != null)
                    _selectedFigure.Select();

                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(SelectedFigureProperties));
            }
        }
    }

    public bool HasSelection => _selectedFigure != null;
    public object? SelectedFigureProperties => SelectedFigure;
    public double Zoom 
    {
        get => _zoom;
        set => SetProperty(ref _zoom, Math.Max(0.1, Math.Min(10.0, value)));
    }

    public double OffsetX
    {
        get => _offsetX;
        set => SetProperty(ref _offsetX, value);
    }
    
    public double OffsetY
    {
        get => _offsetY;
        set => SetProperty(ref _offsetY, value);
    }

    public void AddFigure(FigureViewModel figure)
    {
        Figures.Add(figure);
        SelectedFigure = figure;
    }

    public void RemoveSelectedFigure()
    {
        if (SelectedFigure != null)
        {
            Figures.Remove(SelectedFigure);
            SelectedFigure = null;
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
        var figure = Figures
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
            OnPropertyChanged(nameof(SelectedFigureProperties));
        }
    }

    public void RotateSelectedFigure(double angle)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Rotate(angle);
            OnPropertyChanged(nameof(SelectedFigureProperties));
        }
    }
    
    public void ScaleSelectedFigure(double sx, double sy)
    {
        if (SelectedFigure != null)
        {
            SelectedFigure.Scale(sx, sy);
            OnPropertyChanged(nameof(SelectedFigureProperties));
        }
    }

    public void BringToFront()
    {
        if (SelectedFigure != null)
        {
            Figures.Remove(SelectedFigure);
            Figures.Add(SelectedFigure);
        }
    }
    
    public void SendToBack()
    {
        if (SelectedFigure != null)
        {
            Figures.Remove(SelectedFigure);
            Figures.Insert(0, SelectedFigure);
        }
    }
}