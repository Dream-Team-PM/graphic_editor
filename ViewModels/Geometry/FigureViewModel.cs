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
/// Абстрактный базовый класс для всех геометрических примитивов в редакторе.
/// Реализует интерфейсы: <see cref="ITransformable"/>, <see cref="ISelectable"/>, 
/// <see cref="ICloneableFigure"/>, <see cref="IRenderable"/>, <see cref="IFigure"/>.
/// Предоставляет общую функциональность: свойства стиля, вершины, выделение, клонирование.
/// </summary>
public abstract class FigureViewModel: ViewModelBase, ITransformable, ISelectable, ICloneableFigure, IRenderable, IFigure
{
    private Guid _id; 
    /// <summary>Уникальный идентификатор фигуры, генерируемый при создании.</summary>
    
    private double _opacity = 1.0; 
    /// <summary>Коэффициент непрозрачности фигуры (диапазон: 0.0–1.0).</summary>
    
    public double _rotation = 0; 
    /// <summary>Угол поворота фигуры в градусах относительно центра.</summary>
    
    private string _name; 
    /// <summary>Человеко-читаемое имя фигуры для отображения в UI.</summary>

    /// <summary>
    /// Защищённый конструктор для инициализации базовых свойств фигуры.
    /// Генерирует новый GUID, устанавливает имя по имени типа и инициализирует коллекцию вершин.
    /// </summary>
    protected FigureViewModel()
    {
        _id = Guid.NewGuid();
        _name = GetType().Name.Replace("ViewModel", "");
        Vertices = new ObservableCollection<PointViewModel>();
    }
    
    /// <summary>
    /// Уникальный идентификатор фигуры (только для чтения).
    /// </summary>
    public Guid Id => _id;

