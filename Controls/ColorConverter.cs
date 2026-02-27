// // Helpers/ColorConverter.cs
// using Avalonia.Data.Converters;
// using Avalonia.Media;
// using System.Globalization;
//
// public class DrawingColorConverter : IValueConverter
// {
//     public static readonly DrawingColorConverter Instance = new();
//     
//     public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         if (value is System.Drawing.Color c)
//             return Color.FromArgb(c.A, c.R, c.G, c.B);
//         return Colors.Black;
//     }
//     
//     public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         if (value is Avalonia.Media.Color c)
//             return Color.FromArgb(c.A, c.R, c.G, c.B);
//         return Color.Black;
//     }
// }