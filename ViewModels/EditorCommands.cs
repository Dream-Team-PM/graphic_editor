using System.Reactive;
using System.Drawing;
using ReactiveUI;
using graphic_editor.Models;

namespace graphic_editor.ViewModels;

/// <summary>Группа команд редактора для удобной привязки в XAML</summary>
public record EditorCommands(
    // Фигуры
    ReactiveCommand<Unit, Unit> AddSquare,
    ReactiveCommand<Unit, Unit> AddCircle,
    ReactiveCommand<Unit, Unit> AddRectangle,
    ReactiveCommand<Unit, Unit> AddEllipse,
    ReactiveCommand<Unit, Unit> AddLine,

    // Выделение
    ReactiveCommand<Unit, Unit> DeleteSelected,
    ReactiveCommand<Unit, Unit> DuplicateSelected,

    // Трансформации
    ReactiveCommand<Unit, Unit> RotateLeft,
    ReactiveCommand<Unit, Unit> RotateRight,

    // Zoom
    ReactiveCommand<Unit, Unit> ZoomIn,
    ReactiveCommand<Unit, Unit> ZoomOut,
    ReactiveCommand<Unit, Unit> ZoomFit,

    // Файл
    ReactiveCommand<Unit, Unit> Save,
    ReactiveCommand<Unit, Unit> Open,
    ReactiveCommand<Unit, Unit> Export,

    // UI
    ReactiveCommand<Unit, Unit> ToggleTheme,
    ReactiveCommand<Unit, Unit> CreateNewLayer,

    // Координаты/канвас
    ReactiveCommand<(double x, double y), Unit> UpdateCoordinates,
    ReactiveCommand<Point_1, Unit> CanvasClicked,

    ReactiveCommand<Unit, Unit> SaveCommand,
    ReactiveCommand<Avalonia.Media.Color, Unit> SetStrokeColorCommand,
    ReactiveCommand<Avalonia.Media.Color, Unit> SetFillColorCommand,
    ReactiveCommand<Unit, Unit> OpenFillColorPickerCommand,
    ReactiveCommand<Unit, Unit> OpenStrokeColorPickerCommand
    
    // Pointer-команды можно добавить позже, когда вынесем через Behavior
    // ReactiveCommand<PointerData, Unit> CanvasPointerPressed,
    // ReactiveCommand<PointerData, Unit> CanvasPointerMoved,
    // ReactiveCommand<PointerData, Unit> CanvasPointerReleased
);