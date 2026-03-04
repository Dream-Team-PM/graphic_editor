// Controls/Converters//MathSubtractConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

/// <summary>
/// Конвертер множественных значений для вычисления абсолютной разности двух чисел.
/// </summary>
public class MathSubtractConverter : IMultiValueConverter
{
    /// <summary>
    /// Вычисляет абсолютную разность двух входных значений.
    /// </summary>
    /// <param name="values">Коллекция входных значений (минимум два элемента типа <see cref="double"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// Абсолютная разность |value1 - value2| типа <see cref="double"/>, или 0.0 при ошибке.
    /// </returns>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is double d1 && values[1] is double d2)
            return Math.Abs(d1 - d2);
        return 0.0;
    }
}