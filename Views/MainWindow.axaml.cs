using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Input;
using graphic_editor.ViewModels;

namespace graphic_editor;

public partial class MainWindow : Window
{
    // Параметры выделенного объекта (для демонстрации)
    private double _objectX = 120;
    private double _objectY = 240;
    private double _objectScale = 1.0;
    private double _objectRotation = 0;
    private string _objectColor = "#FF4A90";

	private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
		_viewModel = new MainWindowViewModel();
		DataContext = _viewModel;

        // Начальные значения
        SelectedToolText.Text = "Выделение";
        StrokePercentText.Text = "75%";
        OpacityText.Text = $"Непрозрачность: 100%";

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
		var position = e.GetPosition(this);
		_viewModel.UpdateCoordinatesCommand.Execute((position.X, position.Y));
	}

	private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e) 
    {
		if (_viewModel == null) return;
		var position = e.GetPosition(this);
		var point = new graphic_editor.Models.Point_1(position.X, position.Y);
		_viewModel.CanvasClickedCommand.Execute(point);
    }

    private void ToolButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton btn && btn.IsChecked == true && btn.Tag is string toolName)
        {
			_viewModel.SelectedTool = toolName;
            SelectedToolText.Text = toolName;
        }
    }

    private void StrokeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
		if (_viewModel != null)
            _viewModel.StrokeWidth = (int)e.NewValue;
        StrokePercentText.Text = $"{(int)e.NewValue}%";
    }

    private void OpacitySlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
		if (_viewModel != null)
            _viewModel.Opacity = e.NewValue;
        OpacityText.Text = $"Непрозрачность: {(int)e.NewValue}%";
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

    private void MoveCenter_Click(object? sender, RoutedEventArgs e)
    {
        _objectX = 450;
        _objectY = 310;
        ShowStatus("Перемещение: по центру");
    }

    // ========== КОНТЕКСТНОЕ МЕНЮ - ЦВЕТ ==========
    private void ColorRed_Click(object? sender, RoutedEventArgs e)
    {
        _objectColor = "#FF0000";
        ShowStatus("Цвет: Красный");
    }

    private void ColorGreen_Click(object? sender, RoutedEventArgs e)
    {
        _objectColor = "#00FF00";
        ShowStatus("Цвет: Зелёный");
    }

    private void ColorBlue_Click(object? sender, RoutedEventArgs e)
    {
        _objectColor = "#0000FF";
        ShowStatus("Цвет: Синий");
    }

    private void ColorYellow_Click(object? sender, RoutedEventArgs e)
    {
        _objectColor = "#FFFF00";
        ShowStatus("Цвет: Жёлтый");
    }

    private void ColorWhite_Click(object? sender, RoutedEventArgs e)
    {
        _objectColor = "#FFFFFF";
        ShowStatus("Цвет: Белый");
    }

    private void ColorBlack_Click(object? sender, RoutedEventArgs e)
    {
        _objectColor = "#000000";
        ShowStatus("Цвет: Чёрный");
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
        // Находим TextBlock в статус-баре и обновляем текст
        var statusText = this.FindControl<TextBlock>("StatusText");
        if (statusText != null)
        {
            statusText.Text = message;
        }
    }

	private void ThemeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_viewModel == null) return;
        
        _viewModel.CurrentTheme = e.NewValue >= 0.5 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        
        this.RequestedThemeVariant = _viewModel.CurrentTheme;
		ShowStatus($"Тема: {(e.NewValue >= 0.5 ? "Светлая ☀️" : "Тёмная 🌙")}");
    }
}