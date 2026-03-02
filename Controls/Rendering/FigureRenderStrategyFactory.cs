// // Controls/Rendering/FigureRenderStrategyFactory.cs
//
// using graphic_editor.Controls;
// using graphic_editor.ViewModels;
// namespace graphic_editor.Controls.Rendering;
//
// public class FigureRenderStrategyFactory
// {
//     private readonly Dictionary<Type, IFigureRenderStrategy> _strategies = new();
//     
//     public FigureRenderStrategyFactory()
//     {
//         // Регистрация стратегий при старте
//         Register(new RectangleRenderStrategy());
//         Register(new EllipseRenderStrategy());
//         Register(new LineRenderStrategy());
//         Register(new PolygonRenderStrategy());
//         Register(new PenPointRenderStrategy());
//         // ... остальные
//     }
//     
//     public void Register(IFigureRenderStrategy strategy) =>
//         _strategies[strategy.SupportedFigureType] = strategy;
//     
//     public Control? CreateControl(FigureViewModel figure) =>
//         _strategies.TryGetValue(figure.GetType(), out var strategy)
//             ? strategy.CreateControl(figure)
//             : null;
//     
//     public bool IsSupported(Type figureType) => 
//         _strategies.ContainsKey(figureType);
// }