// // Controls/Rendering/RectangleRenderStrategy.cs
// using Avalonia.Controls;
// using Avalonia.Controls.Shapes;
// using graphic_editor.Controls;
// using graphic_editor.ViewModels;
//
// namespace graphic_editor.Controls.Rendering;
//
// public class RectangleRenderStrategy : IFigureRenderStrategy
// {
//     public Type SupportedFigureType => typeof(RectangleViewModel);
//     
//     public Control? CreateControl(FigureViewModel figure)
//     {
//         if (figure is not RectangleViewModel rect) return null;
//         
//         return new Rectangle
//         {
//             Width = Math.Abs(rect.Width),
//             Height = Math.Abs(rect.Height),
//             [Canvas.LeftProperty] = Math.Min(rect.X, rect.X + rect.Width),
//             [Canvas.TopProperty] = Math.Min(rect.Y, rect.Y + rect.Height),
//             Tag = figure // Важно для hit-testing и привязок
//         };
//     }
// }