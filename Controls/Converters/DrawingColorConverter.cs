// Controls/Converters/DrawingColorConverter.cs
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Drawing;
using System.Globalization;

namespace graphic_editor.Converters; 

/// <summary>
/// Конвертер значений для преобразования между <see cref="System.Drawing.Color"/> и <see cref="Avalonia.Media.Color"/>.
/// </summary>
public class DrawingColorConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML.
    /// </summary>
    public static readonly DrawingColorConverter Instance = new();
    
    /// <summary>
    /// Преобразует <see cref="System.Drawing.Color"/> в <see cref="Avalonia.Media.Color"/>.
    /// </summary>
    /// <param name="value">Исходное значение цвета (<see cref="System.Drawing.Color"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see cref="Avalonia.Media.Color"/> с соответствующими компонентами, или <see cref="Colors.Black"/> при ошибке.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Drawing.Color c)
            return Avalonia.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        return Colors.Black;
    }
    
    /// <summary>
    /// Преобразует <see cref="Avalonia.Media.Color"/> обратно в <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="value">Исходное значение цвета (<see cref="Avalonia.Media.Color"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see cref="System.Drawing.Color"/> с соответствующими компонентами, или <see cref="System.Drawing.Color.Black"/> при ошибке.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Avalonia.Media.Color c)
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        return System.Drawing.Color.Black;
    }
}