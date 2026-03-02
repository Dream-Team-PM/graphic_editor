// ViewModels/Geometry/FigureViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.Interfaces;
using graphic_editor.Geometry;

namespace graphic_editor.ViewModels;

/// <summary>
/// Основной публичный абстрактный класс для работы с геометрическими примитивами (основывается на ViewModelBase и ITransformable, ISelectable, ICloneableFigure, IRenderable, IFigure).
/// </summary>
public abstract class FigureViewModel: ViewModelBase, ITransformable, ISelectable, ICloneableFigure, IRenderable, IFigure
{
    private Guid _id; /// <summary>Приватное свойство - айди фигуры.</summary>
    private double _opacity = 1.0;
    public double _rotation = 0;
    private string _name; /// <summary>Приватное свойство - имя фигуры.</summary>

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
    
    public double Rotation
    {
        get => _rotation;
        set => this.RaiseAndSetIfChanged(ref _rotation, value);
    }
    
    public ObservableCollection<PointViewModel> Vertices { get; protected set;  } /// <summary>Публичная коллекция вершин.</summary>
    // Абстрактные методы для конкретных фигур
    public IEnumerable<Point2D> GetRenderVertices() => GetVertexPoint();
    IEnumerable<Point2D> IFigure.Vertices => GetVertexPoint();
    public virtual FigureViewModel Clone() => (FigureViewModel)MemberwiseClone();
    IFigure ICloneableFigure.Clone() => Clone();
    
    public abstract Point2D Center { get; } /// <summary>Публичное абстрактное свойство центрирования фигуры.</summary>
	public abstract IEnumerable<Point2D> GetVertexPoint(); /// <summary>Публичный абстрактный метод получения вершин фигуры.</summary>

    public abstract void Rotate(double angle); /// <summary>Публичный абстрактный метод вращения фигуры на угол.</summary>
    public abstract void Scale(double sx, double sy); /// <summary>Публичный абстрактный метод масштабирования фигуры.</summary>
    public abstract void Move(double dx, double dy); /// <summary>Публичный абстрактный метод перемещения фигуры.</summary>
    public abstract bool IsIn(Point2D point, double eps = 0.001); /// <summary>Публичный абстрактный метод проверки нахождения в фигуре.</summary>
    public virtual void RadialScale(double scale) => Scale(scale, scale);

	public virtual void Reflection(Point2D a, Point2D b)
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
    
    public virtual bool HasIntersection(Point2D leftTop, Point2D rightBottom)
    {
        var bounds = GetBoundingBox();
        double minX = Math.Min(leftTop.X, rightBottom.X);
        double maxX = Math.Max(leftTop.X, rightBottom.X);
        double minY = Math.Min(leftTop.Y, rightBottom.Y);
        double maxY = Math.Max(leftTop.Y, rightBottom.Y);
        
        return !(bounds.MaxX < minX || bounds.MinX > maxX || 
                 bounds.MaxY < minY || bounds.MinY > maxY);
    }

    // Вспомогательные методы
    public virtual (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox()
    {
        //var vertices = GetVertexPoint().ToList();
        var vertices = Vertices.Select(v => v.ToPoint()).ToList();
        return (
            vertices.Min(p => p.X),
            vertices.Max(p => p.X),
            vertices.Min(p => p.Y),
            vertices.Max(p => p.Y)
        );
    }

    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(Vertices));
    }

	protected Point2D ReflectPoint(Point2D p, Point2D a, Point2D b) => p.Reflect(a, b);
}