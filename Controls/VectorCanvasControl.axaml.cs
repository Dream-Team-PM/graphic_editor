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


namespace graphic_editor.Controls;

public partial class VectorCanvasControl : UserControl
{
    public static readonly StyledProperty<CanvasViewModel?> CanvasViewModelProperty =
        AvaloniaProperty.Register<VectorCanvasControl, CanvasViewModel?>(nameof(CanvasViewModel));
    
    public static readonly StyledProperty<ObservableCollection<FigureViewModel>?> ActiveFiguresProperty = 
        AvaloniaProperty.Register<VectorCanvasControl, ObservableCollection<FigureViewModel>?>(nameof(ActiveFigures));

    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(Zoom), 1.0);

    public static readonly StyledProperty<double> OffsetXProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(OffsetX));

    public static readonly StyledProperty<double> OffsetYProperty =
        AvaloniaProperty.Register<VectorCanvasControl, double>(nameof(OffsetY));

    private readonly Dictionary<Guid, Control> _renderedFigures = new();

    public VectorCanvasControl()
    {
        InitializeComponent();
    }

    public ObservableCollection<FigureViewModel> ActiveFigures
    {
        get => GetValue(ActiveFiguresProperty);
        set => SetValue(ActiveFiguresProperty, value);
    }

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
        SubscribeToFigures();
        RenderAllFigures();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnsubscribeFromFigures();
        base.OnDetachedFromVisualTree(e);
    }

    private void SubscribeToFigures()
    {
        if (ActiveFigures != null)
        {
            ActiveFigures.CollectionChanged += OnFiguresChanged;
        }
    }

    private void UnsubscribeFromFigures()
    {
        if (ActiveFigures != null)
        {
            ActiveFigures.CollectionChanged -= OnFiguresChanged;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanvasViewModelProperty)
        {
            UnsubscribeFromFigures();
            SubscribeToFigures();
            RenderAllFigures();
        }
        else if (change.Property == ActiveFiguresProperty)
        {
            UnsubscribeFromFigures();
            SubscribeToFigures();
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
        if (e.PropertyName == nameof(CanvasViewModel.SelectedFigure))
        {
            UpdateSelectionVisuals();
        }
    }

    private void OnFiguresChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (FigureViewModel figure in e.NewItems)
                    {
                        RenderFigure(figure);
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

    private void RenderAllFigures()
    {
        ClearAllFigures();
        if (ActiveFigures != null)
        {
            foreach (var figure in ActiveFigures)
            {
                RenderFigure(figure);
            }
        }
        UpdateTransform();
    }

    private void RenderFigure(FigureViewModel figure)
    {
        if (_renderedFigures.ContainsKey(figure.Id))
            return;
        
        var control = CreateControlForFigure(figure);
        if (control != null)
        {
            // Привязка данных к UI-элементам
            BindFigureProperties(figure, control);
        //     
             DrawingCanvas.Children.Add(control);
            _renderedFigures[figure.Id] = control;
            
            // Обработка кликов для выделения
            control.Tag = figure;
            control.PointerPressed += OnFigurePointerPressed;
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