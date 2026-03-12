using System.Reactive;
using System.Drawing;
using ReactiveUI;
using graphic_editor.Models;
using graphic_editor.Geometry;

namespace graphic_editor.ViewModels;

/// <summary>Группа команд редактора для удобной привязки в XAML</summary>
public record EditorCommands(
    // Фигуры
    ReactiveCommand<Unit, Unit> AddSquare, /// <summary>Reactive UI добавление квадрата.</summary>
    ReactiveCommand<Unit, Unit> AddCircle, /// <summary>Reactive UI добавление круга.</summary>
    ReactiveCommand<Unit, Unit> AddRectangle, /// <summary>Reactive UI добавление прямоугольника.</summary>
    ReactiveCommand<Unit, Unit> AddEllipse, /// <summary>Reactive UI добавление эллипса.</summary>
    ReactiveCommand<Unit, Unit> AddLine, /// <summary>Reactive UI добавление линии.</summary>
    ReactiveCommand<Unit, Unit> AddPentagon, /// <summary>Reactive UI добавление пятиугольника.</summary>
    ReactiveCommand<Unit, Unit> AddHexagon, /// <summary>Reactive UI добавление шестиугольника.</summary>
    ReactiveCommand<Unit, Unit> AddOctagon, /// <summary>Reactive UI добавление восьмиугольника.</summary>
    ReactiveCommand<Unit, Unit> AddHeptagon, /// <summary>Reactive UI добавление семиугольника.</summary>
    ReactiveCommand<Unit, Unit> AddPentagram, /// <summary>Reactive UI добавление пентаграммы.</summary>
    ReactiveCommand<Unit, Unit> AddTriangle, /// <summary>Reactive UI добавление треугольника.</summary>

    // Выделение
    ReactiveCommand<Unit, Unit> DeleteSelected, /// <summary>Reactive UI удаление выбранной фигуры.</summary>
    ReactiveCommand<Unit, Unit> DuplicateSelected, /// <summary>Reactive UI дубликация выбранной фигуры.</summary>
    
    // Трансформации
    ReactiveCommand<Unit, Unit> RotateLeft, /// <summary>Reactive UI вращение фигуры влево.</summary>
    ReactiveCommand<Unit, Unit> RotateRight, /// <summary>Reactive UI вращение фигуры вправо.</summary>
    ReactiveCommand<Unit, Unit> RotateFull, /// <summary>Reactive UI полное вращение фигуры.</summary>
    ReactiveCommand<Unit, Unit> RotateFreeClick, /// <summary>Reactive UI полное вращение фигуры.</summary>
    ReactiveCommand<Unit, Unit> FlipHorizontal, /// <summary>Reactive UI отражение фигуры по горизонтали.</summary>
    ReactiveCommand<Unit, Unit> FlipVertical, /// <summary>Reactive UI отражение фигуры по вертикали.</summary>

    // Zoom
    ReactiveCommand<Unit, Unit> ZoomIn, /// <summary>Reactive UI приближение зума.</summary>
    ReactiveCommand<Unit, Unit> ZoomOut, /// <summary>Reactive UI удаление зума.</summary>
    ReactiveCommand<Unit, Unit> ZoomFit, /// <summary>Reactive UI нормализация зума.</summary>

    // Файл
    ReactiveCommand<Unit, Unit> Save, /// <summary>Reactive UI сохранение файла.</summary>
    ReactiveCommand<Unit, Unit> Open, /// <summary>Reactive UI открытие файла.</summary>
    ReactiveCommand<Unit, Unit> Export, /// <summary>Reactive UI экспорт файла.</summary>

    // UI
    ReactiveCommand<Unit, Unit> ToggleTheme, /// <summary>Reactive UI переключение темы.</summary>
    ReactiveCommand<Unit, Unit> CreateNewLayer, /// <summary>Reactive UI создание нового слоя.</summary>
    
    // Group
    ReactiveCommand<Unit, Unit> GroupSelected, /// <summary>Reactive UI создание группы.</summary>
    ReactiveCommand<Unit, Unit> UngroupSelected, /// <summary>Reactive UI удаление группы.</summary>
    
    // Move
    ReactiveCommand<Unit, Unit> MoveUp, /// <summary>Reactive UI перемещение вверх.</summary>
    ReactiveCommand<Unit, Unit> MoveDown, /// <summary>Reactive UI перемещение вниз.</summary>
    ReactiveCommand<Unit, Unit> MoveLeft, /// <summary>Reactive UI перемещение влево.</summary>
    ReactiveCommand<Unit, Unit> MoveRight, /// <summary>Reactive UI перемещение вправо.</summary>

    // Координаты/канвас
    ReactiveCommand<(double x, double y), Unit> UpdateCoordinates, /// <summary>Reactive UI обновление координат.</summary>
    ReactiveCommand<Point2D, Unit> CanvasClicked, /// <summary>Reactive UI реакция канваса на клик.</summary>

    ReactiveCommand<Unit, Unit> SaveCommand, /// <summary>Reactive UI команда сохранения.</summary>
    ReactiveCommand<Avalonia.Media.Color, Unit> SetStrokeColorCommand, /// <summary>Reactive UI команда установка толщины.</summary>
    ReactiveCommand<Avalonia.Media.Color, Unit> SetFillColorCommand, /// <summary>Reactive UI команда установка заполнения.</summary>
    ReactiveCommand<Unit, Unit> OpenFillColorPickerCommand, /// <summary>Reactive UI команда открытия палитры цвета заполнения.</summary>
    ReactiveCommand<Unit, Unit> OpenStrokeColorPickerCommand /// <summary>Reactive UI команда открытия палитры цвета линии.</summary>
    
    // Pointer-команды можно добавить позже, когда вынесем через Behavior
    // ReactiveCommand<PointerData, Unit> CanvasPointerPressed,
    // ReactiveCommand<PointerData, Unit> CanvasPointerMoved,
    // ReactiveCommand<PointerData, Unit> CanvasPointerReleased
);