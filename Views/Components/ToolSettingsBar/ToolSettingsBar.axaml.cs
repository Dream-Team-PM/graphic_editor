using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace graphic_editor.Views.Components;

/// <summary>
/// Компонент панели параметров инструмента (цвета, толщина обводки).
/// </summary>
public partial class ToolSettingsBar : UserControl
{
    // ── События для подписки из MainWindow ──
    public event EventHandler<RoutedEventArgs>? FillColorClicked;
    public event EventHandler<RoutedEventArgs>? StrokeColorClicked;
    public event EventHandler<RangeBaseValueChangedEventArgs>? StrokeSliderValueChanged;

    // ── Публичные свойства для доступа к элементам ──
    public TextBlock SelectedToolTextElement => SelectedToolText;
    public TextBlock StrokePercentTextElement => StrokePercentText;
    public Popup ColorPopupElement => ColorPopup;
    public Popup StrokeColorPopupElement => StrokeColorPopup;
    public ColorPickerPopup ColorPickerControlElement => ColorPickerControl;
    public ColorPickerPopup StrokeColorPickerControlElement => StrokeColorPickerControl;
    public Slider StrokeSliderElement => StrokeSlider;

    public ToolSettingsBar()
    {
        InitializeComponent();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        // Подписка на клики по кнопкам цвета
        FillColorButton.Click += (s, e) => FillColorClicked?.Invoke(s, e);
        StrokeColorButton.Click += (s, e) => StrokeColorClicked?.Invoke(s, e);
        
        // Подписка на изменение слайдера
        StrokeSlider.ValueChanged += (s, e) => StrokeSliderValueChanged?.Invoke(s, e);
    }

    /// <summary>
    /// Устанавливает название текущего инструмента.
    /// </summary>
    public void SetSelectedToolName(string toolName)
    {
        SelectedToolText.Text = toolName;
    }

    /// <summary>
    /// Устанавливает текст процента толщины обводки.
    /// </summary>
    public void SetStrokePercentText(string percentText)
    {
        StrokePercentText.Text = percentText;
    }

    /// <summary>
    /// Открывает popup цветового пикера заливки.
    /// </summary>
    public void OpenFillColorPicker()
    {
        ColorPopup.IsOpen = true;
    }

    /// <summary>
    /// Открывает popup цветового пикера обводки.
    /// </summary>
    public void OpenStrokeColorPicker()
    {
        StrokeColorPopup.IsOpen = true;
    }

    /// <summary>
    /// Закрывает все popup пикеров.
    /// </summary>
    public void CloseAllColorPickers()
    {
        ColorPopup.IsOpen = false;
        StrokeColorPopup.IsOpen = false;
    }
}