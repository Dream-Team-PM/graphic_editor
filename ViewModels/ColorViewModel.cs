// ViewModels/ColorViewModel.cs

using System.Drawing;
using System.Globalization;

using ReactiveUI;

namespace graphic_editor.ViewModels;

/// <summary>
/// Класс для работы с цветами (палитра и так далее), основывается на ViewModelBase (Находится в разработке).
/// </summary>
public class ColorViewModel: ViewModelBase
{
    private System.Drawing.Color _color; /// <summary>Приватное свойство цвета.</summary>
    
    public ColorViewModel() : this(System.Drawing.Color.Black) {} /// <summary>Конструктор ColorViewModel.</summary>
    
	/// <summary>Конструктор ColorViewModel по цвету.</summary>
    public ColorViewModel(System.Drawing.Color color) => _color = color;

	/// <summary>Публичное свойство - цвет.</summary>
    public System.Drawing.Color Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

	/// <summary>Публичная строка - цвет в 16-й системе счисления.</summary>
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
                Color = System.Drawing.Color.FromArgb(r, g, b);
            }
            // Поддержка формата #RRGGBBAA
            else if (hex.Length == 8 && 
                     byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a) &&
                     byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r) &&
                     byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g) &&
                     byte.TryParse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
            {
                Color = System.Drawing.Color.FromArgb(a, r, g, b);
            }
        }
    }

	/// <summary>Публичное свойство - байт R.</summary>
    public byte R
    {
        get => _color.R;
        set => Color = System.Drawing.Color.FromArgb(_color.A, value, _color.G, _color.B);
    }
    
	/// <summary>Публичное свойство - байт G.</summary>
    public byte G
    {
        get => _color.G;
        set => Color = System.Drawing.Color.FromArgb(_color.A, _color.R, value, _color.B);
    }
    
	/// <summary>Публичное свойство - байт B.</summary>
    public byte B
    {
        get => _color.B;
        set => Color = System.Drawing.Color.FromArgb(_color.A, _color.R, _color.G, value);
    }
    
	/// <summary>Публичное свойство - байт A.</summary>
    public byte A
    {
        get => _color.A;
        set => Color = System.Drawing.Color.FromArgb(value, _color.R, _color.G, _color.B);
    }
    
	/// <summary>Публичное свойство - конвертация из цвета в ColorViewModel.</summary>
    public static ColorViewModel FromColor(System.Drawing.Color color) => new ColorViewModel(color);
}