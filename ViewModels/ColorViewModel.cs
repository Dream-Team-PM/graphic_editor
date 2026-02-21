// ViewModels/ColorViewModel.cs

using System.Drawing;
using System.Globalization;

using ReactiveUI;

namespace graphic_editor.ViewModels;

public class ColorViewModel: ViewModelBase
{
    private Color _color;
    
    public ColorViewModel() : this(Color.Black) {}
    
    public ColorViewModel(Color color) => _color = color;

    public Color Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    public string HexColor
    {
        get => $"#{_color.R:X2}{_color.G:X2}{_color.B:X2}";
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            
            var hex = value.Replace("#", string.Empty);
            if (hex.Length == 6 && 
                byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
                byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
                byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                Color = Color.FromArgb(r, g, b);
            }
            // Поддержка формата #RRGGBBAA
            else if (hex.Length == 8 && 
                     byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a) &&
                     byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r) &&
                     byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g) &&
                     byte.TryParse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
            {
                Color = Color.FromArgb(a, r, g, b);
            }
        }
    }

    public byte R
    {
        get => _color.R;
        set => Color = Color.FromArgb(_color.A, value, _color.G, _color.B);
    }
    
    public byte G
    {
        get => _color.G;
        set => Color = Color.FromArgb(_color.A, _color.R, value, _color.B);
    }
    
    public byte B
    {
        get => _color.B;
        set => Color = Color.FromArgb(_color.A, _color.R, _color.G, value);
    }
    
    public byte A
    {
        get => _color.A;
        set => Color = Color.FromArgb(value, _color.R, _color.G, _color.B);
    }
    
    public static ColorViewModel FromColor(Color color) => new ColorViewModel(color);
}