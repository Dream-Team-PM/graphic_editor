// ViewModels/Geometry/Figures/Polygons/PentagramViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;

using ReactiveUI;

using graphic_editor.Models;
using graphic_editor.ViewModels;

namespace graphic_editor.Geometry;

/// <summary>
/// Класс пентаграммы (пятиконечной звезды) — специальный случай многоугольника.
/// </summary>
public class PentagramViewModel : PolygonViewModel
{
    /// <summary>
    /// Внешний радиус пентаграммы (расстояние от центра до внешних вершин).
    /// </summary>
    public double OuterRadius { get; }

    /// <summary>
    /// Инициализирует новый экземпляр пентаграммы.
    /// </summary>
    /// <param name="center">Центр пентаграммы.</param>
    /// <param name="outerRadius">Внешний радиус.</param>
    /// <param name="lineColor">Цвет обводки.</param>
    /// <param name="thickness">Толщина обводки.</param>
    /// <param name="fillColor">Цвет заливки.</param>
    /// <param name="opacity">Непрозрачность (0.0–1.0).</param>
    public PentagramViewModel(Point2D center, double outerRadius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(CreateVertices(center, outerRadius), 
            lineColor, thickness, fillColor, opacity)
    {
        Name = "Пентаграмма";
        OuterRadius = outerRadius;
    }

    /// <summary>
    /// Создаёт 10 вершин пентаграммы (5 внешних + 5 внутренних).
    /// </summary>
    /// <param name="center">Центр звезды.</param>
    /// <param name="R">Внешний радиус.</param>
    /// <returns>Перечислимая коллекция точек вершин.</returns>
    private static IEnumerable<Point2D> CreateVertices(Point2D center, double R)
    {
        var points = new List<Point2D>();
        double r = R * 0.382; // Внутренний радиус для правильной звезды

        for (int i = 0; i < 10; i++)
        {
            double angle = i * Math.PI / 5;
            double radius = (i % 2 == 0) ? R : r; // Чётные = внешние, нечётные = внутренние

            points.Add(new Point2D(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            ));
        }

        return points;
    }
    
    public override IEnumerable<Point2D> GetVertexPoint()
    {
        return Vertices.Select(v => v.ToPoint());
    }
    
    /// <summary>
    /// Пересчитывает координаты вершин пентаграммы при изменении внешнего радиуса.
    /// </summary>
    /// <param name="center">Новый центр пентаграммы.</param>
    /// <param name="newOuterRadius">Новый внешний радиус.</param>
    public void UpdateVertices(Point2D center, double newOuterRadius)
    {
        var points = CreateVertices(center, newOuterRadius).ToList();
    
        for (int i = 0; i < Vertices.Count && i < points.Count; i++)
        {
            Vertices[i].X = points[i].X;
            Vertices[i].Y = points[i].Y;
        }
    
        // Уведомляем об изменении каждой вершины
        foreach (var vertex in Vertices)
        {
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
    
        this.RaisePropertyChanged(nameof(Center));
        this.RaisePropertyChanged(nameof(OuterRadius));
        this.RaisePropertyChanged(nameof(Vertices));
    }

    /// <summary>
    /// Уведомляет об изменении геометрических свойств, включая OuterRadius.
    /// </summary>
    protected new void NotifyPropertyChanged()
    {
        base.NotifyPropertyChanged();
        this.RaisePropertyChanged(nameof(OuterRadius));
    }

    /// <summary>
    /// Создает клон фигуры.
    /// </summary>
    public override FigureViewModel Clone()
    {
        var clone = new PentagramViewModel(
            new Point2D(Center.X, Center.Y),
            OuterRadius,
            LineColor,
            Thickness,
            FillColor,
            Opacity);

        return clone;
    }
}