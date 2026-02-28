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
public abstract class FigureViewModel: ViewModelBase, IGraphicFigure, IFigure
{
    private Guid _id; /// <summary>Приватное свойство - айди фигуры.</summary>
    //private Color _lineColor = Color.Black; /// <summary>Приватное свойство - цвет линии.</summary>
    //private Color _fillColor = Color.Transparent; /// <summary>Приватное свойство - цвет заполнения.</summary>
    private double _opacity = 1.0;
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
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

	/// <summary>Публичное свойство - получение цвета линии.</summary>
    public Color LineColor
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

	/// <summary>Публичное свойство - получение цвета заполнения.</summary>
    public Color FillColor
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

	/// <summary>Публичное свойство - получение толщины.</summary>
    public double Thickness
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Публичное свойство - получение размера прозрачности.</summary>
    public double Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }
    
    public ObservableCollection<PointViewModel> Vertices { get; protected set;  } /// <summary>Публичная коллекция вершин.</summary>
    
    public abstract Point_1 Center { get; } /// <summary>Публичное абстрактное свойство центрирования фигуры.</summary>
	IEnumerable<Point_1> IFigure.Vertices => GetVertexPoint(); /// <summary>Публичный абстрактный метод получения вершин фигуры.</summary>

    public abstract void Rotate(double angle); /// <summary>Публичный абстрактный метод вращения фигуры на угол.</summary>
    public abstract void Scale(double sx, double sy); /// <summary>Публичный абстрактный метод масштабирования фигуры.</summary>
    public abstract void Move(double dx, double dy); /// <summary>Публичный абстрактный метод перемещения фигуры.</summary>
    public abstract bool IsIn(Point_1 point, double eps = 0.001); /// <summary>Публичный абстрактный метод проверки нахождения в фигуре.</summary>
    public virtual void RadialScale(double scale) => Scale(scale, scale);

	public virtual void Reflection(Point_1 a, Point_1 b)
    {
        // Базовая реализация отражения
        var center = Center;
        foreach (var vertex in Vertices)
        {
            var p = vertex.ToPoint();
            var reflected = ReflectPoint(p, a, b);
            vertex.X = reflected.X;
            vertex.Y = reflected.Y;
        }
    }
    
    public virtual bool HasIntersection(Point_1 leftTop, Point_1 rightBottom)
    {
        var bounds = GetBoundingBox();
        double minX = Math.Min(leftTop.X, rightBottom.X);
        double maxX = Math.Max(leftTop.X, rightBottom.X);
        double minY = Math.Min(leftTop.Y, rightBottom.Y);
        double maxY = Math.Max(leftTop.Y, rightBottom.Y);
        
        return !(bounds.MaxX < minX || bounds.MinX > maxX || 
                 bounds.MaxY < minY || bounds.MinY > maxY);
    }

	/// <summary>Публичный виртуальный метод копирования (клонирования) фигуры.</summary>
	IFigure IFigure.Clone() => Clone();
    
    // Абстрактные методы для конкретных фигур
    public abstract IEnumerable<Point_1> GetVertexPoint();
    public virtual FigureViewModel Clone() => (FigureViewModel)MemberwiseClone();
    
    // Вспомогательные методы
    protected virtual (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox()
    {
        var points = GetVertexPoint().ToList();
        return (
            points.Min(p => p.X),
            points.Max(p => p.X),
            points.Min(p => p.Y),
            points.Max(p => p.Y)
        );
    }

	protected Point_1 ReflectPoint(Point_1 p, Point_1 a, Point_1 b)
    {
        var d = b - a;
        double A = d.Y;
        double B = -d.X;
        double C = d.X * a.Y - d.Y * a.X;
        double D = (A * p.X + B * p.Y + C) / (A * A + B * B);
        
        return new Point_1(
            p.X - 2 * A * D,
            p.Y - 2 * B * D
        );
    }
}