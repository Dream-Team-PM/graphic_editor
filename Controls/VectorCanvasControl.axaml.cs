// Controls/VectorCanvasControl.axaml.cs

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Windows.Input;
using DynamicData.Experimental;

using graphic_editor.Geometry;
using graphic_editor.Helpers;
using graphic_editor.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace graphic_editor.Controls;

/// <summary>
/// Основной пользовательский контрол для отрисовки векторных фигур на канвасе.
/// Отвечает за рендеринг фигур, обработку выделения, масштабирование и привязку к ViewModel.
/// </summary>
public partial class VectorCanvasControl : UserControl
{
    /// <summary>Словарь для отслеживания отрисованных элементов управления по ID фигур.</summary>
    private readonly Dictionary<Guid, Control> _renderedFigures = new();
    
    /// <summary>Ссылка на текущий активный слой для отрисовки.</summary>
    private LayerViewModel? _currentLayer;
    
    /// <summary>Зависимое свойство для привязки ViewModel канваса.</summary>
    public static readonly StyledProperty<CanvasViewModel?> CanvasViewModelProperty =
        AvaloniaProperty.Register<VectorCanvasControl, CanvasViewModel?>(nameof(CanvasViewModel));
 //
	// public static readonly StyledProperty<ICommand?> PointerPressedCommandProperty =
 //        AvaloniaProperty.Register<VectorCanvasControl, ICommand?>(nameof(PointerPressedCommand));
 //
 //    public static readonly StyledProperty<ICommand?> PointerMovedCommandProperty =
 //        AvaloniaProperty.Register<VectorCanvasControl, ICommand?>(nameof(PointerMovedCommand));

    // public static readonly StyledProperty<ICommand?> PointerReleasedCommandProperty =
    //     AvaloniaProperty.Register<VectorCanvasControl, ICommand?>(nameof(PointerReleasedCommand));
    
    // public static readonly StyledProperty<ObservableCollection<FigureViewModel>?> ActiveFiguresProperty = 
    //     AvaloniaProperty.Register<VectorCanvasControl, ObservableCollection<FigureViewModel>?>(nameof(ActiveFigures));

