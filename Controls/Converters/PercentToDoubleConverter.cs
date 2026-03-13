// Controls/Converters/PercentToDoubleConverter.cs
using Avalonia.Data.Converters;
using System.Globalization;

namespace graphic_editor.Converters;

/// <summary>
/// Конвертер значений для преобразования между процентами (0–100) и коэффициентом (0.0–1.0).
/// </summary>
public class PercentToDoubleConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML.
    /// </summary>
    public static readonly PercentToDoubleConverter Instance = new();
    
    /// <summary>
    /// Преобразует процентное значение в коэффициент.
    /// </summary>
    /// <param name="value">Входное значение в процентах (тип <see cref="double"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// Коэффициент в диапазоне 0.0–1.0 (например, 100 → 1.0), или 1.0 при ошибке.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
            return percent / 100.0;  // 100 → 1.0
        return 1.0;
    }
    
    /// <summary>
    /// Преобразует коэффициент в процентное значение.
    /// </summary>
    /// <param name="value">Входное значение-коэффициент (тип <see cref="double"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// Процентное значение в диапазоне 0–100 (например, 1.0 → 100), или 100.0 при ошибке.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double opacity)
            return opacity * 100.0;  // 1.0 → 100
        return 100.0;
    }
}