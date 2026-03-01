// Views/MainWindow.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Input;

using graphic_editor.ViewModels;
using graphic_editor.Helpers;

namespace graphic_editor;

/// <summary>
/// Основное (главное) окно графического редактора.
/// </summary> 
public partial class MainWindow : Window
{
    // Параметры выделенного объекта (для демонстрации)
    private double _objectX = 120;
    private double _objectY = 240;
    private double _objectScale = 1.0;
    private double _objectRotation = 0;

	private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
		_viewModel = new MainWindowViewModel();
		DataContext = _viewModel;
        // Начальные значения
        SelectedToolText.Text = "Выделение";
        StrokePercentText.Text = "50%";
		if (this.FindControl<Canvas>("MainCanvas") is Canvas canvas) 
		{
			canvas.AddHandler(PointerMovedEvent, OnCanvasPointerMoved);
			canvas.AddHandler(PointerPressedEvent, OnCanvasPointerPressed);
		}
        if (ThemeSlider != null)
        {
            ThemeSlider.Value = _viewModel.CurrentTheme == ThemeVariant.Light ? 1 : 0;
        }
    }

	private void OnCanvasPointerMoved(object? sender, PointerEventArgs e) 
	{
    	if (_viewModel == null) return;
    	// Обновляем координаты
    	var screenPos = e.GetPosition(VectorCanvas);
    	var canvasPoint = VectorCanvas.ScreenToCanvas(screenPos);
    	_viewModel.Commands.UpdateCoordinates.Execute((canvasPoint.X, canvasPoint.Y));
    	_viewModel.HandlePointerMoved(e);
	}

	private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e) 
	{
    	if (_viewModel == null) return;
    	var screenPos = e.GetPosition(VectorCanvas);
    	var point = VectorCanvas.ScreenToCanvas(screenPos);
    	_viewModel.HandlePointerPressed(e);
	}

	private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e) 
	{
    	if (_viewModel == null) return;
    	var screenPos = e.GetPosition(VectorCanvas);
    	var point = VectorCanvas.ScreenToCanvas(screenPos);
    	_viewModel.HandlePointerReleased(e);
	}

    private void ToolButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton btn && btn.IsChecked == true && btn.Tag is string toolName)
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: Setting SelectedTool to '{toolName}' (Tag={btn.Tag})");
            _viewModel.SetToolByName(toolName);
            SelectedToolText.Text = toolName;
        }
        else
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: sender={sender?.GetType()}, IsChecked={(sender as RadioButton)?.IsChecked}, Tag={(sender as RadioButton)?.Tag}");
        }
    }

    private void StrokeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
		StrokePercentText.Text = $"{(int)e.NewValue}%";
    }

    // ========== КОНТЕКСТНОЕ МЕНЮ - ПОВОРОТ ==========
    private void RotateLeft_Click(object? sender, RoutedEventArgs e)
    {
        _objectRotation -= 90;
        ShowStatus($"Поворот: {_objectRotation}°");
    }

    private void RotateRight_Click(object? sender, RoutedEventArgs e)
    {
        _objectRotation += 90;
        ShowStatus($"Поворот: {_objectRotation}°");
    }

    private void Rotate180_Click(object? sender, RoutedEventArgs e)
    {
        _objectRotation += 180;
        ShowStatus($"Поворот: {_objectRotation}°");
    }

    private void RotateFree_Click(object? sender, RoutedEventArgs e)
    {
        ShowStatus("Открытие диалога поворота...");
    }

    // ========== КОНТЕКСТНОЕ МЕНЮ - МАСШТАБ ==========
    private void ScaleUp_Click(object? sender, RoutedEventArgs e)
    {
        _objectScale *= 1.5;
        ShowStatus($"Масштаб: {_objectScale:P0}");
    }

    private void ScaleDown_Click(object? sender, RoutedEventArgs e)
    {
        _objectScale *= 0.5;
        ShowStatus($"Масштаб: {_objectScale:P0}");
    }

    private void ScaleFit_Click(object? sender, RoutedEventArgs e)
    {
        _objectScale = 1.0;
        ShowStatus("Масштаб: по размеру окна");
    }

    private void ScaleOriginal_Click(object? sender, RoutedEventArgs e)
    {
        _objectScale = 1.0;
        ShowStatus("Масштаб: оригинальный размер");
    }

    // ========== КОНТЕКСТНОЕ МЕНЮ - ОТРАЖЕНИЕ ==========
    private void FlipHorizontal_Click(object? sender, RoutedEventArgs e)
    {
        ShowStatus("Отражение: по горизонтали");
    }

    private void FlipVertical_Click(object? sender, RoutedEventArgs e)
    {
        ShowStatus("Отражение: по вертикали");
    }

    // ========== КОНТЕКСТНОЕ МЕНЮ - ПЕРЕМЕЩЕНИЕ ==========
    private void MoveUp_Click(object? sender, RoutedEventArgs e)
    {
        _objectY -= 10;
        ShowStatus($"Перемещение: Y = {_objectY}");
    }

    private void MoveDown_Click(object? sender, RoutedEventArgs e)
    {
        _objectY += 10;
        ShowStatus($"Перемещение: Y = {_objectY}");
    }

    private void MoveLeft_Click(object? sender, RoutedEventArgs e)
    {
        _objectX -= 10;
        ShowStatus($"Перемещение: X = {_objectX}");
    }

    private void MoveRight_Click(object? sender, RoutedEventArgs e)
    {
        _objectX += 10;
        ShowStatus($"Перемещение: X = {_objectX}");
    }

    private void ColorPicker_Click(object? sender, RoutedEventArgs e)
    {
        ShowStatus("Открытие выбора цвета...");
    }

    // ========== КОНТЕКСТНОЕ МЕНЮ - ДРУГОЕ ==========
    private void DeleteObject_Click(object? sender, RoutedEventArgs e)
    {
        ShowStatus("Объект удалён");
    }

    private void DuplicateObject_Click(object? sender, RoutedEventArgs e)
    {
        ShowStatus("Объект дублирован");
    }

    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========
    private void ShowStatus(string message)
    {
        //StatusText.Text = message;
    }

	private void ThemeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_viewModel == null) return;
        
        _viewModel.CurrentTheme = e.NewValue >= 0.5 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        
        this.RequestedThemeVariant = _viewModel.CurrentTheme;
        _viewModel.ToggleTheme();
    }
}