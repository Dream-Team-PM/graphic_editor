using System.Reactive;
using System.Drawing;
using ReactiveUI;
using graphic_editor.Models;
using graphic_editor.Geometry;

namespace graphic_editor.ViewModels;

/// <summary>
/// Группа команд редактора для удобной привязки действий UI к методам ViewModel через ReactiveUI.
/// Содержит все доступные операции: создание фигур, трансформации, работа со слоями, файлами и стилями.
/// </summary>
public record EditorCommands(
    // === Фигуры: добавление примитивов ===
    
    /// <summary>ReactiveCommand для добавления квадрата на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddSquare,
    
    /// <summary>ReactiveCommand для добавления круга на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddCircle,
    
    /// <summary>ReactiveCommand для добавления прямоугольника на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddRectangle,
    
    /// <summary>ReactiveCommand для добавления эллипса на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddEllipse,
    
    /// <summary>ReactiveCommand для добавления линии на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddLine,
    
    /// <summary>ReactiveCommand для добавления правильного пятиугольника на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddPentagon,
    
    /// <summary>ReactiveCommand для добавления правильного шестиугольника на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddHexagon,
    
    /// <summary>ReactiveCommand для добавления правильного восьмиугольника на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddOctagon,
    
    /// <summary>ReactiveCommand для добавления правильного семиугольника на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddHeptagon,
    
    /// <summary>ReactiveCommand для добавления пентаграммы (пятиконечной звезды) на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddPentagram,
    
    /// <summary>ReactiveCommand для добавления треугольника по трём вершинам на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddTriangle,
    
    /// <summary>ReactiveCommand для добавления прямоугольного треугольника на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddRightTriangle,
    
    /// <summary>ReactiveCommand для добавления ромба на активный слой.</summary>
    ReactiveCommand<Unit, Unit> AddRhombus,
    
    // === Выделение и буфер обмена ===
    
    /// <summary>ReactiveCommand для удаления всех выделенных фигур с активного слоя.</summary>
    ReactiveCommand<Unit, Unit> DeleteSelected,
    
    /// <summary>ReactiveCommand для дублирования выделенной фигуры со смещением (10, 10).</summary>
    ReactiveCommand<Unit, Unit> DuplicateSelected,
    
    /// <summary>ReactiveCommand для вырезания выделенных фигур в буфер обмена.</summary>
    ReactiveCommand<Unit, Unit> CutSelected,
    
    /// <summary>ReactiveCommand для копирования выделенных фигур в буфер обмена.</summary>
    ReactiveCommand<Unit, Unit> CopySelected,
    
    /// <summary>ReactiveCommand для вставки фигур из буфера обмена со смещением.</summary>
    ReactiveCommand<Unit, Unit> PasteSelected,
    
    /// <summary>ReactiveCommand для выделения всех фигур на активном слое.</summary>
    ReactiveCommand<Unit, Unit> SelectAllCommand,
    
    /// <summary>ReactiveCommand для снятия выделения со всех фигур.</summary>
    ReactiveCommand<Unit, Unit> DeselectAllCommand,

    // === Порядок отрисовки (Z-order) ===
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур на передний план слоя.</summary>
    ReactiveCommand<Unit, Unit> BringToFront,
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур на задний план слоя.</summary>
    ReactiveCommand<Unit, Unit> SendToBack,
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур на один уровень вперёд.</summary>
    ReactiveCommand<Unit, Unit> BringForward,
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур на один уровень назад.</summary>
    ReactiveCommand<Unit, Unit> SendBackward,

    // === Выравнивание выделенных фигур ===
    
    /// <summary>ReactiveCommand для выравнивания выделенных фигур по левому краю.</summary>
    ReactiveCommand<Unit, Unit> AlignLeft,
    
    /// <summary>ReactiveCommand для выравнивания выделенных фигур по центру горизонтально.</summary>
    ReactiveCommand<Unit, Unit> AlignCenter,
    
    /// <summary>ReactiveCommand для выравнивания выделенных фигур по правому краю.</summary>
    ReactiveCommand<Unit, Unit> AlignRight,
    
    /// <summary>ReactiveCommand для выравнивания выделенных фигур по верхнему краю.</summary>
    ReactiveCommand<Unit, Unit> AlignTop,
    
    /// <summary>ReactiveCommand для выравнивания выделенных фигур по центру вертикально.</summary>
    ReactiveCommand<Unit, Unit> AlignMiddle,
    
    /// <summary>ReactiveCommand для выравнивания выделенных фигур по нижнему краю.</summary>
    ReactiveCommand<Unit, Unit> AlignBottom,

    // === Распределение выделенных фигур ===
    
    /// <summary>ReactiveCommand для равномерного распределения выделенных фигур по горизонтали.</summary>
    ReactiveCommand<Unit, Unit> DistributeHorizontal,
    
    /// <summary>ReactiveCommand для равномерного распределения выделенных фигур по вертикали.</summary>
    ReactiveCommand<Unit, Unit> DistributeVertical,
    
    // === Трансформации фигур ===
    
    /// <summary>ReactiveCommand для вращения выделенных фигур на -90° (против часовой стрелки).</summary>
    ReactiveCommand<Unit, Unit> RotateLeft,
    
    /// <summary>ReactiveCommand для вращения выделенных фигур на +90° (по часовой стрелке).</summary>
    ReactiveCommand<Unit, Unit> RotateRight,
    
    /// <summary>ReactiveCommand для вращения выделенных фигур на 180°.</summary>
    ReactiveCommand<Unit, Unit> RotateFull,
    
    /// <summary>ReactiveCommand для открытия диалога свободного вращения фигуры.</summary>
    ReactiveCommand<Unit, Unit> RotateFreeClick,
    
    /// <summary>ReactiveCommand для горизонтального отражения выделенных фигур.</summary>
    ReactiveCommand<Unit, Unit> FlipHorizontal,
    
    /// <summary>ReactiveCommand для вертикального отражения выделенных фигур.</summary>
    ReactiveCommand<Unit, Unit> FlipVertical,
    
    /// <summary>ReactiveCommand для увеличения масштаба выделенных фигур на 10%.</summary>
    ReactiveCommand<Unit, Unit> ScaleUp,
    
    /// <summary>ReactiveCommand для уменьшения масштаба выделенных фигур на 10%.</summary>
    ReactiveCommand<Unit, Unit> ScaleDown,
    
    /// <summary>ReactiveCommand для масштабирования выделенных фигур под размер видимой области.</summary>
    ReactiveCommand<Unit, Unit> ScaleToFit,

    // === Масштаб канваса (зум) ===
    
    /// <summary>ReactiveCommand для увеличения масштаба канваса в 1.5 раза (макс. 10x).</summary>
    ReactiveCommand<Unit, Unit> ZoomIn,
    
    /// <summary>ReactiveCommand для уменьшения масштаба канваса в 2 раза (мин. 0.1x).</summary>
    ReactiveCommand<Unit, Unit> ZoomOut,
    
    /// <summary>ReactiveCommand для сброса масштаба канваса к значению 1.0 (по размеру окна).</summary>
    ReactiveCommand<Unit, Unit> ZoomFit,

    // === Работа с файлами ===
    
    /// <summary>ReactiveCommand для сохранения текущего проекта в файл.</summary>
    ReactiveCommand<Unit, Unit> Save,
    
    /// <summary>ReactiveCommand для открытия диалога загрузки проекта из файла.</summary>
    ReactiveCommand<Unit, Unit> Open,
    
    /// <summary>ReactiveCommand для экспорта текущего вида канваса в изображение (PNG/JPEG/PDF).</summary>
    ReactiveCommand<Unit, Unit> Export,

    // === UI и настройки ===
    
    /// <summary>ReactiveCommand для переключения темы интерфейса между светлой и тёмной.</summary>
    ReactiveCommand<Unit, Unit> ToggleTheme,
    
    // === Группировка фигур ===
    
    /// <summary>ReactiveCommand для группировки выделенных фигур (минимум 2) в GroupViewModel.</summary>
    ReactiveCommand<Unit, Unit> GroupSelected,
    
    /// <summary>ReactiveCommand для разгруппировки выбранной группы на отдельные фигуры.</summary>
    ReactiveCommand<Unit, Unit> UngroupSelected,
    
    // === Перемещение фигур ===
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур вверх на 10 пикселей.</summary>
    ReactiveCommand<Unit, Unit> MoveUp,
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур вниз на 10 пикселей.</summary>
    ReactiveCommand<Unit, Unit> MoveDown,
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур влево на 10 пикселей.</summary>
    ReactiveCommand<Unit, Unit> MoveLeft,
    
    /// <summary>ReactiveCommand для перемещения выделенных фигур вправо на 10 пикселей.</summary>
    ReactiveCommand<Unit, Unit> MoveRight,

    // === Координаты и взаимодействие с канвасом ===
    
    /// <summary>ReactiveCommand для обновления отображаемых координат курсора мыши.</summary>
    /// <param name="x">Координата X курсора в координатах канваса.</param>
    /// <param name="y">Координата Y курсора в координатах канваса.</param>
    ReactiveCommand<(double x, double y), Unit> UpdateCoordinates,
    
    /// <summary>ReactiveCommand для обработки клика по канвасу (выбор фигуры или начало рисования).</summary>
    /// <param name="point">Точка клика в координатах канваса.</param>
    ReactiveCommand<Point2D, Unit> CanvasClicked,

    // === Управление слоями ===
    
    /// <summary>ReactiveCommand для создания нового слоя и установки его активным.</summary>
    ReactiveCommand<Unit, Unit> CreateNewLayer,
    
    /// <summary>ReactiveCommand для удаления указанного слоя (если он не последний).</summary>
    /// <param name="layer">Экземпляр LayerViewModel для удаления.</param>
    ReactiveCommand<LayerViewModel, Unit> DeleteLayerCommand,
    
    /// <summary>ReactiveCommand для переключения блокировки указанного слоя.</summary>
    /// <param name="layer">Экземпляр LayerViewModel для изменения состояния блокировки.</param>
    ReactiveCommand<LayerViewModel, Unit> ToggleLockLayerCommand,
    
    /// <summary>ReactiveCommand для переключения видимости указанного слоя.</summary>
    /// <param name="layer">Экземпляр LayerViewModel для изменения состояния видимости.</param>
    ReactiveCommand<LayerViewModel, Unit> ToggleVisibilityLayerCommand,
    
    /// <summary>ReactiveCommand для дублирования активного слоя с сохранением всех фигур.</summary>
    ReactiveCommand<Unit, Unit> DuplicateLayerCommand,
    
    /// <summary>ReactiveCommand для объединения активного слоя с предыдущим в списке.</summary>
    ReactiveCommand<Unit, Unit> MergeWithPreviousLayerCommand,
    
    /// <summary>ReactiveCommand для перемещения активного слоя на один уровень вверх.</summary>
    ReactiveCommand<Unit, Unit> BringLayerForwardCommand,
    
    /// <summary>ReactiveCommand для перемещения активного слоя на один уровень вниз.</summary>
    ReactiveCommand<Unit, Unit> SendLayerBackwardCommand,
    
    /// <summary>ReactiveCommand для перемещения активного слоя на самый передний план.</summary>
    ReactiveCommand<Unit, Unit> BringLayerToFrontCommand,
    
    /// <summary>ReactiveCommand для перемещения активного слоя на самый задний план.</summary>
    ReactiveCommand<Unit, Unit> SendLayerToBackCommand,

    // === Настройки стиля ===
    
    /// <summary>ReactiveCommand для установки толщины обводки выделенных фигур.</summary>
    /// <param name="widthStr">Строковое значение толщины в пикселях.</param>
    ReactiveCommand<string, Unit> SetStrokeWidthCommand,
    
    /// <summary>ReactiveCommand для отмены заливки выделенных фигур (прозрачный цвет).</summary>
    ReactiveCommand<Unit, Unit> SetFillNone,
    
    /// <summary>ReactiveCommand для отмены обводки выделенных фигур (прозрачный цвет).</summary>
    ReactiveCommand<Unit, Unit> SetStrokeNone,
    
    /// <summary>ReactiveCommand для асинхронного сохранения проекта в файл.</summary>
    ReactiveCommand<Unit, Unit> SaveCommand,
    
    /// <summary>ReactiveCommand для установки цвета обводки из Avalonia.Media.Color.</summary>
    /// <param name="color">Новый цвет обводки в формате Avalonia.Media.Color.</param>
    ReactiveCommand<Avalonia.Media.Color, Unit> SetStrokeColorCommand,
    
    /// <summary>ReactiveCommand для установки цвета заливки из Avalonia.Media.Color.</summary>
    /// <param name="color">Новый цвет заливки в формате Avalonia.Media.Color.</param>
    ReactiveCommand<Avalonia.Media.Color, Unit> SetFillColorCommand,
    
    /// <summary>ReactiveCommand для открытия палитры выбора цвета заливки.</summary>
    ReactiveCommand<Unit, Unit> OpenFillColorPickerCommand,
    
    /// <summary>ReactiveCommand для открытия палитры выбора цвета обводки.</summary>
    ReactiveCommand<Unit, Unit> OpenStrokeColorPickerCommand,
    
    /// <summary>ReactiveCommand для открытия панели свойств выделенного объекта.</summary>
    ReactiveCommand<Unit, Unit> OpenPropertiesCommand
);