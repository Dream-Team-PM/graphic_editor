// Controls/Converters/ColorToBrushConverter.cs
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace graphic_editor.Converters;

/// <summary>
/// Конвертер значений для преобразования <see cref="System.Drawing.Color"/> в <see cref="SolidColorBrush"/> Avalonia.
/// </summary>
public class ColorToBrushConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML.
    /// </summary>
    public static readonly ColorToBrushConverter Instance = new();
    
    /// <summary>
    /// Преобразует <see cref="System.Drawing.Color"/> в <see cref="SolidColorBrush"/>.
    /// </summary>
    /// <param name="value">Исходное значение цвета (<see cref="System.Drawing.Color"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see cref="SolidColorBrush"/> с соответствующим цветом, или <see cref="Brushes.Black"/> при ошибке.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Drawing.Color color)
            return new SolidColorBrush(
                Color.FromArgb(color.A, color.R, color.G, color.B));
        return Brushes.Black;
    }
    
    /// <summary>
    /// Преобразует <see cref="SolidColorBrush"/> обратно в <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="value">Исходное значение кисти (<see cref="SolidColorBrush"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see cref="System.Drawing.Color"/> с соответствующими компонентами, или <see cref="System.Drawing.Color.Black"/> при ошибке.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
            return System.Drawing.Color.FromArgb(
                brush.Color.A,
                brush.Color.R,
                brush.Color.G,
                brush.Color.B);
        return System.Drawing.Color.Black;
    }
}