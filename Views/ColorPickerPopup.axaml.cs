using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Interactivity;
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
        foreach (var hex in Swatches)
        {
            var b = new Border {
                Width = 20, Height = 20, Margin = new Avalonia.Thickness(2),
                CornerRadius = new Avalonia.CornerRadius(3),
                Background = Brush.Parse(hex),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            b.PointerPressed += (_, _) => { HexInput.Text = hex.TrimStart('#'); };
            SwatchPanel.Children.Add(b);
        }
    }

    private void HexInput_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (HexInput.Text?.Length == 6)
            try { PreviewBorder.Background = Brush.Parse("#" + HexInput.Text); } catch { }
    }

    private void Ok_Click(object? sender, RoutedEventArgs e)
    {
        try { ColorSelected?.Invoke(Color.Parse("#" + HexInput.Text)); } catch { }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Cancelled?.Invoke();
}