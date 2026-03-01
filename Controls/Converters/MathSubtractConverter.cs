// Controls/Converters//MathSubtractConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

/// <summary>
/// Конвертер для вычитания: значение - параметр
/// </summary>
public class MathSubtractConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is double d1 && values[1] is double d2)
            return Math.Abs(d1 - d2);
        return 0.0;
    }
}