using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;  // ✅ Правильное пространство имён
using graphic_editor.Controls;
using graphic_editor.Geometry;

namespace graphic_editor.Views.Components;

/// <summary>
/// Компонент рабочей области редактора (холст + линейки + контекстное меню).
/// </summary>
public partial class EditorWorkspace : UserControl
{
    // ── События для подписки из MainWindow ──
    public event EventHandler<PointerEventArgs>? CanvasPointerMoved;
    public event EventHandler<PointerPressedEventArgs>? CanvasPointerPressed;
    public event EventHandler<PointerReleasedEventArgs>? CanvasPointerReleased;
    public event EventHandler<RoutedEventArgs>? NewDocumentClicked;

    // ── Публичные свойства для доступа к элементам ──
    public VectorCanvasControl VectorCanvasElement => VectorCanvas;
    public Border CanvasBorderElement => CanvasBorder;
    public ScrollViewer CanvasScrollViewerElement => CanvasScrollViewer;

    public EditorWorkspace()
    {
        InitializeComponent();
        SubscribeToCanvasEvents();
    }

    private void SubscribeToCanvasEvents()
    {
        // ✅ Подписка на события указателя холста
        if (VectorCanvas != null)
        {
            VectorCanvas.AddHandler(PointerPressedEvent, OnCanvasPointerPressed, handledEventsToo: true);
            VectorCanvas.AddHandler(PointerMovedEvent, OnCanvasPointerMoved, handledEventsToo: true);
            VectorCanvas.AddHandler(PointerReleasedEvent, OnCanvasPointerReleased, handledEventsToo: true);
        }

        // Кнопка нового документа
        if (NewDocumentButton is Button newDocBtn)
        {
            newDocBtn.Click += (s, e) => NewDocumentClicked?.Invoke(s, e);
        }
    }

    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        CanvasPointerMoved?.Invoke(sender, e);
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CanvasPointerPressed?.Invoke(sender, e);
    }

    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        CanvasPointerReleased?.Invoke(sender, e);
    }

    /// <summary>
    /// Преобразует экранные координаты в координаты холста.
    /// Возвращает Point2D из вашей геометрии.
    /// </summary>
    public Point2D ScreenToCanvas(Avalonia.Point screenPoint)
    {
        // ✅ Явная проверка на null вместо ?. с ??
        if (VectorCanvas != null)
        {
            return VectorCanvas.ScreenToCanvas(screenPoint);
        }
        // ✅ Возвращаем совместимый тип: преобразуем Avalonia.Point в Point2D
        return new Point2D(screenPoint.X, screenPoint.Y);
    }
}