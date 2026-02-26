// ViewModel/FigureViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using ReactiveUI;

using graphic_editor.Models;

namespace graphic_editor.ViewModels;

/// <summary>
/// Основной публичный абстрактный класс для работы с геометрическими примитивами (основывается на ViewModelBase и IGraphicFigure).
/// </summary>
public abstract class FigureViewModel: ViewModelBase, IGraphicFigure
{
    private Guid _id; /// <summary>Приватное свойство - айди фигуры.</summary>
    private Color _lineColor = Color.Black; /// <summary>Приватное свойство - цвет линии.</summary>
    private Color _fillColor = Color.Transparent; /// <summary>Приватное свойство - цвет заполнения.</summary>
    private double _thickness = 1.0; /// <summary>Приватное свойство - толщина.</summary>
    private bool _isSelected; /// <summary>Приватное свойство - флаг выбранности.</summary>
    private string _name; /// <summary>Приватное свойство - имя фигурф.</summary>

	/// <summary>Защищённый конструктор создания фигуры.</summary>
    protected FigureViewModel()
    {
        _id = Guid.NewGuid();
        _name = GetType().Name.Replace("ViewModel", "");
        Vertices = new ObservableCollection<PointViewModel>();
    }
    
    public Guid Id => _id; /// <summary>Публичное свойство - получение айди фигуры.</summary>

	/// <summary>Публичное свойство - получение имени фигуры.</summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

	/// <summary>Публичное свойство - получение выбора фигуры.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

	/// <summary>Публичное свойство - получение цвета линии.</summary>
    public Color LineColor
    {
        get => _lineColor;
        set => this.RaiseAndSetIfChanged(ref _lineColor, value);
    }

	/// <summary>Публичное свойство - получение цвета заполнения.</summary>
    public Color FillColor
    {
        get => _fillColor;
        set => this.RaiseAndSetIfChanged(ref _fillColor, value);
    }

	/// <summary>Публичное свойство - получение толщины.</summary>
    public double Thickness
    {
        get => _thickness;
        set => this.RaiseAndSetIfChanged(ref _thickness, value);
    }
    
    public ObservableCollection<PointViewModel> Vertices { get; protected set;  } /// <summary>Публичная коллекция вершин.</summary>
    
    public abstract Point_1 Center { get; } /// <summary>Публичное абстрактное свойство центрирования фигуры.</summary>

    public abstract void Rotate(double angle); /// <summary>Публичный абстрактный метод вращения фигуры на угол.</summary>
    public abstract void Scale(double sx, double sy); /// <summary>Публичный абстрактный метод масштабирования фигуры.</summary>
    public abstract void Move(double dx, double dy); /// <summary>Публичный абстрактный метод перемещения фигуры.</summary>
    public abstract bool IsIn(Point_1 point, double eps = 0.001); /// <summary>Публичный абстрактный метод проверки нахождения в фигуре.</summary>
    public abstract IEnumerable<Point_1> GetVertexPoint(); /// <summary>Публичный абстрактный метод получения вершин фигуры.</summary>

    public virtual bool Select() => IsSelected = true; /// <summary>Публичное виртуальное свойство выбора фигуры.</summary>
    public virtual bool Deselect() => IsSelected = false; /// <summary>Публичное виртуальное свойство отмены выбора фигуры.</summary>

	/// <summary>Публичный виртуальный метод копирования (клонирования) фигуры.</summary>
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