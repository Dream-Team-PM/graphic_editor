// ViewModels/Geometry/Figures/Groups_Methods/GroupViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using ReactiveUI;
using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Группа фигур — контейнер, позволяющий объединять несколько примитивов 
/// в один логический объект для совместного перемещения, масштабирования и т.д.
/// </summary>
public class GroupViewModel : FigureViewModel
{
    /// <summary>
    /// Коллекция дочерних фигур в группе.
    /// </summary>
    public ObservableCollection<FigureViewModel> Children { get; }

    /// <summary>
    /// Инициализирует новый экземпляр группы фигур.
    /// </summary>
    /// <param name="figures">Коллекция фигур для добавления в группу.</param>
    public GroupViewModel(IEnumerable<FigureViewModel> figures)
    {
        Name = "Группа";
        Children = new ObservableCollection<FigureViewModel>(figures);
        
        // Вычисляем bounding box группы
        UpdateBoundingBox();
        
        // Подписываемся на изменения детей
        foreach (var child in Children)
        {
            child.PropertyChanged += OnChildPropertyChanged;
        }
        Children.CollectionChanged += OnChildrenChanged;
    }

	/// <summary>
    /// Рекурсивно собирает все идентификаторы фигур в группе и вложенных группах.
    /// </summary>
    /// <returns>Список GUID всех фигур в иерархии группы.</returns>
	public List<Guid> GetAllFigureIds()
    {
        var ids = new List<Guid>();
        foreach (var child in Children)
        {
            if (child is GroupViewModel group)
            {
                ids.AddRange(group.GetAllFigureIds());
            }
            else
            {
                ids.Add(child.Id);
            }
        }
        return ids;
    }

	/// <summary>
    /// Применяет указанное действие ко всем фигурам в группе рекурсивно.
    /// </summary>
    /// <param name="action">Действие, применяемое к каждой фигуре.</param>
    public void ApplyToAllChildren(Action<FigureViewModel> action)
    {
        foreach (var child in Children)
        {
            if (child is GroupViewModel group)
            {
                group.ApplyToAllChildren(action);
            }
            else
            {
                action(child);
            }
        }
    }

	/// <summary>
    /// Создаёт глубокую копию группы и всех её дочерних фигур.
    /// </summary>
    /// <returns>Новый экземпляр <see cref="GroupViewModel"/> с клонированными детьми.</returns>
	public override FigureViewModel Clone()
    {
        var clonedChildren = Children.Select(c => c.Clone()).ToList();
        var clone = new GroupViewModel(clonedChildren)
        {
            LineColor = LineColor,
            FillColor = FillColor,
            Opacity = Opacity,
            Thickness = Thickness
        };
        return clone;
    }

    /// <summary>
    /// Обновляет ограничивающий прямоугольник группы на основе позиций дочерних фигур.
    /// </summary>
    private void UpdateBoundingBox()
    {
        if (!Children.Any()) return;
        
        var minX = Children.Min(f => f.Vertices.Min(v => v.X));
        var maxX = Children.Max(f => f.Vertices.Max(v => v.X));
        var minY = Children.Min(f => f.Vertices.Min(v => v.Y));
        var maxY = Children.Max(f => f.Vertices.Max(v => v.Y));
        
        // Обновляем Vertices группы (4 угла bounding box)
        Vertices.Clear();
        Vertices.Add(new PointViewModel(minX, minY));
        Vertices.Add(new PointViewModel(maxX, minY));
        Vertices.Add(new PointViewModel(maxX, maxY));
        Vertices.Add(new PointViewModel(minX, maxY));
        
        this.RaisePropertyChanged(nameof(Center));
    }

    /// <summary>
    /// Обработчик изменения свойств дочерних фигур.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения свойства.</param>
    private void OnChildPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PointViewModel.X) or nameof(PointViewModel.Y))
        {
            UpdateBoundingBox();
            this.RaisePropertyChanged(nameof(Center));
        }
    }

    /// <summary>
    /// Обработчик изменения состава коллекции дочерних фигур.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения коллекции.</param>
    private void OnChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateBoundingBox();
        this.RaisePropertyChanged(nameof(Center));
    }

    /// <summary>Центр группы</summary>
    public override Point2D Center => new Point2D(
        Vertices.Average(v => v.X),
        Vertices.Average(v => v.Y)
    );

    /// <summary>Перемещение всей группы</summary>
    public override void Move(double dx, double dy)
    {
        foreach (var child in Children)
        {
            child.Move(dx, dy);
        }
        UpdateBoundingBox();
    }

    /// <summary>Поворот группы вокруг центра</summary>
    public override void Rotate(double angle)
    {
        var center = Center;
        foreach (var child in Children)
        {
            child.Rotate(angle);
        }
        UpdateBoundingBox();
    }

    /// <summary>Масштабирование группы</summary>
    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var child in Children)
        {
            child.Scale(sx, sy);
        }
        UpdateBoundingBox();
    }

    /// <summary>Проверка попадания в группу (по bounding box)</summary>
    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        // Проверяем попадание в bounding box группы
        var minX = Vertices.Min(v => v.X) - eps;
        var maxX = Vertices.Max(v => v.X) + eps;
        var minY = Vertices.Min(v => v.Y) - eps;
        var maxY = Vertices.Max(v => v.Y) + eps;
        
        if (point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY)
            return false;
        
        // Если попали в bbox — проверяем детей
        return Children.Any(f => f.IsIn(point, eps));
    }

    /// <summary>Получение всех вершин группы</summary>
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Children.SelectMany(f => f.GetVertexPoint());
    }

    /// <summary>
    /// Разгруппировывает группу, возвращая список дочерних фигур.
    /// </summary>
    /// <returns>Коллекция фигур, ранее входивших в группу.</returns>
    public IEnumerable<FigureViewModel> Ungroup()
    {
        foreach (var child in Children)
        {
            child.PropertyChanged -= OnChildPropertyChanged;
        }
        Children.CollectionChanged -= OnChildrenChanged;
        return Children.ToList();
    }
    
    /// <summary>
    /// Возвращает координаты ограничивающего прямоугольника группы.
    /// </summary>
    /// <returns>Кортеж (MinX, MaxX, MinY, MaxY) с границами bbox.</returns>
    public (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox()
    {
        if (!Children.Any()) return (0, 0, 0, 0);
    
        var allX = Children.SelectMany(f => f.Vertices.Select(v => v.X)).ToList();
        var allY = Children.SelectMany(f => f.Vertices.Select(v => v.Y)).ToList();
    
        var minX = allX.Min();
        var maxX = allX.Max();
        var minY = allY.Min();
        var maxY = allY.Max();
    
        return (minX, maxX, minY, maxY);
    }
}