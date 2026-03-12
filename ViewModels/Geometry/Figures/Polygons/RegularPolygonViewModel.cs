// ViewModels/Geometry/Figures/Polygons/RegularPolygonViewModel.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using ReactiveUI;

namespace graphic_editor.Geometry;

/// <summary>
/// Абстрактный базовый класс для правильных многоугольников 
/// (все стороны и углы равны).
/// </summary>
public abstract class RegularPolygonViewModel : PolygonViewModel
{
    /// <summary>
    /// Количество сторон многоугольника.
    /// </summary>
    public int Sides { get; }
    
    /// <summary>
    /// Радиус описанной окружности многоугольника.
    /// </summary>
    public double Radius { get; }

    /// <summary>
    /// Инициализирует новый экземпляр правильного многоугольника.
    /// </summary>
    /// <param name="center">Центр многоугольника.</param>
    /// <param name="sides">Количество сторон (минимум 3).</param>
    /// <param name="radius">Радиус описанной окружности.</param>
    /// <param name="lineColor">Цвет обводки.</param>
    /// <param name="thickness">Толщина обводки.</param>
    /// <param name="fillColor">Цвет заливки.</param>
    /// <param name="opacity">Непрозрачность (0.0–1.0).</param>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если <paramref name="sides"/> &lt; 3.
    /// </exception>
    protected RegularPolygonViewModel(Point2D center, int sides, double radius,
        Color lineColor, double thickness, Color fillColor, double opacity)
        : base(CreateVertices(center, sides, radius), 
            lineColor, thickness, fillColor, opacity)
    {
        if (sides < 3)
            throw new ArgumentException("Polygon must have at least 3 sides.");
        
        Sides = sides;
        Radius = radius;
    }

    /// <summary>
    /// Создаёт коллекцию вершин правильного многоугольника, 
    /// равномерно распределённых по окружности.
    /// </summary>
    /// <param name="center">Центр многоугольника.</param>
    /// <param name="sides">Количество сторон.</param>
    /// <param name="radius">Радиус описанной окружности.</param>
    /// <returns>Перечислимая коллекция точек вершин.</returns>
    private static IEnumerable<Point2D> CreateVertices(Point2D center, int sides, double radius)
    {
        var points = new List<Point2D>();
        double angleStep = 2 * Math.PI / sides;
        double startAngle = -Math.PI / 2; // Начинаем с верха

        for (int i = 0; i < sides; i++)
        {
            double angle = startAngle + i * angleStep;
            points.Add(new Point2D(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)
            ));
        }

        return points;
    }
    
    /// <summary>
    /// Пересчитывает координаты вершин многоугольника при изменении радиуса.
    /// </summary>
    /// <param name="center">Новый центр многоугольника.</param>
    /// <param name="newRadius">Новый радиус описанной окружности.</param>
    public void UpdateVertices(Point2D center, double newRadius)
    {
        double angleStep = 2 * Math.PI / Sides;
        double startAngle = -Math.PI / 2;

        for (int i = 0; i < Vertices.Count && i < Sides; i++)
        {
            double angle = startAngle + i * angleStep;
            Vertices[i].X = center.X + newRadius * Math.Cos(angle);
            Vertices[i].Y = center.Y + newRadius * Math.Sin(angle);
        }
    
        // Уведомляем об изменении каждой вершины
        foreach (var vertex in Vertices)
        {
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
    
        this.RaisePropertyChanged(nameof(Center));
        this.RaisePropertyChanged(nameof(Vertices));
    }
}