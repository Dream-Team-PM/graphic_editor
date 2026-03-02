// // Controls/Rendering/PolygonRenderStrategy.cs
// using Avalonia.Controls;
// using Avalonia.Controls.Shapes;
// using Avalonia.Media;
// using graphic_editor.Controls;
// using graphic_editor.ViewModels;
//
// namespace graphic_editor.Controls.Rendering;
//
// public class PolygonRenderStrategy : IFigureRenderStrategy
// {
//     public Type SupportedFigureType => typeof(PolygonViewModel);
//     
//     public Control? CreateControl(FigureViewModel figure)
//     {
//         if (figure is not PolygonViewModel polygon) return null;
//         
//         var geometry = new StreamGeometry();
//         using var ctx = geometry.Open();
//         
//         if (polygon.Vertices.Count == 0) return null;
//         
//         ctx.BeginFigure(
//             new Avalonia.Point(polygon.Vertices[0].X, polygon.Vertices[0].Y),
//             isFilled: polygon.FillColor.A > 0);
//         
//         for (int i = 1; i < polygon.Vertices.Count; i++)
//             ctx.LineTo(new Avalonia.Point(polygon.Vertices[i].X, polygon.Vertices[i].Y));
//         
//         ctx.EndFigure(isClosed: true);
//         
//         return new Path
//         {
//             Data = geometry,
//             StrokeThickness = Math.Max(1, polygon.Thickness),
//             Tag = figure
//         };
//     }
// }