    /// <summary>Зависимое свойство для коэффициента масштабирования канваса.</summary>
    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(Zoom), 1.0);

    /// <summary>Зависимое свойство для смещения канваса по оси X.</summary>
    public static readonly StyledProperty<double> OffsetXProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(OffsetX));

    /// <summary>Зависимое свойство для смещения канваса по оси Y.</summary>
    public static readonly StyledProperty<double> OffsetYProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(OffsetY));
    
	/// <summary>
    /// Инициализирует новый экземпляр класса <see cref="VectorCanvasControl"/>.
    /// Загружает XAML-компоненты контрола.
    /// </summary>
    public VectorCanvasControl()
    {
        InitializeComponent();
		//this.AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
        //this.AddHandler(PointerMovedEvent, OnPointerMoved, handledEventsToo: true);
        //this.AddHandler(PointerReleasedEvent, OnPointerReleased, handledEventsToo: true);
    }
    
    /// <summary>
    /// Применяет визуальный стиль (цвет обводки, заливки, толщину, прозрачность) к элементу Shape.
    /// </summary>
    /// <param name="shape">Элемент управления Shape для применения стиля.</param>
    /// <param name="figure">Модель фигуры, содержащая параметры стиля.</param>
    private void ApplyStyle(Shape shape, FigureViewModel figure)
    {
        var strokeColor = ToAvaloniaColor(figure.LineColor);
        shape.Stroke = new SolidColorBrush(strokeColor);
        if (shape is Avalonia.Controls.Shapes.Line)
        {
            shape.StrokeThickness = Math.Max(1, figure.Thickness);
        }
        else
        {
            shape.StrokeThickness = 2;
            shape.Fill = figure.FillColor.A > 0 
                ? new SolidColorBrush(ToAvaloniaColor(figure.FillColor)) 
                : Brushes.Transparent;
        }
        shape.Opacity = Math.Clamp(figure.Opacity, 0.1, 1.0);
    }

	// public ICommand? PointerPressedCommand
 //    {
 //        get => GetValue(PointerPressedCommandProperty);
 //        set => SetValue(PointerPressedCommandProperty, value);
 //    }
 //
 //    public ICommand? PointerMovedCommand
 //    {
 //        get => GetValue(PointerMovedCommandProperty);
 //        set => SetValue(PointerMovedCommandProperty, value);
 //    }
 //
 //    public ICommand? PointerReleasedCommand
 //    {
 //        get => GetValue(PointerReleasedCommandProperty);
 //        set => SetValue(PointerReleasedCommandProperty, value);
 //    }

	/// <summary>
    /// Зависимое свойство для доступа к ViewModel канваса.
    /// </summary>
    public CanvasViewModel? CanvasViewModel
    {
        get => GetValue(CanvasViewModelProperty);
        set => SetValue(CanvasViewModelProperty, value);
    }

	/// <summary>
    /// Зависимое свойство для коэффициента масштабирования.
    /// </summary>
    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

	 /// <summary>
    /// Зависимое свойство для смещения по оси X.
    /// </summary>
    public double OffsetX
    {
        get => GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

	/// <summary>
    /// Зависимое свойство для смещения по оси Y.
    /// </summary>
    public double OffsetY
    {
        get => GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    /// <summary>
    /// Отображает или скрывает предварительную фигуру на канвасе.
    /// Используется для визуализации фигуры в процессе рисования.
    /// </summary>
    /// <param name="figure">Фигура для предварительного отображения (или null для скрытия).</param>
	public void ShowPreviewFigure(FigureViewModel? figure)
    {
        // Удаляем старую предварительную фигуру
        var existingPreview = _renderedFigures.Values.FirstOrDefault(c => c.Tag is FigureViewModel f && f.Name == "Preview");
        if (existingPreview != null)
        {
            DrawingCanvas.Children.Remove(existingPreview);
            _renderedFigures.Remove(_renderedFigures.First(kvp => kvp.Value == existingPreview).Key);
        }
        // Добавляем новую предварительную фигуру
        if (figure != null)
        {
            figure.Name = "Preview";
            var control = CreateControlForFigure(figure);
            if (control != null)
            {
                BindFigureProperties(figure, control);
                control.Opacity = 0.5;
                control.IsHitTestVisible = false;
                
                DrawingCanvas.Children.Add(control);
                _renderedFigures[figure.Id] = control;
                control.Tag = figure;
            }
        }
    }
    
	/// <summary>
    /// Вызывается при присоединении контрола к визуальному дереву.
    /// Подписывается на события ViewModel и выполняет первоначальную отрисовку фигур.
    /// </summary>
    /// <param name="e">Аргументы события присоединения.</param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToCanvasViewModel();
        RenderAllFigures();
    }

	/// <summary>
    /// Вызывается при отсоединении контрола от визуального дерева.
    /// Отписывается от событий фигур для предотвращения утечек памяти.
    /// </summary>
    /// <param name="e">Аргументы события отсоединения.</param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromFigures();
        base.OnDetachedFromVisualTree(e);
    }

	/// <summary>
    /// Подписывается на события изменения свойств CanvasViewModel.
    /// </summary>
    private void SubscribeToCanvasViewModel()
    {
        if (CanvasViewModel != null)
        {
            CanvasViewModel.PropertyChanged += OnCanvasViewModelPropertyChanged;
            if (CanvasViewModel.SelectedFigures is INotifyCollectionChanged selectedFigures)
            {
                selectedFigures.CollectionChanged += (s, e) =>
                {
                    Dispatcher.UIThread.Post(UpdateSelectionVisuals);
                    DebugLog.Write(
                        $"[DEBUG] SelectedFigures.CollectionChanged: Action={e.Action}, NewItems={e.NewItems?.Count ?? 0}");
                };
                DebugLog.Write("[DEBUG] Entered to SubscribeToCanvasViewModel");
            }
            SubscribeToCurrentLayer();
        }
    }
    
	/// <summary>
    /// Отписывается от событий изменения свойств CanvasViewModel.
    /// </summary>
    private void UnsubscribeFromCanvasViewModel()
    {
        if (CanvasViewModel != null)
        {
            CanvasViewModel.PropertyChanged -= OnCanvasViewModelPropertyChanged;
        }
    }
    
	/// <summary>
    /// Подписывается на события коллекции фигур текущего активного слоя.
    /// </summary>
    private void SubscribeToCurrentLayer()
    {
        DebugLog.Write($"[DEBUG] SubscribeToCurrentLayer: ActiveLayer={CanvasViewModel?.ActiveLayer?.Name ?? "null"}");
        UnsubscribeFromFigures();
        _currentLayer = CanvasViewModel?.ActiveLayer;
    
        if (CanvasViewModel?.ActiveLayer != null)
        {
            DebugLog.Write($"[DEBUG] Subscribing to Figures.CollectionChanged");
            CanvasViewModel.ActiveLayer.Figures.CollectionChanged += OnFiguresChanged;
            RenderAllFigures();
        }
        else
        {
            DebugLog.Write($"[DEBUG] SKIP subscribe: ActiveLayer is null");
        }
    }

	/// <summary>
    /// Отписывается от событий коллекции фигур текущего слоя.
    /// </summary>
    private void UnsubscribeFromFigures()
    {
        if (_currentLayer != null)
        {
            _currentLayer.Figures.CollectionChanged -= OnFiguresChanged;
            _currentLayer = null;
        }
    }

	/// <summary>
    /// Обрабатывает изменение зависимых свойств контрола.
    /// Обновляет подписки при смене ViewModel и применяет трансформации при изменении зума/смещения.
    /// </summary>
    /// <param name="change">Аргументы изменения свойства.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanvasViewModelProperty)
        {
            var oldVm = change.GetOldValue<CanvasViewModel>();
            var newVm = change.GetNewValue<CanvasViewModel>();
            DebugLog.Write($"[DEBUG] CanvasVM binding changed: Old={oldVm?.GetHashCode()}, New={newVm?.GetHashCode()}");
            UnsubscribeFromCanvasViewModel();
            SubscribeToCanvasViewModel();
            RenderAllFigures();
        }
        else if (change.Property == ZoomProperty || 
                 change.Property == OffsetXProperty || 
                 change.Property == OffsetYProperty)
        {
            UpdateTransform();
        }
    }
    
	/// <summary>
    /// Обработчик изменения свойств CanvasViewModel.
    /// Реагирует на смену активного слоя, предварительной фигуры или выделения.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения свойства.</param>
    private void OnCanvasViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DebugLog.Write($"[DEBUG] CanvasVM PropertyChanged: {e.PropertyName}");
        if (e.PropertyName == nameof(CanvasViewModel.ActiveLayer))
        {
            // Активный слой изменился — подписываемся на его фигуры
            DebugLog.Write($"[DEBUG] ActiveLayer changed, new value: {CanvasViewModel?.ActiveLayer?.Name ?? "null"}");
            SubscribeToCurrentLayer();
        }
		else if (e.PropertyName == nameof(CanvasViewModel.PreviewFigure))
    	{
        	// Обновляем предварительный просмотр
        	Dispatcher.UIThread.Post(() => 
            	ShowPreviewFigure(CanvasViewModel?.PreviewFigure)
        	);
    	} 
        else if (e.PropertyName == nameof(CanvasViewModel.SelectedFigure))
        {
            UpdateSelectionVisuals();
        }
    }

	/// <summary>
    /// Обработчик изменения коллекции фигур слоя.
    /// Добавляет, удаляет или обновляет отображение фигур в зависимости от типа изменения.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы изменения коллекции.</param>
    private void OnFiguresChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DebugLog.Write($"[DEBUG] OnFiguresChanged: Action={e.Action}, NewItems={e.NewItems?.Count ?? 0}");
        try {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        DebugLog.Write($"[DEBUG] OnFiguresChanged.Add: Iterating {e.NewItems.Count} items");
                        foreach (var item in e.NewItems)
                        {
                            DebugLog.Write($"[DEBUG] Processing NewItem: Type={item?.GetType().Name}, Item={item}");
                            if (item is FigureViewModel figure)
                            {
                                DebugLog.Write($"[DEBUG] Rendering new figure: {figure.Name}");
                                RenderFigure(figure);
                            }
                            else
                            {
                                DebugLog.Write($"[ERROR] Item is not FigureViewModel: {item?.GetType()}");
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (FigureViewModel figure in e.OldItems)
                        {
                            RemoveFigure(figure);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearAllFigures();
                    RenderAllFigures();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (FigureViewModel figure in e.OldItems)
                            RemoveFigure(figure);
                    }
                    if (e.NewItems != null)
                    {
                        foreach (FigureViewModel figure in e.NewItems)
                            RenderFigure(figure);
                    }
                    break;
            }
        } 
        catch (Exception ex)
        {
            DebugLog.Write($"[ERROR] OnFiguresChanged exception: {ex.Message}\n{ex.StackTrace}");
        }
    }

	/// <summary>
    /// Выполняет полную перерисовку всех фигур текущего слоя.
    /// </summary>
    private void RenderAllFigures()
    {
        ClearAllFigures();
        DebugLog.Write($"[DEBUG] RenderAllFigures: DrawingCanvas={DrawingCanvas != null}, Figures={_currentLayer?.Figures.Count ?? 0}");
        var figures = _currentLayer?.Figures;
        if (figures != null)
        {
            foreach (var figure in figures)
            {
                RenderFigure(figure);
            }
        }
        UpdateTransform();
    }

	/// <summary>
    /// Отрисовывает отдельную фигуру на канвасе.
    /// </summary>
    /// <param name="figure">Модель фигуры для отрисовки.</param>
    private void RenderFigure(FigureViewModel figure)
    {
        DebugLog.Write($"[DEBUG] RenderFigure: {figure.Name}, DrawingCanvas={DrawingCanvas != null}");
        DebugLog.Write($"[DEBUG] RenderFigure in Control, CanvasVM Hash={CanvasViewModel?.GetHashCode()}");
        if (_renderedFigures.ContainsKey(figure.Id))
            return;
        var control = CreateControlForFigure(figure);
        if (control != null)
        {
            BindFigureProperties(figure, control);
            if (DrawingCanvas != null)
            {
                DrawingCanvas.Children.Add(control);
                _renderedFigures[figure.Id] = control;
                control.Tag = figure;
                control.PointerPressed += OnFigurePointerPressed;
                DebugLog.Write($"[DEBUG] Figure added to canvas");
            }
            else
            {
                DebugLog.Write("[ERROR] DrawingCanvas is null!");
            }
        }
    }

	/// <summary>
    /// Создаёт элемент управления Avalonia для отображения заданной фигуры.
    /// </summary>
    /// <param name="figure">Модель фигуры.</param>
    /// <returns>Элемент управления для отрисовки или null, если тип фигуры не поддерживается.</returns>
    private Control? CreateControlForFigure(FigureViewModel figure)
    {
        return figure switch
        {
            GroupViewModel group => CreateGroup(group),
            PolygonViewModel polygon => CreatePolygon(polygon), // Включает в себя треугольник и многоугольники
            SquareViewModel square => CreateSquare(square),
            CircleViewModel circle => CreateCircle(circle),
            RectangleViewModel rect => CreateRectangle(rect),
            EllipseViewModel ellipse => CreateEllipse(ellipse),
            PenPointViewModel pen => CreatePenPoint(pen), 
            LineViewModel lin => CreateLine(lin),
            // BezieCurveViewModel bezie => CreateBezieCurve(bezie),
            // CurveViewModel curve => CreateCurve(curve),
            // SplineViewModel spline => CreateSpline(spline),
            // RhombusViewModel rhombus => CreateRhombus(rhombus),
            // RightTriangleViewModel right_triangle => CreateRightTriangle(right_triangle),
            _ => null
        };
    }
    
	/// <summary>
    /// Создаёт элемент Path для отрисовки произвольного многоугольника.
    /// </summary>
    /// <param name="polygon">Модель многоугольника.</param>
    /// <returns>Элемент Path с геометрией многоугольника.</returns>
    private Avalonia.Controls.Shapes.Path CreatePolygon(PolygonViewModel polygon)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            if (polygon.Vertices.Count == 0) return null;
        
            ctx.BeginFigure(
                new Avalonia.Point(polygon.Vertices[0].X, polygon.Vertices[0].Y),
                isFilled: polygon.FillColor.A > 0
            );
        
            for (int i = 1; i < polygon.Vertices.Count; i++)
                ctx.LineTo(new Avalonia.Point(polygon.Vertices[i].X, polygon.Vertices[i].Y));
        
            ctx.EndFigure(isClosed: true);
        }
    
        return new Avalonia.Controls.Shapes.Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(ToAvaloniaColor(polygon.LineColor)),
            StrokeThickness = Math.Max(1, polygon.Thickness),
            Fill = polygon.FillColor.A > 0 
                ? new SolidColorBrush(ToAvaloniaColor(polygon.FillColor)) 
                : null,
            Tag = polygon
        };
    }
    
	/// <summary>
    /// Создаёт панель-контейнер для отображения группы фигур.
    /// </summary>
    /// <param name="group">Модель группы фигур.</param>
    /// <returns>Панель с дочерними элементами управления.</returns>
    private Panel CreateGroup(GroupViewModel group)
    {
        var panel = new Panel(); // Контейнер для детей
        foreach (var child in group.Children)
        {
            var childControl = CreateControlForFigure(child);
            if (childControl != null)
            {
                BindFigureProperties(child, childControl);
                panel.Children.Add(childControl);
            }
        }
        panel.Tag = group;
        DebugLog.Write("[DEBUG] Entered to CreateGroup");
        return panel;
    }
    
	/// <summary>
    /// Создаёт элемент Line для отрисовки линии.
    /// </summary>
    /// <param name="line">Модель линии.</param>
    /// <returns>Элемент Line с заданными координатами.</returns>
    private Avalonia.Controls.Shapes.Line CreateLine(LineViewModel line) => new()
    {
        StartPoint = new Avalonia.Point(line.X1, line.Y1),
        EndPoint = new Avalonia.Point(line.X2, line.Y2),
        Tag = line
    };

	/// <summary>
    /// Создаёт элемент Path для отрисовки фигуры по вершинам (прямоугольник или эллипс).
    /// </summary>
    /// <param name="figure">Модель фигуры.</param>
    /// <param name="isEllipse">Флаг, указывающий, что фигура является эллипсом.</param>
    /// <returns>Элемент Path с соответствующей геометрией.</returns>
	private Avalonia.Controls.Shapes.Path CreateShapeFromVertices(FigureViewModel figure, bool isEllipse = false)
	{
    	var geometry = new StreamGeometry();
    	using (var ctx = geometry.Open())
    	{
        	if (figure.Vertices.Count < 4) return null;
        
        	// Для эллипса используем Arc, для прямоугольника - LineTo
        	if (isEllipse)
        	{
            	var center = figure.Center;
            	var rx = Math.Abs(figure.Vertices[2].X - figure.Vertices[0].X) / 2;
            	var ry = Math.Abs(figure.Vertices[2].Y - figure.Vertices[0].Y) / 2;
            
            	ctx.BeginFigure(new Avalonia.Point(center.X - rx, center.Y), isFilled: true);
            	// Верхняя дуга
            	ctx.ArcTo(new Avalonia.Point(center.X + rx, center.Y), 
                     new Avalonia.Size(rx, ry), 0, false, SweepDirection.Clockwise);
            	// Нижняя дуга
            	ctx.ArcTo(new Avalonia.Point(center.X - rx, center.Y), 
                     new Avalonia.Size(rx, ry), 0, false, SweepDirection.Clockwise);
            	ctx.EndFigure(isClosed: true);
        	}
        	else
        	{
            	// Прямоугольник через 4 вершины
            	ctx.BeginFigure(new Avalonia.Point(figure.Vertices[0].X, figure.Vertices[0].Y), isFilled: true);
            	for (int i = 1; i < 4; i++)
                	ctx.LineTo(new Avalonia.Point(figure.Vertices[i].X, figure.Vertices[i].Y));
            	ctx.EndFigure(isClosed: true);
        	}
    	}
    
    	return new Avalonia.Controls.Shapes.Path
    	{
        	Data = geometry,
        	Tag = figure,
        	[Canvas.LeftProperty] = 0,
        	[Canvas.TopProperty] = 0
    	};
	}
    
	/// <summary>
    /// Создаёт элемент Rectangle для отрисовки прямоугольника.
    /// </summary>
    /// <param name="r">Модель прямоугольника.</param>
    /// <returns>Элемент Rectangle с заданными размерами и позицией.</returns>
    private Avalonia.Controls.Shapes.Rectangle CreateRectangle(RectangleViewModel r) => new()
    {
        Width = Math.Abs(r.Width),
        Height = Math.Abs(r.Height),
        [Canvas.LeftProperty] = Math.Min(r.X, r.X + r.Width),
        [Canvas.TopProperty] = Math.Min(r.Y, r.Y + r.Height),
        Tag = r
    };

	/// <summary>
    /// Создаёт элемент Ellipse для отрисовки эллипса.
    /// </summary>
    /// <param name="e">Модель эллипса.</param>
    /// <returns>Элемент Ellipse с заданными размерами и позицией.</returns>
    private Avalonia.Controls.Shapes.Ellipse CreateEllipse(EllipseViewModel e) => new()
    {
        Width = Math.Abs(e.Width),
        Height = Math.Abs(e.Height),
        [Canvas.LeftProperty] = Math.Min(e.X, e.X + e.Width),
        [Canvas.TopProperty] = Math.Min(e.Y, e.Y + e.Height),
        Tag = e
    };
    
	/// <summary>
    /// Создаёт элемент Rectangle для отрисовки квадрата.
    /// </summary>
    /// <param name="square">Модель квадрата.</param>
    /// <returns>Элемент Rectangle с равными шириной и высотой.</returns>
    private Avalonia.Controls.Shapes.Rectangle CreateSquare(SquareViewModel square) => new()
    {
        Width = Math.Abs(square.Side),
        Height = Math.Abs(square.Side),
        [Canvas.LeftProperty] = Math.Min(square.X, square.X + square.Side),
        [Canvas.TopProperty] = Math.Min(square.Y, square.Y + square.Side),
        Tag = square
    };
    
	/// <summary>
    /// Создаёт элемент Ellipse для отрисовки круга.
    /// </summary>
    /// <param name="circle">Модель круга.</param>
    /// <returns>Элемент Ellipse с диаметром, равным удвоенному радиусу.</returns>
    private Avalonia.Controls.Shapes.Ellipse CreateCircle(CircleViewModel circle) => new()
    {
        Width = Math.Abs(circle.Radius * 2),
        Height = Math.Abs(circle.Radius * 2),
        [Canvas.LeftProperty] = circle.X - circle.Radius,
        [Canvas.TopProperty] = circle.Y - circle.Radius,
        Tag = circle
    };

	/// <summary>
    /// Создаёт элемент Ellipse для отрисовки точки пера.
    /// </summary>
    /// <param name="pen">Модель точки пера.</param>
    /// <returns>Элемент Ellipse с радиусом, зависящим от толщины.</returns>
    private Avalonia.Controls.Shapes.Ellipse CreatePenPoint(PenPointViewModel pen)
    {
        var radius = Math.Max(2, pen.Thickness / 2 + 2);
        return new Avalonia.Controls.Shapes.Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            [Canvas.LeftProperty] = pen.X - radius,
            [Canvas.TopProperty] = pen.Y - radius,
            Tag = pen
        };
    }

    /// <summary>
    /// Привязывает свойства модели фигуры к элементу управления для реактивного обновления UI.
    /// </summary>
    /// <param name="figure">Модель фигуры.</param>
    /// <param name="control">Элемент управления для привязки.</param>
    private void BindFigureProperties(FigureViewModel figure, Control control)
    {
        // Конвертация цвета
        if (control is not Shape shape) return;
        ApplyStyle(shape, figure);
        
        if (figure is GroupViewModel group && control is Panel panel)
        {
            foreach (var child in group.Children)
            {
                child.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName is nameof(PointViewModel.X) or nameof(PointViewModel.Y))
                    {
                        Dispatcher.UIThread.Post(() => 
                            UpdateSelectionVisual(group, panel)
                        );
                        DebugLog.Write("[DEBUG] Entered to group BindFigureProperties");
                    }
                };
            }
        }
        if (figure is PolygonViewModel polygon && control is Avalonia.Controls.Shapes.Path path)
        {
            polygon.VerticesChanged += (s, e) =>
            {
                Dispatcher.UIThread.Post(() => 
                    UpdatePolygonGeometry(path, polygon)
                );
            };
        }
        foreach (var vertex in figure.Vertices)
        {
            vertex.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PointViewModel.X) || 
                    e.PropertyName == nameof(PointViewModel.Y))
                {
                    if (control is Avalonia.Controls.Shapes.Path path)
                    {
                        Dispatcher.UIThread.Post(() => 
                            UpdatePolygonGeometry(path, figure as PolygonViewModel)
                        );
                    }
                    else
                    {
                        UpdateShapeGeometry(control as Shape, figure);
                    }
                }
            };
        }
        figure.PropertyChanged += (s, e) =>
        {
            if (control is not Shape shapeCtrl) return;

            switch (e.PropertyName)
            {
                case nameof(FigureViewModel.LineColor):
                    shapeCtrl.Stroke = new SolidColorBrush(
                        ToAvaloniaColor(figure.LineColor));
                    break;
                case nameof(FigureViewModel.FillColor):
                    if (shapeCtrl is not Avalonia.Controls.Shapes.Line)
                    {
                        shapeCtrl.Fill = figure.FillColor.A > 0 
                            ? new SolidColorBrush(ToAvaloniaColor(figure.FillColor)) 
                            : Brushes.Transparent;
                    }
                    break;
                case nameof(FigureViewModel.Thickness):
                    if (shapeCtrl is Avalonia.Controls.Shapes.Line)
                    {
                        shapeCtrl.StrokeThickness = Math.Max(1, figure.Thickness);
                    }
                    else
                    {
                        shapeCtrl.StrokeThickness = 2;
                    }
                    break;
                case nameof(FigureViewModel.Opacity):
                    shapeCtrl.Opacity = Math.Clamp(figure.Opacity, 0.1, 1.0);
                    break;
                case nameof(FigureViewModel.IsSelected):
                    UpdateSelectionVisual(figure, control);
                    break;
                case nameof(LineViewModel.X1):
                case nameof(LineViewModel.Y1):
                case nameof(LineViewModel.X2):
                case nameof(LineViewModel.Y2):
                    if (shapeCtrl is Avalonia.Controls.Shapes.Line line && 
                        figure is LineViewModel lineVm)
                        {
                            line.StartPoint = new Avalonia.Point(lineVm.X1, lineVm.Y1);
                            line.EndPoint = new Avalonia.Point(lineVm.X2, lineVm.Y2);
                        }
                    break;
                case "X":
                case "Y":
                case "Width":
                case "Height":
                case "Side":
                case "Radius":
                    if (figure is PolygonViewModel poly && shapeCtrl is Avalonia.Controls.Shapes.Path p)
                    {
                        UpdatePolygonGeometry(p, poly);
                    }
                    else
                    {
                        UpdateShapeGeometry(shapeCtrl, figure);
                    }
                    break;
            }
        };
    }
    
	/// <summary>
    /// Обновляет геометрию элемента Shape при изменении свойств фигуры.
    /// </summary>
    /// <param name="shape">Элемент управления Shape.</param>
    /// <param name="figure">Модель фигуры с обновлёнными данными.</param>
    private void UpdateShapeGeometry(Shape shape, FigureViewModel figure)
    {
        switch (figure)
        {
            case GroupViewModel:
                break;
            
            case LineViewModel lineVm when shape is Avalonia.Controls.Shapes.Line line:
                line.StartPoint = new Avalonia.Point(lineVm.X1, lineVm.Y1);
                line.EndPoint = new Avalonia.Point(lineVm.X2, lineVm.Y2);
                break;
            
            // Многоугольники (Path)
            case PolygonViewModel polygon when shape is Avalonia.Controls.Shapes.Path path:
                UpdatePolygonGeometry(path, polygon);
                break;
            
            case SquareViewModel square when shape is Avalonia.Controls.Shapes.Rectangle sq:
                sq.Width = sq.Height = Math.Abs(square.Side);
                Canvas.SetLeft(sq, Math.Min(square.X, square.X + square.Side));
                Canvas.SetTop(sq, Math.Min(square.Y, square.Y + square.Side));
                break;
        
            case CircleViewModel circle when shape is Avalonia.Controls.Shapes.Ellipse cir:
                var d = Math.Abs(circle.Radius * 2);
                cir.Width = cir.Height = d;
                Canvas.SetLeft(cir, circle.X - circle.Radius);
                Canvas.SetTop(cir, circle.Y - circle.Radius);
                break;
            
            case RectangleViewModel rect:
                if (shape is Avalonia.Controls.Shapes.Rectangle r)
                {
                    r.Width = Math.Abs(rect.Width);
                    r.Height = Math.Abs(rect.Height);
                    Canvas.SetLeft(r, Math.Min(rect.X, rect.X + rect.Width));
                    Canvas.SetTop(r, Math.Min(rect.Y, rect.Y + rect.Height));
                }
                break;
            
            case EllipseViewModel ellipse:
                if (shape is Avalonia.Controls.Shapes.Ellipse e)
                {
                    e.Width = Math.Abs(ellipse.Width);
                    e.Height = Math.Abs(ellipse.Height);
                    Canvas.SetLeft(e, Math.Min(ellipse.X, ellipse.X + ellipse.Width));
                    Canvas.SetTop(e, Math.Min(ellipse.Y, ellipse.Y + ellipse.Height));
                }
                break;
        }
    }
    
	/// <summary>
    /// Обновляет геометрию Path при изменении вершин многоугольника.
    /// </summary>
    /// <param name="path">Элемент Path для обновления.</param>
    /// <param name="polygon">Модель многоугольника с новыми вершинами.</param>
    private void UpdatePolygonGeometry(Avalonia.Controls.Shapes.Path path, PolygonViewModel polygon)
    {
        DebugLog.Write($"[DEBUG] UpdatePolygonGeometry: {polygon.Name}, Vertices={polygon.Vertices.Count}");
        if (polygon.Vertices.Count < 3) return;
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            if (polygon.Vertices.Count == 0) return;
            ctx.BeginFigure(
                new Avalonia.Point(polygon.Vertices[0].X, polygon.Vertices[0].Y),
                isFilled: polygon.FillColor.A > 0
            );
            for (int i = 1; i < polygon.Vertices.Count; i++)
                ctx.LineTo(new Avalonia.Point(polygon.Vertices[i].X, polygon.Vertices[i].Y));
        
            ctx.EndFigure(isClosed: true);
        }
        path.Data = geometry;
    }
    
    /// <summary>
    /// Обновляет визуальное выделение фигуры (добавляет или удаляет рамку).
    /// </summary>
    /// <param name="figure">Модель фигуры для обновления выделения.</param>
    /// <param name="control">Элемент управления, отображающий фигуру.</param>
    private void UpdateSelectionVisual(FigureViewModel figure, Control control)
    {
        DebugLog.Write($"[DEBUG] UpdateSelectionVisual: {figure.Name}, IsSelected={figure.IsSelected}, Control={control?.GetType().Name}, Parent={control?.Parent?.GetType().Name}");
        if (control.Parent is not Panel parent)
    	{
        	DebugLog.Write($"[WARN] UpdateSelectionVisual: control.Parent is null or not Panel, skipping");
        	return;
    	}
		var adorner = control.Parent is Panel groupPanel ? groupPanel.Children.OfType<Border>().FirstOrDefault(b => b.Tag as string == "SelectionAdorner") : null;
    
        if (figure is GroupViewModel group && control is Panel panel)
        {
            var groupAdorner = panel.Children.OfType<Border>()
            .FirstOrDefault(b => b.Tag as string == "GroupSelectionAdorner");
        
            if (figure.IsSelected)
            {
                if (groupAdorner == null)
                {
                    var border = new Border
                    {
                        BorderBrush = Brushes.Cyan,
                        BorderThickness = new Thickness(1),
                        IsHitTestVisible = false,
                        Tag = "GroupSelectionAdorner"
                    };
                
                    var bbox = group.GetBoundingBox();
                    border.Width = bbox.MaxX - bbox.MinX;
                    border.Height = bbox.MaxY - bbox.MinY;
                    Canvas.SetLeft(border, bbox.MinX);
                    Canvas.SetTop(border, bbox.MinY);
                	parent.Children.Add(border);
                }
            }
            else
            {
                if (groupAdorner?.Parent is Panel groupAdornerParent)
                    groupAdornerParent.Children.Remove(groupAdorner);
            }
            return;
        }
    
        // Для обычных фигур (Rectangle, Ellipse, Path, Line)
        if (figure.IsSelected)
        {
            if (adorner == null && control is Shape shape)
            {
                var border = new Border
                {
                    BorderBrush = Brushes.Blue,
                    BorderThickness = new Thickness(1),
                    IsHitTestVisible = false,
                    Tag = "SelectionAdorner"
                };
                if (shape is Avalonia.Controls.Shapes.Rectangle rect)
                {
                    border.Width = rect.Width;
                    border.Height = rect.Height;
                    Canvas.SetLeft(border, Canvas.GetLeft(rect));
                    Canvas.SetTop(border, Canvas.GetTop(rect));
                }
                else if (shape is Avalonia.Controls.Shapes.Ellipse ellipse)
                {
                    border.Width = ellipse.Width;
                    border.Height = ellipse.Height;
                    Canvas.SetLeft(border, Canvas.GetLeft(ellipse));
                    Canvas.SetTop(border, Canvas.GetTop(ellipse));
                }
                else if (shape is Avalonia.Controls.Shapes.Path path)
                {
                    var bbox = figure.GetBoundingBox();
                    border.Width = bbox.MaxX - bbox.MinX;
                    border.Height = bbox.MaxY - bbox.MinY;
                    Canvas.SetLeft(border, bbox.MinX);
                    Canvas.SetTop(border, bbox.MinY);
                }
                else if (shape is Avalonia.Controls.Shapes.Line line)
                {
                    border.Width = Math.Abs(line.EndPoint.X - line.StartPoint.X);
                    border.Height = Math.Abs(line.EndPoint.Y - line.StartPoint.Y);
                    Canvas.SetLeft(border, Math.Min(line.StartPoint.X, line.EndPoint.X));
                    Canvas.SetTop(border, Math.Min(line.StartPoint.Y, line.EndPoint.Y));
                }
            
                if (shape.Parent is Panel shapeParent)
                {
                    DebugLog.Write($"[DEBUG] Adding border to parent: Width={border.Width}, Height={border.Height}, Left={Canvas.GetLeft(border)}, Top={Canvas.GetTop(border)}");
                    shapeParent.Children.Add(border);
                }
                else
                {
                    DebugLog.Write("[ERROR] shape.Parent is not Panel!");
                }
            }
            control.Opacity = 1.0;
        }
        else
        {
            if (adorner?.Parent is Panel adornerParent)
                adornerParent.Children.Remove(adorner);
        }
    }

    /// <summary>
    /// Обновляет визуальное выделение для всех отрисованных фигур.
    /// Синхронизирует состояние IsSelected моделей с коллекцией SelectedFigures ViewModel.
    /// </summary>
    private void UpdateSelectionVisuals()
    {
        DebugLog.Write($"[DEBUG] UpdateSelectionVisuals: SelectedFigures.Count = {CanvasViewModel?.SelectedFigures?.Count ?? 0}");
        foreach (var kvp in _renderedFigures)
        {
            if (kvp.Value.Tag is FigureViewModel figure)
            {
                figure.IsSelected = CanvasViewModel?.SelectedFigures?.Contains(figure) == true;
                DebugLog.Write($"[DEBUG]   {figure.Name}: IsSelected={figure.IsSelected}");
                UpdateSelectionVisual(figure, kvp.Value);
            }
        }
    }

	/// <summary>
    /// Обработчик нажатия на отрисованную фигуру.
    /// Передаёт событие выделения в ViewModel.
    /// </summary>
    /// <param name="sender">Источник события (Control).</param>
    /// <param name="e">Аргументы события нажатия указателя.</param>
    private void OnFigurePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.Tag is FigureViewModel figure)
        {
            var addToSelection = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            CanvasViewModel?.SelectFigureAt(figure.Center, addToSelection);
            e.Handled = true;
            DebugLog.Write("[DEBUG] Entered to OnFigurePointerPressed");
        }
    }

	/// <summary>
    /// Удаляет фигуру с канваса и очищает связанные ресурсы.
    /// </summary>
    /// <param name="figure">Модель фигуры для удаления.</param>
    private void RemoveFigure(FigureViewModel figure)
	{
    	if (_renderedFigures.TryGetValue(figure.Id, out var control))
    	{
        	control.PointerPressed -= OnFigurePointerPressed;
        	if (control.Parent is Panel parent)
        	{
            	var adorner = parent.Children.OfType<Border>()
                .FirstOrDefault(b => b.Tag as string == "SelectionAdorner");
            	if (adorner != null)
            	{
                	parent.Children.Remove(adorner);
                	DebugLog.Write($"[DEBUG] RemoveFigure: Removed adorner for {figure.Name}");
            	}
        	}
        	if (DrawingCanvas != null && DrawingCanvas.Children.Contains(control))
        	{
            	DrawingCanvas.Children.Remove(control);
            	DebugLog.Write($"[DEBUG] RemoveFigure: Removed control from DrawingCanvas for {figure.Name}");
        	}
        	_renderedFigures.Remove(figure.Id);
        	DebugLog.Write($"[DEBUG] RemoveFigure: {figure.Name}, Id={figure.Id}, Remaining={_renderedFigures.Count}");
    	}
    	else
    	{
        	DebugLog.Write($"[WARN] RemoveFigure: Figure {figure.Name} not found in _renderedFigures");
    	}
	}
    
	/// <summary>
    /// Очищает все отрисованные фигуры с канваса.
    /// </summary>
    private void ClearAllFigures()
    {
        foreach (var control in _renderedFigures.Values)
        {
            control.PointerPressed -= OnFigurePointerPressed;
        }
        _renderedFigures.Clear();
        DrawingCanvas.Children.Clear();
    }

	/// <summary>
    /// Обновляет трансформацию канваса (масштаб и смещение).
    /// </summary>
    private void UpdateTransform()
    {
        var transformGroup = new TransformGroup
        {
            Children = new Transforms
            {
                new TranslateTransform(OffsetX, OffsetY),
                new ScaleTransform(Zoom, Zoom)
            }
        };
        DrawingCanvas.RenderTransform = transformGroup;
    }

    /// <summary>
    /// Преобразует экранные координаты мыши в координаты канваса с учётом зума и смещения.
    /// </summary>
    /// <param name="screenPoint">Точка в экранных координатах.</param>
    /// <returns>Точка в координатах канваса.</returns>
    public graphic_editor.Geometry.Point2D ScreenToCanvas(Avalonia.Point screenPoint)
    {
        var canvasPoint = DrawingCanvas.TranslatePoint(screenPoint, this);
        if (canvasPoint.HasValue)
        {
            return new graphic_editor.Geometry.Point2D(
                (canvasPoint.Value.X - OffsetX) / Zoom,
                (canvasPoint.Value.Y - OffsetY) / Zoom
            );
        }
        return graphic_editor.Geometry.Point2D.Zero;
    }
    
    /// <summary>Метод для конвертации System.Drawing.Color в Avalonia.Media.Color.</summary>
    /// <param name="c">Исходный цвет в формате System.Drawing.Color.</param>
    /// <returns>Цвет в формате Avalonia.Media.Color.</returns>
    private static Avalonia.Media.Color ToAvaloniaColor(System.Drawing.Color c) => 
        Avalonia.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
}