// Controls/VectorCanvasControl.axaml.cs
using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using graphic_editor.ViewModels;
using graphic_editor.Models;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using graphic_editor.Helpers;

namespace graphic_editor.Controls;

public partial class VectorCanvasControl : UserControl
{
    private readonly Dictionary<Guid, Control> _renderedFigures = new();
    private LayerViewModel? _currentLayer;
    public static readonly StyledProperty<CanvasViewModel?> CanvasViewModelProperty =
        AvaloniaProperty.Register<VectorCanvasControl, CanvasViewModel?>(nameof(CanvasViewModel));
    
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
    }

    // public ObservableCollection<FigureViewModel> ActiveFigures
    // {
    //     get => GetValue(ActiveFiguresProperty);
    //     set => SetValue(ActiveFiguresProperty, value);
    // }

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
            // Подписываемся на изменения ActiveLayer
            CanvasViewModel.PropertyChanged += OnCanvasViewModelPropertyChanged;
            // Подписываемся на фигуры текущего активного слоя
            SubscribeToCurrentLayer();
            // SubscribeToLayerFigures(CanvasViewModel.ActiveLayer);
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
        // else if (change.Property == ActiveFiguresProperty)
        // {
        //     UnsubscribeFromFigures();
        //     SubscribeToFigures();
        //     RenderAllFigures();
        // }
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
            // SubscribeToLayerFigures(CanvasViewModel.ActiveLayer);
            DebugLog.Write($"[DEBUG] ActiveLayer changed, new value: {CanvasViewModel?.ActiveLayer?.Name ?? "null"}");
            SubscribeToCurrentLayer();
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
        } catch (Exception ex)
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
        
            // ⚠️ Проверка на null перед добавлением!
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
            RectangleViewModel rect => CreateRectangle(rect),
            EllipseViewModel ellipse => CreateEllipse(ellipse),
            PenPointViewModel pen => CreatePenPoint(pen), 
            // TODO: Добавить другие фигуры
            _ => null
        };
    }
    
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

    private Avalonia.Controls.Shapes.Ellipse CreatePenPoint(PenPointViewModel pen) => new()
    {
        // Точка рисуется как маленький круг радиусом Thickness/2 + 2px для видимости
        // var radius = Math.Max(2, pen.Thickness / 2 + 2);
        Width = Math.Max(2, pen.Thickness / 2 + 2) * 2,
        Height = Math.Max(2, pen.Thickness / 2 + 2) * 2,
        [Canvas.LeftProperty] = pen.X - Math.Max(2, pen.Thickness / 2 + 2), // Центрируем относительно координаты
        [Canvas.TopProperty] = pen.Y - Math.Max(2, pen.Thickness / 2 + 2),
        Tag = pen
    };

    private void BindFigureProperties(FigureViewModel figure, Control control)
    {
        // Конвертация цвета
        if (control is not Shape shape) return;
        // var strokeBrush = new SolidColorBrush(ToAvaloniaColor(figure.LineColor));
        //
        // if (control is Shape shape)
        // {
        //     shape.Stroke = strokeBrush;
        //     shape.StrokeThickness = figure.Thickness;
        //
        //     if (figure.FillColor.A > 0)
        //     {
        //         shape.Fill = new SolidColorBrush(ToAvaloniaColor(figure.FillColor));
        //     }
        // }
        var strokeColor = ToAvaloniaColor(figure.LineColor);
        var fillColor = figure.FillColor.A > 0 ? ToAvaloniaColor(figure.FillColor) : strokeColor;
    
        shape.Stroke = new SolidColorBrush(strokeColor);
        shape.StrokeThickness = 1;  // Для точек обводка минимальная
        shape.Fill = new SolidColorBrush(fillColor);

        // Подписка на изменения
        figure.PropertyChanged += (s, e) =>
        {
            if (control is not Shape shapeCtrl) return;
        
            if (e.PropertyName == nameof(FigureViewModel.LineColor))
            {
                shapeCtrl.Stroke = new SolidColorBrush(ToAvaloniaColor(figure.LineColor));
                if (figure.FillColor.A == 0)
                    shapeCtrl.Fill = new SolidColorBrush(ToAvaloniaColor(figure.LineColor));
            }
            else if (e.PropertyName == nameof(FigureViewModel.FillColor))
            {
                shapeCtrl.Fill = figure.FillColor.A > 0 
                    ? new SolidColorBrush(ToAvaloniaColor(figure.FillColor)) 
                    : new SolidColorBrush(ToAvaloniaColor(figure.LineColor));
            }
            else if (e.PropertyName == nameof(FigureViewModel.Thickness))
            {
                shapeCtrl.StrokeThickness = figure.Thickness;
            }
            else if (e.PropertyName == nameof(FigureViewModel.IsSelected))
            {
                UpdateSelectionVisual(figure, control);
            }
        };
    }

    private void UpdateSelectionVisual(FigureViewModel figure, Control control)
    {
        if (figure.IsSelected)
        {
            // Добавляем визуальное выделение (рамку)
            if (control is Shape shape && shape.Tag is FigureViewModel)
            {
                // Можно добавить эффект свечения или рамку
                shape.Opacity = 1.0;
            }
        }
        else
        {
            if (control is Shape shape)
            {
                shape.Opacity = 1.0;
            }
        }
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var kvp in _renderedFigures)
        {
            if (kvp.Value.Tag is FigureViewModel figure)
            {
                UpdateSelectionVisual(figure, kvp.Value);
            }
        }
    }

    private void OnFigurePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.Tag is FigureViewModel figure)
        {
            CanvasViewModel?.SelectFigureAt(figure.Center);
            e.Handled = true;
        }
    }

    private void RemoveFigure(FigureViewModel figure)
    {
        if (_renderedFigures.TryGetValue(figure.Id, out var control))
        {
            control.PointerPressed -= OnFigurePointerPressed;
             DrawingCanvas.Children.Remove(control);
            _renderedFigures.Remove(figure.Id);
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
    public graphic_editor.Models.Point_1 ScreenToCanvas(Avalonia.Point screenPoint)
    {
        var canvasPoint = DrawingCanvas.TranslatePoint(screenPoint, this);
        if (canvasPoint.HasValue)
        {
            return new graphic_editor.Models.Point_1(
                (canvasPoint.Value.X - OffsetX) / Zoom,
                (canvasPoint.Value.Y - OffsetY) / Zoom
            );
        }
        return graphic_editor.Models.Point_1.Zero;
    }
    
    /// <summary>
    /// Конвертирует System.Drawing.Color в Avalonia.Media.Color
    /// </summary>
    private static Avalonia.Media.Color ToAvaloniaColor(System.Drawing.Color c) => 
        Avalonia.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
}