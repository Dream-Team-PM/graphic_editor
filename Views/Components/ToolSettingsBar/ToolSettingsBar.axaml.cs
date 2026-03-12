using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace graphic_editor.Views.Components;

public partial class ToolSettingsBar : UserControl
{
    public ToolSettingsBar()
    {
        InitializeComponent();
        FillColorButton.Click += OnFillColorClicked;
        StrokeColorButton.Click += OnStrokeColorClicked;
        StrokeSlider.ValueChanged += OnStrokeSliderValueChanged;
    }

    public TextBlock SelectedToolTextElement => SelectedToolText;
    public TextBlock StrokePercentTextElement => StrokePercentText;
    public Popup ColorPopupElement => ColorPopup;
    public Popup StrokeColorPopupElement => StrokeColorPopup;
    public ColorPickerPopup ColorPickerControlElement => ColorPickerControl;
    public ColorPickerPopup StrokeColorPickerControlElement => StrokeColorPickerControl;

    public event EventHandler<RoutedEventArgs>? FillColorClicked;
    public event EventHandler<RoutedEventArgs>? StrokeColorClicked;
    public event EventHandler<RangeBaseValueChangedEventArgs>? StrokeSliderValueChanged;

    private void OnFillColorClicked(object? sender, RoutedEventArgs e)
    {
        FillColorClicked?.Invoke(sender, e);
    }

    private void OnStrokeColorClicked(object? sender, RoutedEventArgs e)
    {
        StrokeColorClicked?.Invoke(sender, e);
    }

    private void OnStrokeSliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        StrokeSliderValueChanged?.Invoke(sender, e);
    }
}
