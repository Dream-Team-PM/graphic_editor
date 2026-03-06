using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using System;

namespace graphic_editor;

public partial class ColorPickerPopup : UserControl
{
    public event Action<Color>? ColorSelected;
    public event Action? Cancelled;
    
    
    private static readonly string[] Swatches = {
        "#FF4A90","#FF6B6B","#FF9F43","#FECA57",
        "#48DBFB","#3A86FF","#8338EC","#06D6A0",
        "#FFFFFF","#AAAAAA","#555555","#000000"
    };
    
    public ColorPickerPopup()
    {
        InitializeComponent();
        InitializeSwatches();
    }
    
    private void InitializeSwatches()
    {
        SwatchPanel.Children.Clear();
        foreach (var hex in Swatches)
        {
            var b = new Border {
                Width = 20, Height = 20, Margin = new Avalonia.Thickness(2),
                CornerRadius = new Avalonia.CornerRadius(3),
                Background = Brush.Parse(hex),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            b.PointerPressed += (_, _) => { 
                HexInput.Text = hex.TrimStart('#'); 
                UpdatePreviewFromHex();
            };
            SwatchPanel.Children.Add(b);
        }
    }
    
    public void SetColor(Color color)
    {
        _currentColor = color;
        PreviewBorder.Background = new SolidColorBrush(color);
        HexInput.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    
    private Color _currentColor;
    
    private void UpdatePreviewFromHex()
    {
        if (HexInput.Text?.Length == 6)
        {
            try
            {
                var color = Color.Parse("#" + HexInput.Text);
                _currentColor = color;
                PreviewBorder.Background = new SolidColorBrush(color);
            }
            catch { }
        }
    }
    
    private void HexInput_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdatePreviewFromHex();
    }
    
    private void Ok_Click(object? sender, RoutedEventArgs e)
    {
        try 
        { 
            ColorSelected?.Invoke(_currentColor); 
        } 
        catch { }
    }
    
    private void Cancel_Click(object? sender, RoutedEventArgs e) => Cancelled?.Invoke();
}