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

using graphic_editor.ViewModels;
using graphic_editor.Geometry;
using graphic_editor.Helpers;

namespace graphic_editor.Controls;

/// <summary>
/// Основной контрол для корректной работы канваса.
/// </summary>
public partial class VectorCanvasControl : UserControl
{
    private readonly Dictionary<Guid, Control> _renderedFigures = new();
    private LayerViewModel? _currentLayer;
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

    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(Zoom), 1.0);

    public static readonly StyledProperty<double> OffsetXProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(OffsetX));

    public static readonly StyledProperty<double> OffsetYProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(OffsetY));
    
    public VectorCanvasControl()
    {
        InitializeComponent();
		//this.AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
        //this.AddHandler(PointerMovedEvent, OnPointerMoved, handledEventsToo: true);
        //this.AddHandler(PointerReleasedEvent, OnPointerReleased, handledEventsToo: true);
    }
    
    /// <summary>Единый метод применения стиля к Shape.</summary>
    /// <param name="shape">Элемент управления Shape для применения стиля.</param>
    /// <param name="figure">Модель фигуры, содержащая параметры стиля.</param>
    private void ApplyStyle(Shape shape, FigureViewModel figure)
    {
        var strokeColor = ToAvaloniaColor(figure.LineColor);
        shape.Stroke = new SolidColorBrush(strokeColor);
        // StrokeThickness — линии используют Thickness, фигуры — контур 2px
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

    public CanvasViewModel? CanvasViewModel
    {
        get => GetValue(CanvasViewModelProperty);
        set => SetValue(CanvasViewModelProperty, value);
    }

    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public double OffsetX
    {
        get => GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    public double OffsetY
    {
        get => GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    /// <summary>Метод для отображения предварительной фигуры на канвасе.</summary>
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
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeToCanvasViewModel();
        RenderAllFigures();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromFigures();
        base.OnDetachedFromVisualTree(e);
    }

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
    
    private void UnsubscribeFromCanvasViewModel()
    {
        if (CanvasViewModel != null)
        {
            CanvasViewModel.PropertyChanged -= OnCanvasViewModelPropertyChanged;
        }
    }
    
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

    private void UnsubscribeFromFigures()
    {
        if (_currentLayer != null)
        {
            _currentLayer.Figures.CollectionChanged -= OnFiguresChanged;
            _currentLayer = null;
        }
    }

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
    
    private Avalonia.Controls.Shapes.Line CreateLine(LineViewModel line) => new()
    {
        StartPoint = new Avalonia.Point(line.X1, line.Y1),
        EndPoint = new Avalonia.Point(line.X2, line.Y2),
        Tag = line
    };
    
    private Avalonia.Controls.Shapes.Rectangle CreateRectangle(RectangleViewModel r) => new()
    {
        Width = Math.Abs(r.Width),
        Height = Math.Abs(r.Height),
        [Canvas.LeftProperty] = Math.Min(r.X, r.X + r.Width),
        [Canvas.TopProperty] = Math.Min(r.Y, r.Y + r.Height),
        Tag = r
    };

    private Avalonia.Controls.Shapes.Ellipse CreateEllipse(EllipseViewModel e) => new()
    {
        Width = Math.Abs(e.Width),
        Height = Math.Abs(e.Height),
        [Canvas.LeftProperty] = Math.Min(e.X, e.X + e.Width),
        [Canvas.TopProperty] = Math.Min(e.Y, e.Y + e.Height),
        Tag = e
    };
    
    private Avalonia.Controls.Shapes.Rectangle CreateSquare(SquareViewModel square) => new()
    {
        Width = Math.Abs(square.Side),
        Height = Math.Abs(square.Side),
        [Canvas.LeftProperty] = Math.Min(square.X, square.X + square.Side),
        [Canvas.TopProperty] = Math.Min(square.Y, square.Y + square.Side),
        Tag = square
    };
    
    private Avalonia.Controls.Shapes.Ellipse CreateCircle(CircleViewModel circle) => new()
    {
        Width = Math.Abs(circle.Radius * 2),
        Height = Math.Abs(circle.Radius * 2),
        [Canvas.LeftProperty] = circle.X - circle.Radius,
        [Canvas.TopProperty] = circle.Y - circle.Radius,
        Tag = circle
    };

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

    /// <summary>Метод для привязки свойств фигуры к элементу управления.</summary>
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
                        // Перерисовываем рамку группы при изменении детей
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
    
    /// <summary>Метод для обновления визуального выделения фигуры.</summary>
    /// <param name="figure">Модель фигуры для обновления выделения.</param>
    /// <param name="control">Элемент управления, отображающий фигуру.</param>
    private void UpdateSelectionVisual(FigureViewModel figure, Control control)
    {
        DebugLog.Write($"[DEBUG] UpdateSelectionVisual: {figure.Name}, IsSelected={figure.IsSelected}, Control={control?.GetType().Name}, Parent={control?.Parent?.GetType().Name}");
        if (control.Parent is not Panel parent)
    	{
        	DebugLog.Write($"[WARN] UpdateSelectionVisual: control.Parent is null or not Panel, skipping");
        	return;  // ← Выходим, не пытаемся добавить adorner
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
    
        // ✅ Для обычных фигур (Rectangle, Ellipse, Path, Line)
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
                    // Для полигонов — используем bounding box фигуры
                    var bbox = figure.GetBoundingBox();
                    border.Width = bbox.MaxX - bbox.MinX;
                    border.Height = bbox.MaxY - bbox.MinY;
                    Canvas.SetLeft(border, bbox.MinX);
                    Canvas.SetTop(border, bbox.MinY);
                }
                else if (shape is Avalonia.Controls.Shapes.Line line)
                {
                    // Для линии — используем StartPoint и EndPoint
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

    /// <summary>Метод для обновления визуального выделения всех фигур.</summary>
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

    private void RemoveFigure(FigureViewModel figure)
{
    if (_renderedFigures.TryGetValue(figure.Id, out var control))
    {
        // ✅ Отписываемся от событий
        control.PointerPressed -= OnFigurePointerPressed;
        
        // ✅ Удаляем adorner если есть
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
        
        // ✅ Удаляем из Canvas
        if (DrawingCanvas != null && DrawingCanvas.Children.Contains(control))
        {
            DrawingCanvas.Children.Remove(control);
            DebugLog.Write($"[DEBUG] RemoveFigure: Removed control from DrawingCanvas for {figure.Name}");
        }
        
        // ✅ Удаляем из словаря
        _renderedFigures.Remove(figure.Id);
        
        DebugLog.Write($"[DEBUG] RemoveFigure: {figure.Name}, Id={figure.Id}, Remaining={_renderedFigures.Count}");
    }
    else
    {
        DebugLog.Write($"[WARN] RemoveFigure: Figure {figure.Name} not found in _renderedFigures");
    }
}
    
    private void ClearAllFigures()
    {
        foreach (var control in _renderedFigures.Values)
        {
            control.PointerPressed -= OnFigurePointerPressed;
        }
        _renderedFigures.Clear();
        DrawingCanvas.Children.Clear();
    }

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

    // Метод для конвертации координат мыши в координаты холста
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