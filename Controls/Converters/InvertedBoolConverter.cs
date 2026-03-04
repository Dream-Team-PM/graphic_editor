// Controls/Converters/InvertedBoolConverter.cs

using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

/// <summary>
/// Конвертер значений для инверсии логического значения (true ↔ false).
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML.
    /// </summary>
    public static readonly InvertedBoolConverter Instance = new();

    /// <summary>
    /// Инвертирует входное логическое значение.
    /// </summary>
    /// <param name="value">Входное значение типа <see cref="bool"/>.</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see langword="true"/>, если входное значение <see langword="false"/>, и наоборот.
    /// Возвращает <see langword="null"/>, если входное значение не является <see cref="bool"/>.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    /// <summary>
    /// Инвертирует входное логическое значение (обратное преобразование).
    /// </summary>
    /// <param name="value">Входное значение типа <see cref="bool"/>.</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see langword="true"/>, если входное значение <see langword="false"/>, и наоборот.
    /// Возвращает <see langword="null"/>, если входное значение не является <see cref="bool"/>.
    /// </returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}