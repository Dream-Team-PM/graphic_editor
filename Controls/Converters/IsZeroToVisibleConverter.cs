// Controls/Converters/IsZeroToVisibleConverter.cs

using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

/// <summary>
/// Конвертер значений для проверки, равно ли целое число нулю.
/// Используется для управления видимостью элементов UI.
/// </summary>
public class IsZeroToVisibleConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML.
    /// </summary>
    public static readonly IsZeroToVisibleConverter Instance = new();

    /// <summary>
    /// Проверяет, равно ли входное значение нулю.
    /// </summary>
    /// <param name="value">Входное значение (ожидается тип <see cref="int"/>).</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации (не используется).</param>
    /// <param name="culture">Культура для преобразования (не используется).</param>
    /// <returns>
    /// <see langword="true"/>, если значение равно 0; иначе <see langword="false"/>.
    /// Возвращает <see langword="null"/>, если входное значение не является <see cref="int"/>.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int count && count == 0;
    }

    /// <summary>
    /// Обратное преобразование не поддерживается.
    /// </summary>
    /// <param name="value">Входное значение.</param>
    /// <param name="targetType">Целевой тип преобразования.</param>
    /// <param name="parameter">Дополнительный параметр конвертации.</param>
    /// <param name="culture">Культура для преобразования.</param>
    /// <exception cref="NotImplementedException">Всегда выбрасывается, так как обратное преобразование не определено.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}