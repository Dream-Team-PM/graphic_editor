// ViewModel/FigureViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using ReactiveUI;

using graphic_editor.Models;

namespace graphic_editor.ViewModels;

public abstract class FigureViewModel: ViewModelBase, IGraphicFigure
{
    private Guid _id;
    private Color _lineColor = Color.Black;
    private Color _fillColor = Color.Transparent;
    private double _thickness = 1.0;
    private bool _isSelected;
    private string _name;

    protected FigureViewModel()
    {
        _id = Guid.NewGuid();
        _name = GetType().Name.Replace("ViewModel", "");
        Vertices = new ObservableCollection<PointViewModel>();
    }
    
    public Guid Id => _id;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public Color LineColor
    {
        get => _lineColor;
        set => this.RaiseAndSetIfChanged(ref _lineColor, value);
    }

    public Color FillColor
    {
        get => _fillColor;
        set => this.RaiseAndSetIfChanged(ref _fillColor, value);
    }

    public double Thickness
    {
        get => _thickness;
        set => this.RaiseAndSetIfChanged(ref _thickness, value);
    }
    
    public ObservableCollection<PointViewModel> Vertices { get; protected set;  }
    
    public abstract Point_1 Center { get; }

    public abstract void Rotate(double angle);
    public abstract void Scale(double sx, double sy);
    public abstract void Move(double dx, double dy);
    public abstract bool IsIn(Point_1 point, double eps = 0.001);
    public abstract IEnumerable<Point_1> GetVertexPoint();

    public virtual bool Select() => IsSelected = true;
    public virtual bool Deselect() => IsSelected = false;

    public virtual FigureViewModel Clone()
    {
        var clone = (FigureViewModel)MemberwiseClone();
        clone._id = Guid.NewGuid();
        clone.Vertices = new ObservableCollection<PointViewModel>(
            Vertices.Select(v => new PointViewModel(v.X, v.Y))
        );
        return clone;
    }
}