    /// <summary>
    /// Имя фигуры для отображения в интерфейсе (например, в списке слоёв).
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Флаг выделения фигуры: используется для визуального отображения и операций редактирования.
    /// </summary>
    public bool IsSelected
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Цвет контура (обводки) фигуры в формате System.Drawing.Color.
    /// </summary>
    public Color LineColor
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Цвет заливки фигуры в формате System.Drawing.Color.
    /// Если альфа-канал равен 0, заливка не отображается.
    /// </summary>
    public Color FillColor
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Толщина линии обводки в пикселях.
    /// </summary>
    public double Thickness
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Непрозрачность фигуры: 0.0 — полностью прозрачная, 1.0 — полностью непрозрачная.
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }
    
    /// <summary>
    /// Угол поворота фигуры в градусах (положительный — по часовой стрелке).
    /// </summary>
    public double Rotation
    {
        get => _rotation;
        set => this.RaiseAndSetIfChanged(ref _rotation, value);
    }
    
    /// <summary>
    /// Коллекция вершин фигуры в виде наблюдаемых PointViewModel.
    /// Используется для реактивного обновления UI при изменении координат.
    /// </summary>
    public ObservableCollection<PointViewModel> Vertices { get; protected set; } 
    
    // ========== ИНТЕРФЕЙС IFigure ==========
    
    /// <summary>
    /// Получает коллекцию вершин фигуры в виде Point2D для геометрических операций.
    /// Реализация интерфейса IFigure.
    /// </summary>
    IEnumerable<Point2D> IFigure.Vertices => GetVertexPoint();
    
    // ========== ИНТЕРФЕЙС IRenderable ==========
    
    /// <summary>
    /// Получает вершины фигуры в формате, пригодном для отрисовки.
    /// По умолчанию возвращает те же вершины, что и GetVertexPoint().
    /// </summary>
    /// <returns>Перечисление точек Point2D для рендеринга.</returns>
    public IEnumerable<Point2D> GetRenderVertices() => GetVertexPoint();
    
    // ========== ИНТЕРФЕЙС ICloneableFigure ==========
    
    /// <summary>
    /// Создаёт поверхностную копию фигуры с помощью MemberwiseClone.
    /// Производные классы должны переопределить этот метод для глубокого клонирования.
    /// </summary>
    /// <returns>Новый экземпляр FigureViewModel с теми же свойствами.</returns>
    public virtual FigureViewModel Clone() => (FigureViewModel)MemberwiseClone();
    
    /// <summary>
    /// Явная реализация интерфейса ICloneableFigure.Clone().
    /// </summary>
    /// <returns>Клон фигуры как IFigure.</returns>
    IFigure ICloneableFigure.Clone() => Clone();
    
    // ========== АБСТРАКТНЫЕ ЧЛЕНЫ ==========
    
    /// <summary>
    /// Получает центральную точку фигуры для выполнения трансформаций.
    /// </summary>
    public abstract Point2D Center { get; }
    
    /// <summary>
    /// Возвращает коллекцию вершин фигуры в виде Point2D.
    /// Используется для геометрических вычислений и отрисовки.
    /// </summary>
    /// <returns>Перечисление точек вершин.</returns>
    public abstract IEnumerable<Point2D> GetVertexPoint();

    /// <summary>
    /// Поворачивает фигуру на заданный угол вокруг её центра.
    /// </summary>
    /// <param name="angle">Угол поворота в градусах (положительный — по часовой стрелке).</param>
    public abstract void Rotate(double angle);
    
    /// <summary>
    /// Масштабирует фигуру относительно центра с заданными коэффициентами по осям.
    /// </summary>
    /// <param name="sx">Коэффициент масштабирования по оси X.</param>
    /// <param name="sy">Коэффициент масштабирования по оси Y.</param>
    public abstract void Scale(double sx, double sy);
    
    /// <summary>
    /// Перемещает фигуру на заданный вектор смещения.
    /// </summary>
    /// <param name="dx">Смещение по оси X.</param>
    /// <param name="dy">Смещение по оси Y.</param>
    public abstract void Move(double dx, double dy);
    
    /// <summary>
    /// Проверяет, находится ли заданная точка внутри фигуры или на её контуре.
    /// </summary>
    /// <param name="point">Проверяемая точка в координатах канваса.</param>
    /// <param name="eps">Допуск для пограничных случаев (по умолчанию 0.001).</param>
    /// <returns>True, если точка принадлежит фигуре; иначе false.</returns>
    public abstract bool IsIn(Point2D point, double eps = 0.001);

    // ========== ВИРТУАЛЬНЫЕ МЕТОДЫ ==========
    
    /// <summary>
    /// Выполняет радиальное (равномерное) масштабирование фигуры.
    /// </summary>
    /// <param name="scale">Коэффициент масштабирования (применяется к обеим осям).</param>
    public virtual void RadialScale(double scale) => Scale(scale, scale);

    /// <summary>
    /// Выполняет отражение фигуры относительно прямой, заданной двумя точками.
    /// Базовая реализация отражает все вершины фигуры.
    /// </summary>
    /// <param name="a">Первая точка, определяющая ось отражения.</param>
    /// <param name="b">Вторая точка, определяющая ось отражения.</param>
    public virtual void Reflection(Point2D a, Point2D b)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            var p = vertex.ToPoint();
            var reflected = ReflectPoint(p, a, b);
            vertex.X = reflected.X;
            vertex.Y = reflected.Y;
        }
    }
    
    /// <summary>
    /// Проверяет пересечение фигуры с заданной прямоугольной областью.
    /// Используется для выделения областью (marquee selection).
    /// </summary>
    /// <param name="leftTop">Левый верхний угол выделяющей области.</param>
    /// <param name="rightBottom">Правый нижний угол выделяющей области.</param>
    /// <returns>True, если фигура пересекается с областью; иначе false.</returns>
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

    /// <summary>
    /// Вычисляет ограничивающий прямоугольник (bounding box) фигуры.
    /// </summary>
    /// <returns>
    /// Кортеж (MinX, MaxX, MinY, MaxY) с координатами границ фигуры.
    /// </returns>
    public virtual (double MinX, double MaxX, double MinY, double MaxY) GetBoundingBox()
    {
        var vertices = Vertices.Select(v => v.ToPoint()).ToList();
        return (
            vertices.Min(p => p.X),
            vertices.Max(p => p.X),
            vertices.Min(p => p.Y),
            vertices.Max(p => p.Y)
        );
    }

    /// <summary>
    /// Принудительно уведомляет подписчиков об изменении свойств фигуры.
    /// Используется при массовом обновлении вершин для корректного обновления UI.
    /// </summary>
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(Vertices));
        this.RaisePropertyChanged(nameof(Center));
    }

    /// <summary>
    /// Вспомогательный метод для отражения точки относительно прямой, заданной двумя точками.
    /// </summary>
    /// <param name="p">Отражаемая точка.</param>
    /// <param name="a">Первая точка прямой.</param>
    /// <param name="b">Вторая точка прямой.</param>
    /// <returns>Отражённая точка в координатах Point2D.</returns>
    protected Point2D ReflectPoint(Point2D p, Point2D a, Point2D b) => p.Reflect(a, b);
}