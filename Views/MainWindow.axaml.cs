using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace graphic_editor;

public partial class MainWindow : Window
{
    // Параметры выделенного объекта (для демонстрации)
    private double _objectX = 120;
    private double _objectY = 240;
    private double _objectScale = 1.0;
    private double _objectRotation = 0;
    private string _objectColor = "#FF4A90";

    public MainWindow()
    {
        InitializeComponent();

        // Начальные значения
        SelectedToolText.Text = "Выделение";
        StrokePercentText.Text = "75%";
        OpacityText.Text = $"Непрозрачность: 100%";
    }

    private void ToolButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton btn && btn.IsChecked == true && btn.Tag is string toolName)
        {
            SelectedToolText.Text = toolName;
        }
    }

    private void StrokeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        StrokePercentText.Text = $"{(int)e.NewValue}%";
    }

    private void OpacitySlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
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
}