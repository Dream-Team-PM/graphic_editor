using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Controls.Primitives;
using ReactiveUI;
using graphic_editor.Models;
using graphic_editor.Geometry;
using graphic_editor.Helpers;
using graphic_editor.Interfaces;
using graphic_editor.State;
using graphic_editor.Commands;

namespace graphic_editor.ViewModels;

/// <summary>
/// ViewModel для главного окна графического редактора "Магический графический редактор".
/// Управляет состоянием UI, инструментами рисования, историей действий и взаимодействием с канвасом.
/// Реализует паттерн MVVM с использованием ReactiveUI для реактивных привязок.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFileService _fileService; /// <summary>Сервис для работы с файлами проекта (сохранение/загрузка).</summary>
    private readonly HistoryViewModel _history; /// <summary>Менеджер истории действий для поддержки Undo/Redo.</summary>
	private readonly LayerViewModel _layer; /// <summary>Менеджер истории действий для поддержки Undo/Redo.</summary>
    /// <summary>Флаг, указывающий, выполняется ли в данный момент перетаскивание выделенных фигур.</summary>
    private bool _isDragging;
    
    /// <summary>Начальная точка перетаскивания для вычисления дельты смещения.</summary>
    private Point2D _dragStart;
    /// <summary>Словарь исходных координат вершин для каждой фигуры при начале перетаскивания (для Undo).</summary>
    private Dictionary<Guid, List<(double X, double Y)>> _originalVertices;
    
    /// <summary>
    /// Публичный доступ к ViewModel истории для привязки в UI.
    /// </summary>
    public HistoryViewModel History => _history;
	public LayerViewModel Layer => _layer;

    // ========== ПОЛЯ ==========
    /// <summary>Реактивный хелпер для форматирования и обновления текста координат курсора "X: .. Y: ..".</summary>
    private readonly ObservableAsPropertyHelper<string> _coordinatesText;
    
    /// <summary>Текст сообщения статуса для отображения в нижней панели окна (информация о действиях пользователя).</summary>
    private string _statusMessage = "Готово";
    
    /// <summary>Текущий выбранный инструмент рисования из перечисления DrawingTool.</summary>
    private DrawingTool _selectedTool = DrawingTool.Select;
    
    /// <summary>Текущий выбранный цвет заливки для новых фигур с поддержкой реактивных уведомлений.</summary>
    private ColorViewModel _fillColor = new ColorViewModel(System.Drawing.Color.FromArgb(255, 74, 144));
    
    /// <summary>Текущий выбранный цвет обводки для новых фигур с поддержкой реактивных уведомлений.</summary>
    private ColorViewModel _strokeColor = new ColorViewModel(System.Drawing.Color.Black);
    
    /// <summary>Текущая тема оформления интерфейса (светлая ThemeVariant.Light или тёмная ThemeVariant.Dark).</summary>
    private ThemeVariant _currentTheme = ThemeVariant.Dark;
    
    /// <summary>Точка начала операции рисования (координаты при нажатии кнопки мыши на канвасе).</summary>
    private Point2D _drawingStartPoint;
    
    /// <summary>Флаг, указывающий, была ли инициализирована точка начала рисования для текущего примитива.</summary>
    private bool _hasDrawingStart;
    
    /// <summary>Предварительная фигура для визуализации в процессе рисования (удаляется при завершении).</summary>
    private FigureViewModel? _previewFigure;
    
    /// <summary>Инструмент, который в данный момент используется для рисования (может отличаться от _selectedTool).</summary>
    private DrawingTool _currentDrawingTool;
    
    /// <summary>Коллекция точек для инструмента "Перо" при многоточечном рисовании кривой.</summary>
    private List<Point2D> _penPoints = new();
    
    /// <summary>Минимальный допустимый размер фигуры (в пикселях) для предотвращения создания микро-объектов.</summary>
    private const double MinFigureSize = 5.0;
    
    /// <summary>Минимально допустимый коэффициент масштабирования канваса (0.1 = 10% от оригинала).</summary>
    private const double DefaultZoomMin = 0.1;
    
    /// <summary>Максимально допустимый коэффициент масштабирования канваса (10.0 = 1000% от оригинала).</summary>
    private const double DefaultZoomMax = 10.0;
    
    /// <summary>Флаг, указывающий, что пользователь выполняет выделение областью (marquee selection прямоугольником).</summary>
    private bool _isSelectingArea;
    
    /// <summary>Начальная точка выделения областью в координатах канваса.</summary>
    private Point2D _selectionStart;

    /// <summary>Текущая конечная точка выделения областью (обновляется при движении мыши).</summary>
    private Point2D _selectionEnd;
    
    /// <summary>Флаг, указывающий, открыта ли палитра выбора цвета для заливки фигур.</summary>
    private bool _isColorPickerOpen;
    
    /// <summary>Флаг, указывающий, открыта ли палитра выбора цвета для обводки фигур.</summary>
    private bool _isStrokeColorPickerOpen;
    
    /// <summary>Буфер обмена для операций копирования/вставки фигур.</summary>
    private List<FigureViewModel> _clipboard = new();

    // ========== СВОЙСТВА ==========
    
    /// <summary>
    /// Получает человеко-читаемое название выбранного инструмента.
    /// </summary>
    public string SelectedToolDisplayName => _selectedTool.ToDisplayName();
    
    /// <summary>
    /// Получает текущий активный инструмент рисования.
    /// </summary>
    public DrawingTool CurrentTool => _selectedTool;
    
    /// <summary>
    /// Коллекция ReactiveCommand для привязки действий UI к методам ViewModel.
    /// </summary>
    public EditorCommands Commands { get; }
    
    /// <summary>
    /// ViewModel канваса, управляющая слоями, фигурами и их состоянием.
    /// </summary>
    public CanvasViewModel Canvas { get; }
    
    /// <summary>
    /// Форматированный текст координат курсора для отображения в UI.
    /// </summary>
    public string CoordinatesText => _coordinatesText.Value;
    
    /// <summary>
    /// Проверяет, есть ли выделенные фигуры на канвасе.
    /// </summary>
    public bool HasSelection => Canvas?.HasSelection ?? false;
    
    /// <summary>
    /// Проверяет, возможно ли выполнение группировки (требуется минимум 2 выделенные фигуры).
    /// </summary>
    public bool CanGroup => Canvas?.SelectedFigures?.Count >= 2;
    
    /// <summary>
    /// Проверяет, возможно ли выполнение разгруппировки (выбрана фигура типа GroupViewModel).
    /// </summary>
    public bool CanUngroup => Canvas?.SelectedFigure is GroupViewModel;
    
    /// <summary>
    /// Проверяет, возможно ли выравнивание (требуется минимум 2 выделенные фигуры).
    /// </summary>
    public bool CanAlign => Canvas?.SelectedFigures?.Count >= 2;
    
    /// <summary>
    /// Проверяет, возможно ли распределение (требуется минимум 3 выделенные фигуры).
    /// </summary>
    public bool CanDistribute => Canvas?.SelectedFigures?.Count >= 3;
    
    /// <summary>
    /// Текст сообщения статуса для отображения пользователю в нижней панели окна.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, выполняется ли в данный момент выделение областью (marquee selection).
    /// Используется для отображения прямоугольника выделения на канвасе.
    /// </summary>
    public bool IsSelectingArea
    {
        get => _isSelectingArea;
        set => this.RaiseAndSetIfChanged(ref _isSelectingArea, value);
    }
    
    /// <summary>
    /// Начальная точка выделения областью в координатах канваса.
    /// Устанавливается при нажатии кнопки мыши и используется для вычисления прямоугольника выделения.
    /// </summary>
    public Point2D SelectionStart
    {
        get => _selectionStart;
        set => this.RaiseAndSetIfChanged(ref _selectionStart, value);
    }
    
    
    /// <summary>
    /// Текущая конечная точка выделения областью (обновляется при движении мыши).
    /// Используется для динамического отображения прямоугольника выделения.
    /// </summary>
    public Point2D SelectionEnd
    {
        get => _selectionEnd;
        set => this.RaiseAndSetIfChanged(ref _selectionEnd, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, что пользователь находится в процессе рисования фигуры.
    /// Блокирует другие действия до завершения операции (отпускание кнопки мыши).
    /// </summary>
    public bool IsDrawing
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Предварительная фигура для визуального отображения в процессе рисования.
    /// Удаляется при завершении рисования и замене на финальную фигуру.
    /// </summary>
    public FigureViewModel? PreviewFigure
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Толщина линии обводки в пикселях для новых и выделенных фигур.
    /// Применяется реактивно ко всем выделенным объектам при изменении.
    /// </summary>
    public int StrokeWidth 
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Коэффициент непрозрачности фигур в процентах (0–100).
    /// Конвертируется в диапазон 0.0–1.0 при применении к фигурам.
    /// </summary>
    public double Opacity
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Модель цвета заливки с поддержкой реактивных уведомлений.
    /// Используется для новых фигур и реактивного обновления выделенных.
    /// </summary>
    public ColorViewModel FillColor
    {
        get => _fillColor;
        set => this.RaiseAndSetIfChanged(ref _fillColor, value);
    }
    
    /// <summary>
    /// Модель цвета обводки с поддержкой реактивных уведомлений.
    /// Используется для новых фигур и реактивного обновления выделенных.
    /// </summary>
    public ColorViewModel StrokeColor
    {
        get => _strokeColor;
        set => this.RaiseAndSetIfChanged(ref _strokeColor, value);
    }
    
    /// <summary>
    /// Текущая тема оформления приложения (светлая или тёмная).
    /// Используется для переключения стилей через RequestedThemeVariant.
    /// </summary>
    public ThemeVariant CurrentTheme
    {
        get => _currentTheme;
        set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
    }
    
    /// <summary>
    /// Текущая координата X курсора мыши в координатах канваса.
    /// Обновляется при движении мыши и используется для отображения в статус-баре.
    /// </summary>
    public double MouseX
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Текущая координата Y курсора мыши в координатах канваса.
    /// Обновляется при движении мыши и используется для отображения в статус-баре.
    /// </summary>
    public double MouseY
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, открыта ли палитра выбора цвета заливки.
    /// Привязан к свойству IsOpen Popup в ToolSettingsBar.
    /// </summary>
    public bool IsColorPickerOpen
    {
        get => _isColorPickerOpen;
        set => this.RaiseAndSetIfChanged(ref _isColorPickerOpen, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, открыта ли палитра выбора цвета обводки.
    /// Привязан к свойству IsOpen Popup в ToolSettingsBar.
    /// </summary>
    public bool IsStrokeColorPickerOpen
    {
        get => _isStrokeColorPickerOpen;
        set => this.RaiseAndSetIfChanged(ref _isStrokeColorPickerOpen, value);
    }

    /// <summary>
    /// Применяет текущие настройки стиля (цвета, толщина, прозрачность) к заданной фигуре.
    /// </summary>
    /// <typeparam name="T">Тип фигуры, наследующий FigureViewModel.</typeparam>
    /// <param name="figure">Экземпляр фигуры для применения стиля.</param>
    /// <param name="solidFill">Если true, использует цвет обводки для заливки (для инструмента Pen).</param>
    private void ApplyStyle<T>(T figure, bool solidFill = false) where T : FigureViewModel
    {
        figure.LineColor = StrokeColor.Color;
        figure.FillColor = solidFill ? StrokeColor.Color : FillColor.Color;
        figure.Thickness = StrokeWidth;
    }
    
    /// <summary>
    /// Команда истории для операции перетаскивания фигур.
    /// Реализует интерфейс IHistoryAction для поддержки Undo/Redo.
    /// Сохраняет исходные координаты вершин и дельту смещения для отмены/повтора.
    /// </summary>
    public class DragMoveCommand : IHistoryAction
    {
        /// <summary>Список идентификаторов перемещаемых фигур.</summary>
        private readonly List<Guid> _figureIds;
        
        /// <summary>Словарь исходных координат вершин для каждой фигуры (для Undo).</summary>
        private readonly Dictionary<Guid, List<(double X, double Y)>> _originalVertices;
        
        /// <summary>Вектор смещения, применённый к фигурам.</summary>
        private readonly Point2D _delta;
        
        /// <summary>Ссылка на CanvasViewModel для доступа к коллекциям фигур.</summary>
        private CanvasViewModel? _canvas;

        /// <summary>
        /// Получает человеко-читаемое описание команды для отображения в истории.
        /// </summary>
        public string Description => "Перемещение";

        /// <summary>
        /// Инициализирует новый экземпляр команды перемещения.
        /// </summary>
        /// <param name="figureIds">Список идентификаторов перемещаемых фигур.</param>
        /// <param name="originalVertices">Словарь исходных координат вершин для каждой фигуры.</param>
        /// <param name="delta">Вектор смещения, применённый к фигурам.</param>
        public DragMoveCommand(List<Guid> figureIds, Dictionary<Guid, List<(double X, double Y)>> originalVertices, Point2D delta)
        {
            _figureIds = figureIds;
            _originalVertices = originalVertices;
            _delta = delta;
        }

        /// <summary>
        /// Устанавливает ссылку на CanvasViewModel для выполнения команды.
        /// Вызывается после добавления команды в историю.
        /// </summary>
        /// <param name="canvas">Экземпляр CanvasViewModel для доступа к данным.</param>
        public void SetCanvas(CanvasViewModel canvas) => _canvas = canvas;

        /// <summary>
        /// Отменяет операцию перемещения: восстанавливает исходные координаты вершин.
        /// Использует сохранённые данные из _originalVertices для каждой фигуры.
        /// </summary>
        public void Undo()
        {
            if (_canvas == null) return;
            foreach (var id in _figureIds)
            {
                var figure = FindFigure(id);
                if (figure != null && _originalVertices.TryGetValue(id, out var verts))
                {
                    for (int i = 0; i < figure.Vertices.Count && i < verts.Count; i++)
                    {
                        figure.Vertices[i].X = verts[i].X;
                        figure.Vertices[i].Y = verts[i].Y;
                    }
                }
            }
        }
        
        /// <summary>
        /// Повторяет операцию перемещения: применяет сохранённое смещение к исходным координатам.
        /// Использует данные из _originalVertices и _delta для восстановления состояния после Undo.
        /// </summary>
        public void Redo()
        {
            if (_canvas == null) return;
            foreach (var id in _figureIds)
            {
                var figure = FindFigure(id);
                if (figure != null && _originalVertices.TryGetValue(id, out var verts))
                {
                    for (int i = 0; i < figure.Vertices.Count && i < verts.Count; i++)
                    {
                        figure.Vertices[i].X = verts[i].X + _delta.X;
                        figure.Vertices[i].Y = verts[i].Y + _delta.Y;
                    }
                }
            }
        }

        /// <summary>
        /// Находит фигуру по идентификатору во всех слоях канваса.
        /// </summary>
        /// <param name="id">Уникальный идентификатор фигуры типа Guid.</param>
        /// <returns>Экземпляр FigureViewModel или null, если фигура не найдена.</returns>
        private FigureViewModel? FindFigure(Guid id) =>
            _canvas?.Layers.SelectMany(l => l.Figures).FirstOrDefault(f => f.Id == id);
    }

    // ========== КОНСТРУКТОР ==========
    
    /// <summary>
    /// Инициализирует новый экземпляр MainWindowViewModel.
    /// Создаёт зависимости, инициализирует команды ReactiveUI и настраивает реактивные привязки.
    /// </summary>
    /// <param name="fileService">Сервис для работы с файлами проекта.</param>
    /// <param name="history">Экземпляр HistoryViewModel для управления Undo/Redo.</param>
    public MainWindowViewModel(
        IFileService fileService,
        HistoryViewModel history
    )
    {
        _fileService = fileService;
        _history = history;
        Canvas = new CanvasViewModel();
        Canvas.History = _history;
        _history.SetCanvas(Canvas);
        SetTool(DrawingTool.Select);
        
        // Инициализация ReactiveCommand для всех действий редактора
        Commands = new EditorCommands(
            AddCircle: ReactiveCommand.Create(AddCircle),
            AddSquare: ReactiveCommand.Create(AddSquare),
            AddRectangle: ReactiveCommand.Create(AddRectangle),
            AddEllipse: ReactiveCommand.Create(AddEllipse),
            AddLine: ReactiveCommand.Create(AddLine),
            AddPentagon: ReactiveCommand.Create(AddPentagon),
            AddHexagon: ReactiveCommand.Create(AddHexagon),
            AddOctagon: ReactiveCommand.Create(AddOctagon),
            AddHeptagon: ReactiveCommand.Create(AddHeptagon),
            AddPentagram: ReactiveCommand.Create(AddPentagram),
            AddTriangle: ReactiveCommand.Create(AddTriangle),
            AddRightTriangle: ReactiveCommand.Create(AddRightTriangle),
            AddRhombus: ReactiveCommand.Create(AddRhombus),
            DeleteSelected: ReactiveCommand.Create(DeleteSelected),
            DuplicateSelected: ReactiveCommand.Create(DuplicateSelected),
            RotateLeft: ReactiveCommand.Create(RotateLeft),
            RotateRight: ReactiveCommand.Create(RotateRight),
            RotateFull: ReactiveCommand.Create(RotateFull),
            RotateFreeClick: ReactiveCommand.Create(RotateFreeClick),
            ZoomIn: ReactiveCommand.Create(ZoomIn),
            ZoomOut: ReactiveCommand.Create(ZoomOut),
            ZoomFit: ReactiveCommand.Create(ZoomFit),
            FlipHorizontal: ReactiveCommand.Create(FlipHorizontal),
            FlipVertical: ReactiveCommand.Create(FlipVertical),
            ToggleTheme: ReactiveCommand.Create(ToggleTheme),
            Save: ReactiveCommand.Create(Save),
            Open: ReactiveCommand.Create(Open),
            Export: ReactiveCommand.Create(Export),
            CreateNewLayer: ReactiveCommand.Create(CreateNewLayer),
            UpdateCoordinates: ReactiveCommand.Create<(double x, double y)>(UpdateCoordinates),
            CanvasClicked: ReactiveCommand.Create<Point2D>(CanvasClicked),
            SaveCommand: ReactiveCommand.CreateFromTask(SaveAsync),
            GroupSelected: ReactiveCommand.Create(GroupSelected),
            UngroupSelected: ReactiveCommand.Create(UngroupSelected),
            MoveUp: ReactiveCommand.Create(() => MoveSelected(0, -10)),
            MoveDown: ReactiveCommand.Create(() => MoveSelected(0, 10)),
            MoveLeft: ReactiveCommand.Create(() => MoveSelected(-10, 0)),
            MoveRight: ReactiveCommand.Create(() => MoveSelected(10, 0)),
            SetStrokeColorCommand: ReactiveCommand.Create<Avalonia.Media.Color>(c => StrokeColor.Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B)),
            SetFillColorCommand: ReactiveCommand.Create<Avalonia.Media.Color>(c => FillColor.Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B)),
            OpenFillColorPickerCommand: ReactiveCommand.Create(() => { IsColorPickerOpen = true; }),
            OpenStrokeColorPickerCommand: ReactiveCommand.Create(() => { IsStrokeColorPickerOpen = true; }),
			DeleteLayerCommand: ReactiveCommand.Create<LayerViewModel>(DeleteLayer),
			ToggleLockLayerCommand: ReactiveCommand.Create<LayerViewModel>(ToggleLockLayer),
			ToggleVisibilityLayerCommand: ReactiveCommand.Create<LayerViewModel>(ToggleVisibilityLayer),
			DuplicateLayerCommand: ReactiveCommand.Create(DuplicateLayer),
    		MergeWithPreviousLayerCommand: ReactiveCommand.Create(MergeWithPreviousLayer),
    		BringLayerForwardCommand: ReactiveCommand.Create(BringLayerForward),
    		SendLayerBackwardCommand: ReactiveCommand.Create(SendLayerBackward),
    		BringLayerToFrontCommand: ReactiveCommand.Create(BringLayerToFront),
    		SendLayerToBackCommand: ReactiveCommand.Create(SendLayerToBack),
            // Выделение
            CutSelected: ReactiveCommand.Create(CutSelected, this.WhenAnyValue(x => x.HasSelection)),
            CopySelected: ReactiveCommand.Create(CopySelected, this.WhenAnyValue(x => x.HasSelection)),
            PasteSelected: ReactiveCommand.Create(PasteSelected),
            SelectAllCommand: ReactiveCommand.Create(SelectAll),
            DeselectAllCommand: ReactiveCommand.Create(DeselectAll),
    
            // Порядок (Z-order)
            BringToFront: ReactiveCommand.Create(BringSelectedToFront, this.WhenAnyValue(x => x.HasSelection)),
            SendToBack: ReactiveCommand.Create(SendSelectedToBack, this.WhenAnyValue(x => x.HasSelection)),
            BringForward: ReactiveCommand.Create(BringSelectedForward, this.WhenAnyValue(x => x.HasSelection)),
            SendBackward: ReactiveCommand.Create(SendSelectedBackward, this.WhenAnyValue(x => x.HasSelection)),
    
            // Выравнивание
            AlignLeft: ReactiveCommand.Create(AlignLeft, this.WhenAnyValue(x => x.CanAlign)),
            AlignCenter: ReactiveCommand.Create(AlignCenter, this.WhenAnyValue(x => x.CanAlign)),
            AlignRight: ReactiveCommand.Create(AlignRight, this.WhenAnyValue(x => x.CanAlign)),
            AlignTop: ReactiveCommand.Create(AlignTop, this.WhenAnyValue(x => x.CanAlign)),
            AlignMiddle: ReactiveCommand.Create(AlignMiddle, this.WhenAnyValue(x => x.CanAlign)),
            AlignBottom: ReactiveCommand.Create(AlignBottom, this.WhenAnyValue(x => x.CanAlign)),
    
            // Распределение
            DistributeHorizontal: ReactiveCommand.Create(DistributeHorizontal, this.WhenAnyValue(x => x.CanDistribute)),
            DistributeVertical: ReactiveCommand.Create(DistributeVertical, this.WhenAnyValue(x => x.CanDistribute)),
    
            // Масштаб фигур
            ScaleUp: ReactiveCommand.Create(ScaleSelectedUp, this.WhenAnyValue(x => x.HasSelection)),
            ScaleDown: ReactiveCommand.Create(ScaleSelectedDown, this.WhenAnyValue(x => x.HasSelection)),
            ScaleToFit: ReactiveCommand.Create(ScaleSelectedToFit, this.WhenAnyValue(x => x.HasSelection)),
    
            // Стиль
            SetStrokeWidthCommand: ReactiveCommand.Create<string>(SetStrokeWidth, this.WhenAnyValue(x => x.HasSelection)),
            SetFillNone: ReactiveCommand.Create(SetFillNone, this.WhenAnyValue(x => x.HasSelection)),
            SetStrokeNone: ReactiveCommand.Create(SetStrokeNone, this.WhenAnyValue(x => x.HasSelection)),
    
            // Свойства
            OpenPropertiesCommand: ReactiveCommand.Create(OpenProperties, this.WhenAnyValue(x => x.HasSelection))
        );
        
        // Реактивная привязка: обновление текста координат при изменении MouseX/MouseY
        _coordinatesText = this
            .WhenAnyValue(x => x.MouseX, x => x.MouseY)
            .Select(_ => $"X: {MouseX:F1}  Y: {MouseY:F1}")
            .ToProperty(this, x => x.CoordinatesText);
        
        // Подписка на изменение выбранной фигуры для обновления HasSelection
        this.WhenAnyValue(x => x.Canvas.SelectedFigure)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelection)));
        
        // Реактивное применение стиля к выделенным фигурам при изменении параметров
        this.WhenAnyValue(x => x.StrokeColor.Color)
            .Subscribe(color => ApplyStyleToSelected(f => f.LineColor = color));
        this.WhenAnyValue(x => x.FillColor.Color)
            .Subscribe(color => ApplyStyleToSelected(f => f.FillColor = color));
        this.WhenAnyValue(x => x.StrokeWidth)
            .Subscribe(thickness => ApplyStyleToSelected(f => f.Thickness = thickness));
        this.WhenAnyValue(x => x.Opacity)
            .Subscribe(opacity => ApplyStyleToSelected(f => f.Opacity = opacity / 100.0));
        this.WhenAnyValue(x => x.Canvas.SelectedFigures)
        .Subscribe(_ => 
        {
            this.RaisePropertyChanged(nameof(CanGroup));
            this.RaisePropertyChanged(nameof(CanUngroup));
            this.RaisePropertyChanged(nameof(CanAlign));
            this.RaisePropertyChanged(nameof(CanDistribute));
        });
    }

    /// <summary>
    /// Создаёт модель проекта из текущего состояния редактора для сохранения.
    /// </summary>
    /// <returns>Экземпляр Project с данными слоёв и настройками канваса.</returns>
    private Project CreateProject()
    {
        return new Project
        {
            Name = "Безымянный",
            Layers = Canvas.Layers,
            CanvasZoom = Canvas.Zoom,
            CanvasOffsetX = Canvas.OffsetX,
            CanvasOffsetY = Canvas.OffsetY
        };
    }
    
    /// <summary>
    /// Получает текущие настройки стиля для создания новых фигур.
    /// </summary>
    /// <returns>Экземпляр StyleSettings с актуальными параметрами.</returns>
    private StyleSettings GetCurrentStyle() => new(
        StrokeColor.Color,
        FillColor.Color,
        StrokeWidth,
        Opacity / 100.0);

    /// <summary>
    /// Устанавливает инструмент рисования по строковому имени (из Tag кнопки UI).
    /// Использует DrawingToolExtensions.TryParse для преобразования строки в перечисление.
    /// </summary>
    /// <param name="toolName">Строковое название инструмента (например, "Перо", "Прямоугольник").</param>
    public void SetToolByName(string toolName)
    {
        if (DrawingToolExtensions.TryParse(toolName, out var tool))
        {
            SetTool(tool);
        }
    }
    
    /// <summary>
    /// Устанавливает активный инструмент рисования и сбрасывает состояние рисования при смене.
    /// Если было активное рисование другим инструментом — очищает предварительную фигуру.
    /// </summary>
    /// <param name="tool">Перечисление DrawingTool для установки.</param>
    public void SetTool(DrawingTool tool)
    {
        if (IsDrawing && _selectedTool != tool)
        {
            if (_selectedTool == DrawingTool.Pen && _previewFigure != null && Canvas?.ActiveLayer != null)
            {
                Canvas.ActiveLayer.Figures.Remove(_previewFigure);
            }
            ResetDrawingState();
        }
        _selectedTool = tool;

        if (Canvas != null)
        {
            Canvas.CurrentTool = tool;
        }

        this.RaisePropertyChanged(nameof(SelectedToolDisplayName));
        StatusMessage = $"Установлен инструмент: {SelectedToolDisplayName}";
    }

    /// <summary>
    /// Перемещает выделенные фигуры на заданный вектор и добавляет действие в историю для Undo.
    /// Для групп фигур извлекает все вложенные фигуры и перемещает их рекурсивно.
    /// </summary>
    /// <param name="dx">Смещение по оси X в координатах канваса.</param>
    /// <param name="dy">Смещение по оси Y в координатах канваса.</param>
    private void MoveSelected(double dx, double dy)
    {
        if (Canvas?.SelectedFigures?.Any() != true) return;
        var allFigureIds = new List<Guid>();
        foreach (var figure in Canvas.SelectedFigures)
        {
            if (figure is GroupViewModel group)
            {
                allFigureIds.AddRange(group.GetAllFigureIds());
            }
            else
            {
                allFigureIds.Add(figure.Id);
            }
        }
        var cmd = new MoveFigureCommand(
            Canvas.SelectedFigures.Select(f => f.Id).ToList(),
            dx, dy);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = $"Перемещено на ({dx}, {dy})";
    }

    /// <summary>
    /// Группирует выделенные фигуры в одну группу (GroupViewModel).
    /// Требует выделения минимум двух фигур, иначе выводит сообщение об ошибке.
    /// </summary>
    public void GroupSelected()
    {
        if (Canvas?.ActiveLayer == null) return;
        var activeLayer = Canvas.ActiveLayer;
        DebugLog.Write($"[DEBUG] GroupSelected: SelectedFigures.Count = {Canvas.SelectedFigures.Count}");
        foreach (var f in Canvas.SelectedFigures)
        {
            DebugLog.Write($"[DEBUG]   - {f.Name}, IsSelected={f.IsSelected}");
        }
        if (Canvas.SelectedFigures.Count < 2)
        {
            StatusMessage = "Выделите минимум 2 фигуры для группировки (Ctrl+Click)";
            DebugLog.Write("Выделите минимум 2 фигуры для группировки");
            return;
        }
        var figuresToGroup = Canvas.SelectedFigures.ToList();
        var group = new GroupViewModel(figuresToGroup);
        foreach (var figure in figuresToGroup)
        {
            activeLayer.Figures.Remove(figure);
        }
        activeLayer.Figures.Add(group);
        Canvas.SelectedFigures.Clear();
        Canvas.SelectedFigures.Add(group);
        Canvas.SelectedFigure = group;
        Dispatcher.UIThread.Post(() =>
        {
            Canvas.RaisePropertyChanged(nameof(Canvas.SelectedFigure));
        });
        StatusMessage = $"Создана группа из {group.Children.Count} фигур";
        DebugLog.Write($"Создана группа из {group.Children.Count} фигур");
    }
    
    /// <summary>
    /// Разгруппировывает выбранную группу, добавляя её дочерние фигуры обратно на активный слой.
    /// Если выбрана не группа — выводит сообщение об ошибке.
    /// </summary>
    private void UngroupSelected()
    {
        if (Canvas?.SelectedFigure is not GroupViewModel group)
        {
            StatusMessage = "Выберите группу для разгруппировки";
            DebugLog.Write("Выберите группу для разгруппировки");
            return;
        }
        var activeLayer = Canvas.ActiveLayer;
        if (activeLayer == null) return;
        var children = group.Ungroup();
        activeLayer.Figures.Remove(group);
        foreach (var child in children)
        {
            activeLayer.Figures.Add(child);
        }
        Canvas.SelectedFigures.Clear();
        Canvas.SelectedFigure = null;
        StatusMessage = $"Группа разгруппирована на {children.Count()} фигур";
        DebugLog.Write($"Группа разгруппирована на {children.Count()} фигур");
    }

    /// <summary>
    /// Применяет действие изменения стиля ко всем выделенным фигурам и добавляет команду в историю.
    /// Обрабатывает как отдельные фигуры, так и группы (рекурсивно применяя к дочерним элементам).
    /// </summary>
    /// <param name="apply">Действие, изменяющее свойства фигуры (цвет, толщина, прозрачность и т.д.).</param>
    private void ApplyStyleToSelected(Action<FigureViewModel> apply)
    {
        if (Canvas?.SelectedFigure is GroupViewModel group)
        {
            foreach (var child in group.Children)
            {
                apply(child);
            }
        }
        else if (Canvas?.SelectedFigure is FigureViewModel figure)
        {
            apply(figure);
        }
        else if (Canvas?.SelectedFigures?.Any() == true)
        {
            foreach (var f in Canvas.SelectedFigures)
            {
                apply(f);
            }
        }
        if (Canvas?.SelectedFigures?.Any() == true)
        {
            var cmd = new StyleChangeCommand(
                Canvas.SelectedFigures.Select(f => f.Id).ToList(),
                StrokeColor.Color,
                FillColor.Color,
                StrokeWidth,
                Opacity / 100.0);
            cmd.Execute(Canvas);
            _history.AddAction(cmd);
        }
    }

    /// <summary>
    /// Асинхронно сохраняет проект в файл (заглушка для будущей реализации).
    /// </summary>
    private async Task SaveAsync()
    {
        StatusMessage = "Сохранение...";
        try
        {
            await Task.Delay(100);
            DebugLog.Write("Файл сохранён");
            StatusMessage = "Файл сохранён ✓";
        }
        catch (Exception ex)
        {
            DebugLog.Write("Ошибка сохранения");
            StatusMessage = "Ошибка сохранения ✗";
        }
    }

    // ========== МЕТОДЫ ДОБАВЛЕНИЯ ПРИМИТИВОВ ==========
    
    /// <summary>
    /// Добавляет квадрат с текущими настройками стиля на активный слой.
    /// </summary>
    private void AddSquare()
    {
        var sq = new SquareViewModel(100, 100, 150, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(sq);
        var cmd = new AddFigureCommand(sq, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен квадрат";
    }
    
    /// <summary>
    /// Добавляет прямоугольник с текущими настройками стиля на активный слой.
    /// </summary>
    private void AddRectangle()
    {
        var rect = new RectangleViewModel(100, 100, 150, 100, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(rect);
        var cmd = new AddFigureCommand(rect, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен прямоугольник";
    }
    
    /// <summary>
    /// Добавляет круг с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (100, 100) и радиусом 150 пикселей.
    /// </summary>
    private void AddCircle()
    {
        var circle = new CircleViewModel(100, 100, 150, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(circle);
        var cmd = new AddFigureCommand(circle, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен круг";
    }
    
    /// <summary>
    /// Добавляет эллипс с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (100, 100) и размерами 150×100 пикселей.
    /// </summary>
    private void AddEllipse()
    {
        var ellipse = new EllipseViewModel(100, 100, 150, 100, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(ellipse);
        var cmd = new AddFigureCommand(ellipse, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен эллипс";
    }
    
    /// <summary>
    /// Добавляет линию с текущими настройками стиля на активный слой.
    /// Создаёт фигуру от точки (100, 100) до точки (300, 300).
    /// </summary>
    private void AddLine()
    {
        var line = new LineViewModel(100, 100, 300, 300, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        var cmd = new AddFigureCommand(line, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлена линия";
    }
    
    /// <summary>
    /// Добавляет правильный пятиугольник с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (200, 200) и радиусом описанной окружности 75 пикселей.
    /// </summary>
    private void AddPentagon()
    {
        var pentagon = new PentagonViewModel(
            new Point2D(200, 200), 75,
            StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(pentagon);
        var cmd = new AddFigureCommand(pentagon, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен пятиугольник";
    }
    
    /// <summary>
    /// Добавляет правильный шестиугольник с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (200, 200) и радиусом описанной окружности 75 пикселей.
    /// </summary>
    private void AddHexagon()
    {
        var hexagon = new HexagonViewModel(
            new Point2D(200, 200), 75,
            StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(hexagon);
        var cmd = new AddFigureCommand(hexagon, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен шестиугольник";
    }
    
    /// <summary>
    /// Добавляет правильный семиугольник с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (200, 200) и радиусом описанной окружности 75 пикселей.
    /// </summary>
    private void AddHeptagon()
    {
        var heptagon = new HeptagonViewModel(
            new Point2D(200, 200), 75,
            StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(heptagon);
        var cmd = new AddFigureCommand(heptagon, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен семиугольник";
    }
    
    /// <summary>
    /// Добавляет правильный восьмиугольник с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (200, 200) и радиусом описанной окружности 75 пикселей.
    /// </summary>
    private void AddOctagon()
    {
        var octagon = new OctagonViewModel(
            new Point2D(200, 200), 75,
            StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(octagon);
        var cmd = new AddFigureCommand(octagon, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен восьмиугольник";
    }
    
    /// <summary>
    /// Добавляет треугольник по трём вершинам с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с вершинами в (200,200), (100,200), (200,100).
    /// </summary>
    private void AddTriangle()
    {
        var triangle = new TriangleViewModel(
            new Point2D(200, 200), new Point2D(100, 200), new Point2D(200, 100),
            StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(triangle);
        var cmd = new AddFigureCommand(triangle, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен треугольник";
    }

    /// <summary>
    /// Добавляет прямоугольный треугольник с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с прямым углом в (100, 100) и катетами 100×100 пикселей.
    /// </summary>
    private void AddRightTriangle()
    {
        var triangle = new RightTriangleViewModel(
            100, 100,           // позиция прямого угла
            100, 100,           // ширина и высота катетов
            StrokeColor.Color, 
            StrokeWidth, 
            FillColor.Color, 
            Opacity / 100.0);
    
        ApplyStyle(triangle);
        var cmd = new AddFigureCommand(triangle, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен прямоугольный треугольник";
    }

    /// <summary>
    /// Добавляет ромб с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (200, 200) и диагоналями 100×100 пикселей.
    /// </summary>
    private void AddRhombus()
    {
        var rhombus = new RhombusViewModel(
            200, 200,           // центр
            100, 100,           // ширина и высота (диагонали)
            StrokeColor.Color, 
            StrokeWidth, 
            FillColor.Color, 
            Opacity / 100.0);
    
        ApplyStyle(rhombus);
        var cmd = new AddFigureCommand(rhombus, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлен ромб";
    }
    
    /// <summary>
    /// Добавляет пентаграмму (пятиконечную звезду) с текущими настройками стиля на активный слой.
    /// Создаёт фигуру с центром в (200, 200) и радиусом описанной окружности 50 пикселей.
    /// </summary>
    private void AddPentagram()
    {
        var pentagram = new PentagramViewModel(
            new Point2D(200, 200), 50,
            StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(pentagram);
        var cmd = new AddFigureCommand(pentagram, Canvas.ActiveLayer?.Id);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Добавлена пентаграмма";
    }

    /// <summary>
    /// Удаляет все выделенные фигуры с активного слоя и добавляет действие в историю для Undo.
    /// Очищает коллекцию SelectedFigures после удаления.
    /// </summary>
    private void DeleteSelected()
    {
        if (Canvas?.SelectedFigures?.Any() != true) return;
        var figures = Canvas.SelectedFigures.ToList();
        var cmd = new DeleteFigureCommand(figures);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        Canvas.SelectedFigures.Clear();
        Canvas.SelectedFigure = null;
        StatusMessage = "Объект удалён";
    }
    
    /// <summary>
    /// Дублирует выбранную фигуру со смещением (10, 10) и добавляет в историю для Undo.
    /// Использует метод Clone() для создания глубокой копии фигуры.
    /// </summary>
    private void DuplicateSelected()
    {
        var selectedFigures = Canvas?.ActiveLayer?.Figures?.Where(f => f.IsSelected == true)?.ToList();
        if (selectedFigures == null || !selectedFigures.Any())
            return;
        foreach (var selectedFigure in selectedFigures)
        {
            var original = selectedFigure;
            var clone = original.Clone();
            clone.Move(10, 10);
            var cmd = new AddFigureCommand(clone, Canvas.ActiveLayer?.Id);
            cmd.Execute(Canvas);
            _history.AddAction(cmd);
        }
        StatusMessage = "Объекты дублирован";
    }

    /// <summary>
    /// Вращает выделенные фигуры на -90 градусов (против часовой стрелки).
    /// </summary>
    private void RotateLeft() => RotateSelected(-90);
    
    /// <summary>
    /// Вращает выделенные фигуры на +90 градусов (по часовой стрелке).
    /// </summary>
    private void RotateRight() => RotateSelected(90);
    
    /// <summary>
    /// Вращает выделенные фигуры на 180 градусов.
    /// </summary>
    private void RotateFull() => RotateSelected(180);
    
    /// <summary>
    /// Вращает выделенные фигуры на заданный угол и добавляет действие в историю.
    /// </summary>
    /// <param name="angle">Угол вращения в градусах.</param>
    private void RotateSelected(double angle)
    {
        if (Canvas?.SelectedFigures?.Any() != true) return;
        var allFigureIds = new List<Guid>();
        foreach (var figure in Canvas.SelectedFigures)
        {
            if (figure is GroupViewModel group)
            {
                allFigureIds.AddRange(group.GetAllFigureIds());
            }
            else
            {
                allFigureIds.Add(figure.Id);
            }
        }
        Point2D p = null;
        var cmd = new RotateFigureCommand(
            Canvas.SelectedFigures.Select(f => f.Id).ToList(),
            angle);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = $"Поворот на {angle}°";
    }
    
    /// <summary>
    /// Открывает диалог свободного вращения (заглушка для будущей реализации).
    /// </summary>
    private void RotateFreeClick()
    {
        StatusMessage = "Открытие диалога поворота...";
    }

    /// <summary>
    /// Увеличивает масштаб канваса в 1.5 раза (до максимума 10x).
    /// </summary>
    private void ZoomIn()
    {
        if (Canvas != null)
        {
            var oldZoom = Canvas.Zoom;
            var newZoom = Math.Min(oldZoom * 1.5, 10.0);
            Canvas.Zoom = newZoom;
            var cmd = new ZoomCommand(oldZoom, newZoom);
            cmd.SetCanvas(Canvas);
            _history.AddAction(cmd);
            StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
        }
    }
    
    /// <summary>
    /// Уменьшает масштаб канваса в 2 раза (до минимума 0.1x).
    /// </summary>
    private void ZoomOut()
    {
        if (Canvas != null)
        {
            var oldZoom = Canvas.Zoom;
            var newZoom = Math.Max(oldZoom * 0.5, 0.1);
            Canvas.Zoom = newZoom;
            var cmd = new ZoomCommand(oldZoom, newZoom);
            cmd.SetCanvas(Canvas);
            _history.AddAction(cmd);
            StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
        }
    }
    
    /// <summary>
    /// Сбрасывает масштаб канваса к значению 1.0 (по размеру окна).
    /// </summary>
    private void ZoomFit()
    {
        if (Canvas != null) {
            var oldZoom = Canvas.Zoom;
            Canvas.Zoom = 1.0;
            var cmd = new ZoomCommand(oldZoom, 1.0);
            cmd.SetCanvas(Canvas);
            _history.AddAction(cmd);
            StatusMessage = "Масштаб: по размеру окна";
        }
    }
    
    /// <summary>
    /// Переключает тему интерфейса между светлой и тёмной.
    /// </summary>
    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        StatusMessage = $"Тема: {(CurrentTheme == ThemeVariant.Light ? "Светлая ☀️" : "Тёмная 🌙")}";
    }

    /// <summary>
    /// Выполняет вертикальное отражение выделенных фигур относительно их центра.
    /// Добавляет действие в историю для поддержки Undo.
    /// </summary>
    private void FlipVertical()
    {
        if (Canvas?.SelectedFigures?.Any() != true) return;
        var allFigureIds = new List<Guid>();
        foreach (var figure in Canvas.SelectedFigures)
        {
            if (figure is GroupViewModel group)
            {
                allFigureIds.AddRange(group.GetAllFigureIds());
            }
            else
            {
                allFigureIds.Add(figure.Id);
            }
        }
        var cmd = new ReflectionFigureCommand(
            Canvas.SelectedFigures.Select(f => f.Id).ToList(),
            ReflectionType.Vertical);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Отражение: по вертикали";
    }
    
    /// <summary>
    /// Выполняет горизонтальное отражение выделенных фигур относительно их центра.
    /// Добавляет действие в историю для поддержки Undo.
    /// </summary>
    private void FlipHorizontal()
    {
        if (Canvas?.SelectedFigures?.Any() != true) return;
        var allFigureIds = new List<Guid>();
        foreach (var figure in Canvas.SelectedFigures)
        {
            if (figure is GroupViewModel group)
            {
                allFigureIds.AddRange(group.GetAllFigureIds());
            }
            else
            {
                allFigureIds.Add(figure.Id);
            }
        }
        var cmd = new ReflectionFigureCommand(
            Canvas.SelectedFigures.Select(f => f.Id).ToList(),
            ReflectionType.Horizontal);
        cmd.Execute(Canvas);
        _history.AddAction(cmd);
        StatusMessage = "Отражение: по горизонтали";
    }

    /// <summary>
    /// Обновляет отображаемые координаты курсора мыши в статус-баре.
    /// </summary>
    /// <param name="coords">Кортеж с координатами (X, Y) в координатах канваса.</param>

    private void UpdateCoordinates((double x, double y) coords)
    {
        MouseX = coords.x;
        MouseY = coords.y;
        this.RaisePropertyChanged(nameof(CoordinatesText));
    }

    /// <summary>
    /// Обрабатывает клик по канвасу: активирует слой, выбирает фигуру или начинает рисование.
    /// Логика зависит от текущего выбранного инструмента (Select, примитивы, Pen).
    /// </summary>
    /// <param name="point">Точка клика в координатах канваса.</param>
    public void CanvasClicked(Point2D point)
    {
        DebugLog.Write($"[DEBUG] CanvasClicked START: Tool='{CurrentTool.ToDisplayName()}', IsCanvasActive={Canvas?.IsCanvasActive}, Canvas={Canvas?.GetHashCode()}");
        if (Canvas == null)
        {
            DebugLog.Write("[ERROR] Canvas is null in CanvasClicked!");
            return;
        }
        if (!Canvas.IsCanvasActive)
        {
            DebugLog.Write("[DEBUG] Canvas not active, activating...");
            Canvas.ActivateCanvas();
            StatusMessage = "Слой создан. Можно рисовать! ✏️";
        }
        DebugLog.Write($"[DEBUG] SelectedTool value: '{CurrentTool.ToDisplayName()}' (Length={CurrentTool.ToDisplayName()?.Length})");
        if (CurrentTool == DrawingTool.Select)
        {
            DebugLog.Write("[DEBUG] Tool=Выделение, calling SelectFigureAt");
            Canvas.SelectFigureAt(point);
            StatusMessage = HasSelection ? "Объект выделен" : "Выделение снято";
        }
        else
        {
            if (CurrentTool.IsPrimitive())
            {
                Commands.CanvasClicked.Execute(point);
            }
        }
        DebugLog.Write($"[DEBUG] CanvasClicked END");
    }

    /// <summary>
    /// Заглушка для сохранения проекта (будущая реализация).
    /// </summary>
    private void Save()
    {
        StatusMessage = "Сохранение...";
    }
    
    /// <summary>
    /// Заглушка для открытия проекта (будущая реализация).
    /// </summary>
    private void Open()
    {
        StatusMessage = "Открытие файла...";
    }
    
    /// <summary>
    /// Заглушка для экспорта изображения (будущая реализация).
    /// </summary>
    private void Export()
    {
        StatusMessage = "Экспорт...";
    }
    
    /// <summary>
    /// Создаёт новый слой с уникальным именем и делает его активным для рисования.
    /// Активирует канвас и выводит сообщение о готовности к рисованию.
    /// </summary>
    private void CreateNewLayer()
    {
        if (Canvas == null) return;
        var newLayer = new LayerViewModel($"Слой {Canvas.Layers.Count + 1}");
        Canvas.Layers.Add(newLayer);
        Canvas.ActiveLayer = newLayer;
        Canvas.IsCanvasActive = true;
        DebugLog.Write($"[DEBUG] CreateNewLayer: Created {newLayer.Name}, ActiveLayer={Canvas.ActiveLayer?.Name}");
        StatusMessage = $"Слой '{newLayer.Name}' создан. Можно рисовать! ✏️";
    }

    /// <summary>
    /// Публичный обработчик события нажатия кнопки мыши на канвасе.
    /// Делегирует вызов приватному методу CanvasPointerPressed.
    /// </summary>
    /// <param name="e">Аргументы события PointerPressedEventArgs с данными о нажатии.</param>
    public void HandlePointerPressed(PointerPressedEventArgs e)
    {
        CanvasPointerPressed(e);
    }
    
    /// <summary>
    /// Публичный обработчик события перемещения мыши над канвасом.
    /// Делегирует вызов приватному методу CanvasPointerMoved.
    /// </summary>
    /// <param name="e">Аргументы события PointerEventArgs с данными о позиции курсора.</param>
    public void HandlePointerMoved(PointerEventArgs e)
    {
        CanvasPointerMoved(e);
    }
    
    /// <summary>
    /// Публичный обработчик события отпускания кнопки мыши на канвасе.
    /// Делегирует вызов приватному методу CanvasPointerReleased.
    /// </summary>
    /// <param name="e">Аргументы события PointerReleasedEventArgs с данными об отпускании.</param>
    public void HandlePointerReleased(PointerReleasedEventArgs e)
    {
        CanvasPointerReleased(e);
    }

    private bool s_area = false;
    private Point2D s_start = null;
    private Point2D s_end = null;
    
    /// <summary>
    /// Обрабатывает нажатие кнопки мыши: начинает рисование, выделение областью или выбор фигуры.
    /// Логика зависит от текущего инструмента: примитивы, перо, выделение или текст.
    /// </summary>
    /// <param name="e">Аргументы события PointerPressedEventArgs с данными о нажатии.</param>

    private void CanvasPointerPressed(PointerPressedEventArgs e)
    {
        if (Canvas == null) return;
        var point = GetCanvasPoint(e);
        DebugLog.Write($"[DEBUG] PointerPressed at {point}, Tool={CurrentTool.ToDisplayName()}, IsDrawing={IsDrawing}");
        if (CurrentTool.IsPrimitive())
        {
            if (!IsDrawing || _currentDrawingTool != CurrentTool)
            {
                if (_currentDrawingTool == DrawingTool.Pen && _previewFigure != null && Canvas.ActiveLayer != null)
                {
                    Canvas.ActiveLayer.Figures.Remove(_previewFigure);
                }
                StartDrawing(point, CurrentTool);
            }
            e.Handled = true;
        }
        else if (CurrentTool == DrawingTool.Pen)
        {
            if (!IsDrawing)
            {
                StartPenDrawing(point);
            }
            else if (_currentDrawingTool == DrawingTool.Pen)
            {
                AddPenPoint(point);
            }
            StatusMessage = "Рисование пером: добавляйте точки (Enter для завершения)";
            e.Handled = true;
        }
        else if (CurrentTool == DrawingTool.Select)
        {
			if (IsDrawing)
            {
                if (_currentDrawingTool == DrawingTool.Pen && _previewFigure != null && Canvas.ActiveLayer != null)
                {
                    Canvas.ActiveLayer.Figures.Remove(_previewFigure);
                }
                ResetDrawingState();
            }
            var figure = Canvas.ActiveLayer?.Figures.LastOrDefault(f => f.IsIn(point));

            double xMax = 0, xMin = 0, yMax = 0, yMin = 0;
            bool p_in_s_area = false;
            if (s_area)
            {
                xMax = Math.Max(s_start.X, s_end.X);
                xMin = Math.Min(s_start.X, s_end.X);
                yMax = Math.Max(s_start.Y, s_end.Y);
                yMin = Math.Min(s_start.Y, s_end.Y);
                if ( point.X > xMin && point.X < xMax && point.Y > yMin && point.Y < yMax)
                {

                    p_in_s_area = true;
                }

            }
            
            if (figure != null || (Canvas.SelectedFigures.Any() && p_in_s_area))
            {
                var addToSelection = e.KeyModifiers.HasFlag(KeyModifiers.Control);

                
                if (p_in_s_area)
                {

                    Console.WriteLine(" in selcted area");
                }
                else
                {
                    s_area = false;

                    Canvas.SelectFigureAt(point, addToSelection);
                }
                // Начинаем перетаскивание, если не мультивыделение (или можно всегда начинать, если кликнули на фигуру)
                if (!addToSelection) // начинаем драг только при обычном клике
                {
                    _isDragging = true;
                    _dragStart = point;
                    
                    _originalVertices = new Dictionary<Guid, List<(double X, double Y)>>();
                    foreach (var f in Canvas.SelectedFigures)
                    {
                        // Сохраняем текущие координаты всех вершин
                        _originalVertices[f.Id] = f.Vertices.Select(v => (v.X, v.Y)).ToList();
                    }
                }
				DebugLog.Write($"Объект {HasSelection} и addToSelection =  {addToSelection}");
                StatusMessage = HasSelection ? "Объект выделен" : "Выделение снято";
            }
            else
            {
                // начало выделения областью
                _isSelectingArea = true;
				_selectionStart = point;
                _selectionEnd = point;
                s_area = true;
                _selectionStart = new Point2D(point.X, point.Y);
                s_start = new Point2D(point.X,point.Y);
                s_end = new Point2D(point.X, point.Y);
                _selectionEnd = new Point2D(point.X, point.Y);
                this.RaisePropertyChanged(nameof(IsSelectingArea));
                this.RaisePropertyChanged(nameof(SelectionStart));
                this.RaisePropertyChanged(nameof(SelectionEnd));
                DebugLog.Write($"Начато выделение областью");
            }
            e.Handled = true;
        }
		else if (CurrentTool == DrawingTool.Text)
		{
    		// Текст создаётся по клику, а не по перетаскиванию
    		if (!IsDrawing)
    		{
        		StartTextInput(point);
    		}
    		e.Handled = true;
		}
    }

    /// <summary>
    /// Инициализирует режим рисования пером: создаёт первую точку и предварительную фигуру.
    /// Добавляет точку в коллекцию _penPoints и отображает preview на канвасе.
    /// </summary>
    /// <param name="startPoint">Точка начала рисования в координатах канваса.</param>
    private void StartPenDrawing(Point2D startPoint)
    {
        IsDrawing = true;
        _currentDrawingTool = DrawingTool.Pen;
        _penPoints.Clear();
        _penPoints.Add(startPoint);
        var firstPoint = new PenPointViewModel(startPoint.X, startPoint.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(firstPoint, solidFill: true);
        Canvas?.AddFigure(firstPoint);
        _previewFigure = new PenPointViewModel(startPoint.X, startPoint.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(_previewFigure, solidFill: true);
        Canvas?.AddFigure(_previewFigure);
        _drawingStartPoint = startPoint;
        _hasDrawingStart = true;
        StatusMessage = "Рисование пером: кликайте для добавления точек (Enter для завершения)";
    }

    /// <summary>
    /// Обрабатывает перемещение мыши: обновляет предварительную фигуру или область выделения.
    /// Также обновляет координаты курсора в статус-баре.
    /// </summary>
    /// <param name="e">Аргументы события PointerEventArgs с данными о позиции курсора.</param>
    private void CanvasPointerMoved(PointerEventArgs e)
    {
        if (Canvas == null) return;
        var point = GetCanvasPoint(e);
        UpdateCoordinates((point.X, point.Y));
        if (_isDragging )
        {
            var delta = point - _dragStart;
            foreach (var f in Canvas.SelectedFigures)
            {
                if (_originalVertices.TryGetValue(f.Id, out var originalVerts))
                {
                    // Восстанавливаем исходные координаты и применяем смещение
                    for (int i = 0; i < f.Vertices.Count; i++)
                    {
                        f.Vertices[i].X = originalVerts[i].X + delta.X;
                        f.Vertices[i].Y = originalVerts[i].Y + delta.Y;
                    }
                }
            }
            e.Handled = true;
        }
        else if (_isSelectingArea)
        {
            _selectionEnd = point;
            s_end = point;
            this.RaisePropertyChanged(nameof(SelectionEnd));
            e.Handled = true;
        }
        if (IsDrawing && _hasDrawingStart && _previewFigure != null)
        {
            if (_currentDrawingTool == DrawingTool.Pen)
            {
                if (_previewFigure is PenPointViewModel previewPoint)
                {
                    previewPoint.Vertices[0].X = point.X;
                    previewPoint.Vertices[0].Y = point.Y;
                    previewPoint.RaisePropertyChanged(nameof(PenPointViewModel.X));
                    previewPoint.RaisePropertyChanged(nameof(PenPointViewModel.Y));
                }
            }
            else
            {
                UpdatePreviewFigure(_previewFigure, _drawingStartPoint, point);
            }
            e.Handled = true;
        }
        if (CurrentTool == DrawingTool.Select && _isSelectingArea)
        {
            _selectionEnd = point;
            this.RaisePropertyChanged(nameof(SelectionEnd));
            e.Handled = true;
        }
    }

    /// <summary>
    /// Обрабатывает отпускание кнопки мыши: завершает рисование или выделение областью.
    /// Для перетаскивания фигур добавляет команду DragMoveCommand в историю.
    /// </summary>
    /// <param name="e">Аргументы события PointerReleasedEventArgs с данными об отпускании.</param>
    private void CanvasPointerReleased(PointerReleasedEventArgs e)
    {
        if (Canvas == null) return;
        var point = GetCanvasPoint(e);
        if (_isDragging && _originalVertices != null)
        {
            _isDragging = false;
            var delta = point - _dragStart;
            if (delta.X != 0 || delta.Y != 0)
            {
                // Создаём команду перемещения
                var figureIds = Canvas.SelectedFigures.Select(f => f.Id).ToList();
                var cmd = new DragMoveCommand(figureIds, _originalVertices, delta);
              
          
                cmd.SetCanvas(Canvas);
                _history.AddAction(cmd);
            }
            _originalVertices = null;
            e.Handled = true;
        }
        else if (_isSelectingArea)
        {
            // завершение выделения областью
            _isSelectingArea = false;
            SelectFiguresInArea(_selectionStart, _selectionEnd);
            e.Handled = true;
        }
        DebugLog.Write($"[DEBUG] PointerReleased at {point}, IsDrawing={IsDrawing}");
        if (IsDrawing && _hasDrawingStart && CurrentTool.IsPrimitive())
        {
            FinishDrawingPrimitive(point);
            e.Handled = true;
        }
        if (CurrentTool == DrawingTool.Select && _isSelectingArea)
        {
            _isSelectingArea = false;
            this.RaisePropertyChanged(nameof(IsSelectingArea));
            SelectFiguresInArea(_selectionStart, _selectionEnd);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Выделяет все фигуры, полностью попавшие в прямоугольную область выделения.
    /// Снимает предыдущее выделение и обновляет коллекцию SelectedFigures.
    /// </summary>
    /// <param name="start">Начальная точка области выделения в координатах канваса.</param>
    /// <param name="end">Конечная точка области выделения в координатах канваса.</param>
    private void SelectFiguresInArea(Point2D start, Point2D end)
    {
        if (Canvas?.ActiveLayer == null) return;
        var minX = Math.Min(start.X, end.X);
        var maxX = Math.Max(start.X, end.X);
        var minY = Math.Min(start.Y, end.Y);
        var maxY = Math.Max(start.Y, end.Y);
        var figuresInArea = Canvas.ActiveLayer.Figures
            .Where(f =>
            {
                var bbox = f.GetBoundingBox();
                return bbox.MinX >= minX && bbox.MaxX <= maxX &&
                       bbox.MinY >= minY && bbox.MaxY <= maxY;
            })
            .ToList();
        foreach (var f in Canvas.SelectedFigures)
            f.IsSelected = false;
        Canvas.SelectedFigures.Clear();
        foreach (var f in figuresInArea)
        {
            f.IsSelected = true;
            Canvas.SelectedFigures.Add(f);
        }
        Canvas.RaisePropertyChanged(nameof(Canvas.SelectedFigures));
        Canvas.RaisePropertyChanged(nameof(Canvas.HasSelection));
        StatusMessage = $"Выделено {figuresInArea.Count} фигур(ы)";
    }

    /// <summary>
    /// Инициализирует режим рисования примитива: создаёт предварительную фигуру.
    /// Сбрасывает предыдущее состояние рисования, если активен другой инструмент.
    /// </summary>
    /// <param name="startPoint">Точка начала рисования в координатах канваса.</param>
    /// <param name="tool">Инструмент DrawingTool для создания соответствующей предварительной фигуры.</param>
    private void StartDrawing(Point2D startPoint, DrawingTool tool)
    {
        if (IsDrawing && _currentDrawingTool != tool)
        {
            if (_currentDrawingTool == DrawingTool.Pen && _previewFigure != null && Canvas?.ActiveLayer != null)
            {
                Canvas.ActiveLayer.Figures.Remove(_previewFigure);
            }
            ResetDrawingState();
        }
        IsDrawing = true;
        _drawingStartPoint = startPoint;
        _hasDrawingStart = true;
        _currentDrawingTool = tool;
        _previewFigure = CreatePreviewFigure(startPoint, startPoint, tool);
        if (_previewFigure != null)
        {
            Canvas?.AddFigure(_previewFigure);
            StatusMessage = $"Рисование {tool}: отпустите кнопку мыши для завершения";
        }
    }

    /// <summary>
    /// Завершает рисование примитива: создаёт финальную фигуру и добавляет её на слой.
    /// Проверяет минимальный размер фигуры для предотвращения создания микро-объектов.
    /// </summary>
    /// <param name="endPoint">Конечная точка рисования в координатах канваса.</param>
    private void FinishDrawingPrimitive(Point2D endPoint)
    {
        var start = _drawingStartPoint;
        var end = endPoint;
        if (_previewFigure != null && Canvas.ActiveLayer != null)
        {
            Canvas.ActiveLayer.Figures.Remove(_previewFigure);
        }
        bool isValid = Math.Abs(end.X - start.X) > MinFigureSize || Math.Abs(end.Y - start.Y) > MinFigureSize;
        if (isValid)
        {
            FigureViewModel? finalFigure = CreateFinalFigure(start, end, _currentDrawingTool);
            if (finalFigure != null)
            {
                Canvas.AddFigure(finalFigure);
                StatusMessage = $"{_currentDrawingTool.ToDisplayName()} создан";
            }
        }
        else
        {
            StatusMessage = $"{_currentDrawingTool.ToDisplayName()} слишком маленький, не создан";
        }
        ResetDrawingState();
    }

    /// <summary>
    /// Добавляет новую точку в режиме рисования пером и обновляет предварительную фигуру.
    /// Сохраняет точку в коллекции _penPoints для последующего использования.
    /// </summary>
    /// <param name="point">Координаты добавляемой точки в координатах канваса.</param>
    private void AddPenPoint(Point2D point)
    {
        _penPoints.Add(point);
        if (_previewFigure != null && Canvas.ActiveLayer != null)
        {
            Canvas.ActiveLayer.Figures.Remove(_previewFigure);
        }
        var penPoint = new PenPointViewModel(point.X, point.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(penPoint, solidFill: true);
        Canvas?.AddFigure(penPoint);
        _previewFigure = new PenPointViewModel(point.X, point.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(_previewFigure, solidFill: true);
        Canvas?.AddFigure(_previewFigure);
        StatusMessage = $"Точка {_penPoints.Count}: ({point.X:F0}, {point.Y:F0})";
        _drawingStartPoint = point;
    }

    /// <summary>
    /// Обновляет координаты предварительной точки пера при движении мыши.
    /// Вызывает RaisePropertyChanged для реактивного обновления UI.
    /// </summary>
    /// <param name="point">Новые координаты для предварительной точки в координатах канваса.</param>
    private void UpdatePreviewPoint(Point2D point)
    {
        if (_previewFigure is PenPointViewModel previewPoint)
        {
            previewPoint.Vertices[0].X = point.X;
            previewPoint.Vertices[0].Y = point.Y;
            previewPoint.RaisePropertyChanged(nameof(PenPointViewModel.X));
            previewPoint.RaisePropertyChanged(nameof(PenPointViewModel.Y));
        }
    }

    /// <summary>
    /// Завершает режим рисования пером: удаляет предварительную фигуру и сбрасывает состояние.
    /// Выводит сообщение с количеством созданных точек.
    /// </summary>
    private void FinishPenDrawing()
    {
        if (_previewFigure != null && Canvas.ActiveLayer != null)
        {
            Canvas.ActiveLayer.Figures.Remove(_previewFigure);
        }
        StatusMessage = $"Рисование пером завершено. Всего точек: {_penPoints.Count}";
        ResetDrawingState();
    }

    /// <summary>
    /// Создаёт финальную фигуру по заданным координатам и инструменту.
    /// </summary>
    /// <param name="start">Начальная точка рисования.</param>
    /// <param name="end">Конечная точка рисования.</param>
    /// <param name="tool">Тип инструмента для создания соответствующей фигуры.</param>
    /// <returns>Экземпляр FigureViewModel или null, если создание невозможно.</returns>
    private FigureViewModel? CreateFinalFigure(Point2D start, Point2D end, DrawingTool tool)
    {
        var size = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var radius = size / 2;
        FigureViewModel? figure = tool switch
        {
            DrawingTool.Line => new LineViewModel(start.X, start.Y, end.X, end.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Square => new SquareViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), size, size, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Circle => new CircleViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), size, size, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Rectangle => new RectangleViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y), StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Ellipse => new EllipseViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y), StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Pentagon => new PentagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Hexagon => new HexagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Octagon => new OctagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Heptagon => new HeptagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Pentagram => new PentagramViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Triangle => new TriangleViewModel(
                new Point2D(center.X, center.Y - radius),
                new Point2D(center.X - radius, center.Y + radius),
                new Point2D(center.X + radius, center.Y + radius),
                StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.RightTriangle => new RightTriangleViewModel(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y),
                StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Rhombus => new RhombusViewModel(
                center.X, center.Y,
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y),
                StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Text => new TextViewModel(
                start.X, start.Y,
                "Новый текст",  // Дефолтный текст, пользователь сможет отредактировать
                24,  // Размер шрифта по умолчанию
                "Segoe UI",
                StrokeColor.Color,
                FillColor.Color,
                Opacity / 100.0),
            _ => null
        };
        if (figure != null)
            ApplyStyle(figure);
        return figure;
    }

    /// <summary>
    /// Сбрасывает все флаги и поля, связанные с режимом рисования.
    /// Используется при завершении рисования или смене инструмента.
    /// </summary>
    private void ResetDrawingState()
    {
        IsDrawing = false;
        _hasDrawingStart = false;
        _previewFigure = null;
        _currentDrawingTool = default;
        _penPoints.Clear();
    }

    /// <summary>
    /// Создаёт предварительную фигуру для визуализации в процессе рисования.
    /// </summary>
    /// <param name="start">Начальная точка рисования.</param>
    /// <param name="end">Текущая конечная точка.</param>
    /// <param name="tool">Тип инструмента для создания соответствующей предварительной фигуры.</param>
    /// <returns>Экземпляр FigureViewModel для предварительного отображения или null.</returns>
    private FigureViewModel? CreatePreviewFigure(Point2D start, Point2D end, DrawingTool tool)
    {
        var size = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var radius = size / 2;
        FigureViewModel? figure = tool switch
        {
            DrawingTool.Line => new LineViewModel(start.X, start.Y, end.X, end.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Square => new SquareViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), size, size, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Circle => new CircleViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), size, size, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Rectangle => new RectangleViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y), StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Ellipse => new EllipseViewModel(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y), StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Pen => new PenPointViewModel(start.X, start.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Pentagon => new PentagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Hexagon => new HexagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Octagon => new OctagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Heptagon => new HeptagonViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Pentagram => new PentagramViewModel(center, radius, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Triangle => new TriangleViewModel(
                new Point2D(center.X, center.Y - Math.Max(1, radius)),
                new Point2D(center.X - Math.Max(1, radius), center.Y + Math.Max(1, radius)),
                new Point2D(center.X + Math.Max(1, radius), center.Y + Math.Max(1, radius)),
                StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.RightTriangle => new RightTriangleViewModel(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y),
                StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Rhombus => new RhombusViewModel(
                center.X, center.Y,
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y),
                StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
            DrawingTool.Text => new TextViewModel(
                start.X, start.Y,
                "Текст...",
                24,
                "Segoe UI",
                StrokeColor.Color,
                FillColor.Color,
                Opacity / 100.0),
            _ => null
        };
        if (figure != null)
        {
            if (tool == DrawingTool.Pen)
            {
                ApplyStyle(figure, solidFill: true);
            }
            else
            {
                ApplyStyle(figure);
            }
        }
        return figure;
    }

    /// <summary>
    /// Обновляет геометрию предварительной фигуры при перемещении мыши.
    /// </summary>
    /// <param name="preview">Экземпляр предварительной фигуры для обновления.</param>
    /// <param name="start">Начальная точка рисования.</param>
    /// <param name="end">Текущая конечная точка.</param>
    private void UpdatePreviewFigure(FigureViewModel preview, Point2D start, Point2D end)
    {
        var size = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        var radius = size / 2;
        switch (preview)
        {
            case LineViewModel line:
                line.Vertices[1].X = end.X;
                line.Vertices[1].Y = end.Y;
                line.RaisePropertyChanged(nameof(LineViewModel.X1));
                line.RaisePropertyChanged(nameof(LineViewModel.Y1));
                line.RaisePropertyChanged(nameof(LineViewModel.X2));
                line.RaisePropertyChanged(nameof(LineViewModel.Y2));
                break;
            case RectangleViewModel rect when rect.Name == "Квадрат":
                rect.Vertices[0].X = Math.Min(start.X, end.X);
                rect.Vertices[0].Y = Math.Min(start.Y, end.Y);
                rect.Vertices[1].X = rect.Vertices[0].X + size;
                rect.Vertices[1].Y = rect.Vertices[0].Y;
                rect.Vertices[2].X = rect.Vertices[0].X + size;
                rect.Vertices[2].Y = rect.Vertices[0].Y + size;
                rect.Vertices[3].X = rect.Vertices[0].X;
                rect.Vertices[3].Y = rect.Vertices[0].Y + size;
                rect.RaisePropertyChanged(nameof(RectangleViewModel.X));
                rect.RaisePropertyChanged(nameof(RectangleViewModel.Y));
                rect.RaisePropertyChanged(nameof(RectangleViewModel.Width));
                rect.RaisePropertyChanged(nameof(RectangleViewModel.Height));
                break;
            case RectangleViewModel rect:
                double rectX = Math.Min(start.X, end.X);
                double rectY = Math.Min(start.Y, end.Y);
                double rectWidth = Math.Abs(end.X - start.X);
                double rectHeight = Math.Abs(end.Y - start.Y);
                rect.Vertices[0].X = rectX;
                rect.Vertices[0].Y = rectY;
                rect.Vertices[1].X = rectX + rectWidth;
                rect.Vertices[1].Y = rectY;
                rect.Vertices[2].X = rectX + rectWidth;
                rect.Vertices[2].Y = rectY + rectHeight;
                rect.Vertices[3].X = rectX;
                rect.Vertices[3].Y = rectY + rectHeight;
                rect.RaisePropertyChanged(nameof(RectangleViewModel.X));
                rect.RaisePropertyChanged(nameof(RectangleViewModel.Y));
                rect.RaisePropertyChanged(nameof(RectangleViewModel.Width));
                rect.RaisePropertyChanged(nameof(RectangleViewModel.Height));
                break;
            case EllipseViewModel ellipse when ellipse.Name == "Круг":
                ellipse.Vertices[0].X = Math.Min(start.X, end.X);
                ellipse.Vertices[0].Y = Math.Min(start.Y, end.Y);
                ellipse.Vertices[1].X = ellipse.Vertices[0].X + size;
                ellipse.Vertices[1].Y = ellipse.Vertices[0].Y;
                ellipse.Vertices[2].X = ellipse.Vertices[0].X + size;
                ellipse.Vertices[2].Y = ellipse.Vertices[0].Y + size;
                ellipse.Vertices[3].X = ellipse.Vertices[0].X;
                ellipse.Vertices[3].Y = ellipse.Vertices[0].Y + size;
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.X));
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Y));
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Width));
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Height));
                break;
            case EllipseViewModel ellipse:
                double ellipseX = Math.Min(start.X, end.X);
                double ellipseY = Math.Min(start.Y, end.Y);
                double ellipseWidth = Math.Abs(end.X - start.X);
                double ellipseHeight = Math.Abs(end.Y - start.Y);
                ellipse.Vertices[0].X = ellipseX;
                ellipse.Vertices[0].Y = ellipseY;
                ellipse.Vertices[1].X = ellipseX + ellipseWidth;
                ellipse.Vertices[1].Y = ellipseY;
                ellipse.Vertices[2].X = ellipseX + ellipseWidth;
                ellipse.Vertices[2].Y = ellipseY + ellipseHeight;
                ellipse.Vertices[3].X = ellipseX;
                ellipse.Vertices[3].Y = ellipseY + ellipseHeight;
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.X));
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Y));
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Width));
                ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Height));
                break;
            case RhombusViewModel rhombus:
                // Пересчитываем вершины ромба по новым размерам
                UpdatePolygonBoundingBox(rhombus, start, end);
                break;
            case RegularPolygonViewModel polygon:
                polygon.UpdateVertices(center, radius);
                break;
            case PentagramViewModel star:
                star.UpdateVertices(center, radius);
                break;
            case TriangleViewModel triangle:
                UpdatePolygonBoundingBox(triangle, start, end);
                break;
            case RightTriangleViewModel rt:
                rt.UpdateVertices(
                    Math.Min(start.X, end.X),
                    Math.Min(start.Y, end.Y),
                    Math.Abs(end.X - start.X),
                    Math.Abs(end.Y - start.Y));
                break;
        }
    }

    /// <summary>
    /// Масштабирует bounding box полигона относительно центра при изменении размеров.
    /// </summary>
    /// <param name="polygon">Модель полигона для обновления.</param>
    /// <param name="start">Начальная точка масштабирования.</param>
    /// <param name="end">Конечная точка масштабирования.</param>
    private void UpdatePolygonBoundingBox(PolygonViewModel polygon, Point2D start, Point2D end)
    {
        var minX = polygon.Vertices.Min(v => v.X);
        var maxX = polygon.Vertices.Max(v => v.X);
        var minY = polygon.Vertices.Min(v => v.Y);
        var maxY = polygon.Vertices.Max(v => v.Y);
        var origWidth = Math.Max(maxX - minX, 1.0);
        var origHeight = Math.Max(maxY - minY, 1.0);
        var targetWidth = Math.Abs(end.X - start.X);
        var targetHeight = Math.Abs(end.Y - start.Y);
        var scaleX = targetWidth / origWidth;
        var scaleY = targetHeight / origHeight;
        var scale = Math.Max(scaleX, scaleY);
        var center = new Point2D((minX + maxX) / 2, (minY + maxY) / 2);
        var newCenter = new Point2D(Math.Min(start.X, end.X) + targetWidth/2, Math.Min(start.Y, end.Y) + targetHeight/2);
        foreach (var vertex in polygon.Vertices)
        {
            var dx = vertex.X - center.X;
            var dy = vertex.Y - center.Y;
            vertex.X = newCenter.X + dx * scale;
            vertex.Y = newCenter.Y + dy * scale;
            vertex.RaisePropertyChanged(nameof(PointViewModel.X));
            vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
        }
        polygon.RaisePropertyChanged(nameof(PolygonViewModel.Center));
        polygon.RaisePropertyChanged(nameof(PolygonViewModel.Vertices));
    }
    
    // === Буфер обмена ===
    
    /// <summary>
    /// Вырезает выделенные фигуры: копирует их в буфер обмена и удаляет с канваса.
    /// Комбинирует методы CopySelected() и DeleteSelected() для операции Cut.
    /// </summary>
    private void CutSelected()
	{
    	CopySelected();
    	DeleteSelected();
	}

    /// <summary>
    /// Копирует выделенные фигуры во внутренний буфер обмена _clipboard.
    /// Использует метод Clone() для создания глубокой копии каждой фигуры.
    /// </summary>
	private void CopySelected()
	{
    	_clipboard.Clear();
    	if (Canvas?.SelectedFigures?.Any() == true)
    	{
        	foreach (var figure in Canvas.SelectedFigures)
        	{
            	_clipboard.Add(figure.Clone());
        	}
    	}
    	StatusMessage = $"Скопировано {_clipboard.Count} объектов";
	}

    /// <summary>
    /// Вставляет фигуры из буфера обмена на активный слой со смещением (20, 20).
    /// Использует метод Clone() для создания независимых копий вставляемых фигур.
    /// </summary>
	private void PasteSelected()
	{
    	if (_clipboard.Count == 0 || Canvas?.ActiveLayer == null) return;
    
    	foreach (var original in _clipboard)
    	{
        	var clone = original.Clone();
        	clone.Move(20, 20); // Смещение при вставке
        	Canvas.AddFigure(clone);
    	}
    	StatusMessage = $"Вставлено {_clipboard.Count} объектов";
	}
    
    // === Выделение ===

    /// <summary>
    /// Выделяет все фигуры на активном слое.
    /// Снимает предыдущее выделение, затем добавляет все фигуры в SelectedFigures.
    /// </summary>
	private void SelectAll()
	{
    	if (Canvas?.ActiveLayer == null) return;
    
    	foreach (var f in Canvas.SelectedFigures)
        	f.IsSelected = false;
    	Canvas.SelectedFigures.Clear();
    
    	foreach (var figure in Canvas.ActiveLayer.Figures)
    	{
        	figure.IsSelected = true;
        	Canvas.SelectedFigures.Add(figure);
    	}
    	Canvas.RaisePropertyChanged(nameof(Canvas.SelectedFigures));
    	StatusMessage = $"Выделено {Canvas.SelectedFigures.Count} объектов";
	}

    /// <summary>
    /// Снимает выделение со всех фигур и очищает коллекцию SelectedFigures.
    /// Устанавливает SelectedFigure в null для сброса состояния выбора.
    /// </summary>
	private void DeselectAll()
	{
    	foreach (var f in Canvas.SelectedFigures)
        	f.IsSelected = false;
    	Canvas.SelectedFigures.Clear();
    	Canvas.SelectedFigure = null;
    	StatusMessage = "Выделение снято";
	}
    
    // === Порядок отрисовки (Z-order) ===

    /// <summary>
    /// Перемещает выделенные фигуры на передний план активного слоя.
    /// Фигуры, добавленные последними, отрисовываются поверх остальных.
    /// </summary>
    private void BringSelectedToFront()
    {
        if (Canvas?.ActiveLayer == null || !Canvas.SelectedFigures.Any()) return;
        
        foreach (var figure in Canvas.SelectedFigures.ToList())
        {
            Canvas.ActiveLayer.Figures.Remove(figure);
            Canvas.ActiveLayer.Figures.Add(figure);
        }
        StatusMessage = "На передний план";
    }

    /// <summary>
    /// Перемещает выделенные фигуры на задний план активного слоя.
    /// Фигуры, добавленные первыми, отрисовываются под остальными.
    /// </summary>
    private void SendSelectedToBack()
    {
        if (Canvas?.ActiveLayer == null || !Canvas.SelectedFigures.Any()) return;
        
        foreach (var figure in Canvas.SelectedFigures.Reverse().ToList())
        {
            Canvas.ActiveLayer.Figures.Remove(figure);
            Canvas.ActiveLayer.Figures.Insert(0, figure);
        }
        StatusMessage = "На задний план";
    }

    /// <summary>
    /// Перемещает выделенные фигуры на один уровень вверх в порядке отрисовки.
    /// Меняет местами фигуру со следующей в коллекции, если это возможно.
    /// </summary>
    private void BringSelectedForward()
    {
        if (Canvas?.ActiveLayer == null || !Canvas.SelectedFigures.Any()) return;
        
        foreach (var figure in Canvas.SelectedFigures.ToList())
        {
            var index = Canvas.ActiveLayer.Figures.IndexOf(figure);
            if (index < Canvas.ActiveLayer.Figures.Count - 1)
            {
                Canvas.ActiveLayer.Figures.Move(index, index + 1);
            }
        }
        StatusMessage = "Перемещено вперёд";
    }

    /// <summary>
    /// Перемещает выделенные фигуры на один уровень вниз в порядке отрисовки.
    /// Меняет местами фигуру с предыдущей в коллекции, если это возможно.
    /// </summary>
    private void SendSelectedBackward()
    {
        if (Canvas?.ActiveLayer == null || !Canvas.SelectedFigures.Any()) return;
        
        foreach (var figure in Canvas.SelectedFigures.ToList())
        {
            var index = Canvas.ActiveLayer.Figures.IndexOf(figure);
            if (index > 0)
            {
                Canvas.ActiveLayer.Figures.Move(index, index - 1);
            }
        }
        StatusMessage = "Перемещено назад";
    }

    // === Выравнивание ===
    
    /// <summary>Перечисление типов выравнивания для метода AlignSelected.</summary>
    private enum AlignType { Left, Center, Right, Top, Middle, Bottom }

    /// <summary>Выравнивает выделенные фигуры по левому краю.</summary>
    private void AlignLeft() => AlignSelected(AlignType.Left);

    /// <summary>Выравнивает выделенные фигуры по центру горизонтально.</summary>
    private void AlignCenter() => AlignSelected(AlignType.Center);

    /// <summary>Выравнивает выделенные фигуры по правому краю.</summary>
    private void AlignRight() => AlignSelected(AlignType.Right);

    /// <summary>Выравнивает выделенные фигуры по верхнему краю.</summary>
    private void AlignTop() => AlignSelected(AlignType.Top);

    /// <summary>Выравнивает выделенные фигуры по центру вертикально.</summary>
    private void AlignMiddle() => AlignSelected(AlignType.Middle);

    /// <summary>Выравнивает выделенные фигуры по нижнему краю.</summary>
    private void AlignBottom() => AlignSelected(AlignType.Bottom);

    /// <summary>
    /// Выравнивает выделенные фигуры по заданному типу.
    /// Вычисляет целевую координату на основе ограничивающих прямоугольников всех фигур,
    /// затем смещает каждую фигуру к этой координате по соответствующей оси.
    /// </summary>
    /// <param name="type">Тип выравнивания из перечисления AlignType.</param>
    private void AlignSelected(AlignType type)
    {
        if (Canvas?.SelectedFigures?.Count < 2) return;
        
        var bounds = Canvas.SelectedFigures.Select(f => f.GetBoundingBox()).ToList();
        double target = type switch
        {
            AlignType.Left => bounds.Min(b => b.MinX),
            AlignType.Center => bounds.Average(b => (b.MinX + b.MaxX) / 2),
            AlignType.Right => bounds.Max(b => b.MaxX),
            AlignType.Top => bounds.Min(b => b.MinY),
            AlignType.Middle => bounds.Average(b => (b.MinY + b.MaxY) / 2),
            AlignType.Bottom => bounds.Max(b => b.MaxY),
            _ => 0
        };
        foreach (var figure in Canvas.SelectedFigures)
        {
            var bbox = figure.GetBoundingBox();
            double current = type switch
            {
                AlignType.Left or AlignType.Right or AlignType.Center => 
                    type == AlignType.Center ? (bbox.MinX + bbox.MaxX) / 2 : 
                    type == AlignType.Left ? bbox.MinX : bbox.MaxX,
                _ => type == AlignType.Middle ? (bbox.MinY + bbox.MaxY) / 2 : 
                    type == AlignType.Top ? bbox.MinY : bbox.MaxY
            };
            
            double delta = target - current;
            if (type is AlignType.Left or AlignType.Center or AlignType.Right)
                figure.Move(delta, 0);
            else
                figure.Move(0, delta);
        }
        StatusMessage = $"Выровнено по {(type == AlignType.Center || type == AlignType.Middle ? "центру" : type.ToString().ToLower())}";
    }

    // === Распределение ===
    
    /// <summary>
    /// Равномерно распределяет выделенные фигуры по горизонтали.
    /// Вычисляет шаг между крайними фигурами и позиционирует промежуточные фигуры с равными интервалами.
    /// Требует минимум 3 выделенные фигуры.
    /// </summary>
    private void DistributeHorizontal()
    {
        if (Canvas?.SelectedFigures?.Count < 3) return;
        
        var sorted = Canvas.SelectedFigures
            .OrderBy(f => f.GetBoundingBox().MinX)
            .ToList();
        
        double min = sorted.First().GetBoundingBox().MinX;
        double max = sorted.Last().GetBoundingBox().MaxX;
        double step = (max - min) / (sorted.Count - 1);
        
        for (int i = 1; i < sorted.Count - 1; i++)
        {
            var figure = sorted[i];
            var bbox = figure.GetBoundingBox();
            double target = min + i * step - (bbox.MinX + bbox.MaxX) / 2;
            figure.Move(target, 0);
        }
        StatusMessage = "Распределено горизонтально";
    }

    /// <summary>
    /// Равномерно распределяет выделенные фигуры по вертикали.
    /// Вычисляет шаг между крайними фигурами и позиционирует промежуточные фигуры с равными интервалами.
    /// Требует минимум 3 выделенные фигуры.
    /// </summary>
    private void DistributeVertical()
    {
        if (Canvas?.SelectedFigures?.Count < 3) return;
        
        var sorted = Canvas.SelectedFigures
            .OrderBy(f => f.GetBoundingBox().MinY)
            .ToList();
        
        double min = sorted.First().GetBoundingBox().MinY;
        double max = sorted.Last().GetBoundingBox().MaxY;
        double step = (max - min) / (sorted.Count - 1);
        
        for (int i = 1; i < sorted.Count - 1; i++)
        {
            var figure = sorted[i];
            var bbox = figure.GetBoundingBox();
            double target = min + i * step - (bbox.MinY + bbox.MaxY) / 2;
            figure.Move(0, target);
        }
        StatusMessage = "Распределено вертикально";
    }

    // === Масштаб фигур ===
    /// <summary>
    /// Увеличивает масштаб выделенных фигур на 10% (коэффициент 1.1).
    /// </summary>
    private void ScaleSelectedUp() => ScaleSelected(1.1, 1.1);

    /// <summary>
    /// Уменьшает масштаб выделенных фигур на 10% (коэффициент 0.9).
    /// </summary>
    private void ScaleSelectedDown() => ScaleSelected(0.9, 0.9);

    /// <summary>
    /// Масштабирует выделенные фигуры по заданным коэффициентам по осям X и Y.
    /// Применяет метод Scale() к каждой фигуре в коллекции SelectedFigures.
    /// </summary>
    /// <param name="sx">Коэффициент масштабирования по оси X (1.0 = 100%).</param>
    /// <param name="sy">Коэффициент масштабирования по оси Y (1.0 = 100%).</param>
    private void ScaleSelected(double sx, double sy)
    {
        if (Canvas?.SelectedFigures?.Any() != true) return;
    
        foreach (var figure in Canvas.SelectedFigures)
        {
            figure.Scale(sx, sy);
        }
        StatusMessage = $"Масштаб: {sx:P0}";
    }

    /// <summary>
    /// Заглушка для масштабирования выделенных фигур под размер видимой области канваса.
    /// Планируется реализация автоматического подбора коэффициентов масштабирования.
    /// </summary>
    private void ScaleSelectedToFit()
    {
        // Заглушка: масштабирует выделенное под размер видимой области
        StatusMessage = "Масштабирование по размеру холста (заглушка)";
    }

    // === Стиль ===
    
    /// <summary>
    /// Устанавливает толщину обводки для выделенных фигур.
    /// Парсит строковое значение в целое число и применяет ко всем выделенным фигурам.
    /// </summary>
    /// <param name="widthStr">Строковое представление толщины в пикселях.</param>
    private void SetStrokeWidth(string widthStr)
    {
        if (int.TryParse(widthStr, out var width) && Canvas?.SelectedFigures?.Any() == true)
        {
            foreach (var f in Canvas.SelectedFigures)
                f.Thickness = width;
            StrokeWidth = width;
            StatusMessage = $"Толщина: {width} пкс";
        }
    }

    /// <summary>
    /// Отменяет заливку выделенных фигур, устанавливая прозрачный цвет.
    /// </summary>
    private void SetFillNone()
    {
        if (Canvas?.SelectedFigures?.Any() == true)
        {
            foreach (var f in Canvas.SelectedFigures)
                f.FillColor = System.Drawing.Color.Transparent;
            StatusMessage = "Заливка: нет";
        }
    }

    /// <summary>
    /// Отменяет обводку выделенных фигур, устанавливая прозрачный цвет.
    /// </summary>
    private void SetStrokeNone()
    {
        if (Canvas?.SelectedFigures?.Any() == true)
        {
            foreach (var f in Canvas.SelectedFigures)
                f.LineColor = System.Drawing.Color.Transparent;
            StatusMessage = "Обводка: нет";
        }
    }

    /// <summary>
    /// Заглушка для открытия панели свойств выделенного объекта.
    /// Планируется реализация диалога с расширенными настройками фигуры.
    /// </summary>
    private void OpenProperties()
    {
        // TODO: Открыть панель свойств объекта
        StatusMessage = "Свойства объекта (заглушка)";
    }

    // === Управление слоями ===

    /// <summary>
    /// Объединяет активный слой с предыдущим в списке слоёв.
    /// Переносит все фигуры из текущего слоя в предыдущий, затем удаляет текущий слой.
    /// Если активный слой первый в списке — выводит сообщение об ошибке.
    /// </summary>
    private void MergeLayerWithPrevious()
    {
        if (Canvas?.ActiveLayer == null) return;
        var index = Canvas.Layers.IndexOf(Canvas.ActiveLayer);
        if (index <= 0)
        {
            StatusMessage = "Нет предыдущего слоя для объединения";
            return;
        }
        
        var current = Canvas.ActiveLayer;
        var previous = Canvas.Layers[index - 1];
        
        foreach (var figure in current.Figures.ToList())
        {
            previous.Figures.Add(figure);
            current.Figures.Remove(figure);
        }
        
        Canvas.Layers.Remove(current);
        Canvas.ActiveLayer = previous;
        
        StatusMessage = $"Слой '{current.Name}' объединён с '{previous.Name}'";
    }
    
    /// <summary>
    /// Перемещает активный слой на самый передний план отрисовки (последний в коллекции).
    /// </summary>
    private void BringActiveLayerToFront()
    {
        if (Canvas?.ActiveLayer == null) return;
        Canvas.Layers.Remove(Canvas.ActiveLayer);
        Canvas.Layers.Add(Canvas.ActiveLayer);
        StatusMessage = "Слой перемещён на передний план";
    }

    /// <summary>
    /// Перемещает активный слой на самый задний план отрисовки (первый в коллекции).
    /// </summary>
    private void SendActiveLayerToBack()
    {
        if (Canvas?.ActiveLayer == null) return;
        Canvas.Layers.Remove(Canvas.ActiveLayer);
        Canvas.Layers.Insert(0, Canvas.ActiveLayer);
        StatusMessage = "Слой перемещён на задний план";
    }

    /// <summary>
    /// Перемещает активный слой на один уровень вверх в порядке отрисовки.
    /// Меняет местами слой со следующим в коллекции, если это возможно.
    /// </summary>
    private void BringActiveLayerForward()
    {
        if (Canvas?.ActiveLayer == null) return;
        var index = Canvas.Layers.IndexOf(Canvas.ActiveLayer);
        if (index < Canvas.Layers.Count - 1)
        {
            Canvas.Layers.Move(index, index + 1);
            StatusMessage = "Слой перемещён вперёд";
        }
    }

    /// <summary>
    /// Перемещает активный слой на один уровень вниз в порядке отрисовки.
    /// Меняет местами слой с предыдущим в коллекции, если это возможно.
    /// </summary>
    private void SendActiveLayerBackward()
    {
        if (Canvas?.ActiveLayer == null) return;
        var index = Canvas.Layers.IndexOf(Canvas.ActiveLayer);
        if (index > 0)
        {
            Canvas.Layers.Move(index, index - 1);
            StatusMessage = "Слой перемещён назад";
        }
    }
    
    /// <summary>
    /// Инициализирует режим ввода текста: создаёт предварительный TextViewModel и активирует ввод.
    /// Выводит подсказку о доступных клавишах (Enter для завершения, Esc для отмены).
    /// </summary>
    /// <param name="point">Точка вставки текста в координатах канваса.</param>
    private void StartTextInput(Point2D point)
    {
        IsDrawing = true;
        _currentDrawingTool = DrawingTool.Text;
        _drawingStartPoint = point;
        _hasDrawingStart = true;
    
        // Создаём временный текст для предпросмотра
        _previewFigure = new TextViewModel(
            point.X, point.Y,
            "",  // Пустой текст — ждём ввода
            24,
            "Segoe UI",
            StrokeColor.Color,
            FillColor.Color,
            Opacity / 100.0);
    
        Canvas?.AddFigure(_previewFigure);
        StatusMessage = "Введите текст (Enter для завершения, Esc для отмены)";
        // TopLevel.GetTopLevel(...)?.Activate();
    }

    /// <summary>
    /// Преобразует экранные координаты события мыши в координаты канваса с учётом зума и смещения.
    /// Учитывает текущие значения Canvas.Zoom, Canvas.OffsetX и Canvas.OffsetY.
    /// </summary>
    /// <param name="e">Аргументы события PointerEventArgs с экранными координатами.</param>
    /// <returns>Точка в координатах канваса типа Point2D.</returns>
    private Point2D GetCanvasPoint(PointerEventArgs e)
    {
        var screenPos = e.GetPosition((Avalonia.Visual?)e.Source);
        if (Canvas != null)
        {
            return new Point2D(
                (screenPos.X - Canvas.OffsetX) / Canvas.Zoom,
                (screenPos.Y - Canvas.OffsetY) / Canvas.Zoom
            );
        }
        return new Point2D(screenPos.X, screenPos.Y);
    }

    /// <summary>
    /// Удаляет указанный слой (если он не последний в коллекции).
    /// При удалении активного слоя переключает на соседний слой.
    /// </summary>
    private void DeleteLayer(LayerViewModel layer)
    {
        if (Canvas?.Layers.Count <= 1)
        {
            StatusMessage = "Нельзя удалить последний слой";
            return;
        }
    
        // Если удаляем активный слой — переключаем на другой
        if (Canvas.ActiveLayer == layer)
        {
            var index = Canvas.Layers.IndexOf(layer);
            Canvas.ActiveLayer = index > 0 ? Canvas.Layers[index - 1] : Canvas.Layers[1];
        }
    
        Canvas.Layers.Remove(layer);
        StatusMessage = $"Слой '{layer.Name}' удалён";
    }
    
    /// <summary>
    /// Переключает блокировку слоя: если заблокирован — разблокирует, и наоборот.
    /// Обновляет статус-сообщение и уведомляет об изменении коллекции слоёв.
    /// </summary>
    /// <param name="layer">Экземпляр LayerViewModel для изменения состояния блокировки.</param>
    private void ToggleLockLayer(LayerViewModel layer)
    {
	    DebugLog.Write($"[DEBUG] ToggleLockLayer: {layer.Name} -> {layer.IsLocked}");
        StatusMessage = layer.IsLocked 
            ? $"Слой '{layer.Name}' заблокирован" 
            : $"Слой '{layer.Name}' разблокирован";
	    Canvas?.RaisePropertyChanged(nameof(Canvas.Layers));
    }

    /// <summary>
    /// Переключает видимость слоя: если скрыт — показывает, и наоборот.
    /// Обновляет статус-сообщение и уведомляет об изменении коллекции слоёв.
    /// </summary>
    /// <param name="layer">Экземпляр LayerViewModel для изменения состояния видимости.</param>
    private void ToggleVisibilityLayer(LayerViewModel layer)
    {
        DebugLog.Write($"[DEBUG] ToggleVisibilityLayer: {layer.Name} -> {layer.IsVisible}");
        
        StatusMessage = layer.IsVisible 
            ? $"Слой '{layer.Name}' показан" 
            : $"Слой '{layer.Name}' скрыт";
	    Canvas?.RaisePropertyChanged(nameof(Canvas.Layers));
    }

    /// <summary>
    /// Дублирует активный слой с сохранением всех фигур через глубокое клонирование.
    /// Вставляет копию после текущего слоя и делает её активной.
    /// </summary>
    private void DuplicateLayer()
    {
        if (Canvas?.ActiveLayer == null) return;
        
        var source = Canvas.ActiveLayer;
        var duplicate = new LayerViewModel($"Копия {source.Name}")
        {
            IsVisible = source.IsVisible,
            IsLocked = source.IsLocked
        };
        
        // Копируем фигуры (глубокое клонирование)
        foreach (var figure in source.Figures)
        {
            var clone = figure.Clone();
            duplicate.Figures.Add(clone);
        }
        
        // Вставляем после текущего слоя
        var index = Canvas.Layers.IndexOf(source);
        Canvas.Layers.Insert(index + 1, duplicate);
        Canvas.ActiveLayer = duplicate;
        
        StatusMessage = $"Слой '{source.Name}' дублирован";
        DebugLog.Write($"[DEBUG] Layer duplicated: {source.Name} -> {duplicate.Name}");
    }

    /// <summary>
    /// Объединяет активный слой с предыдущим: переносит все фигуры и удаляет текущий слой.
    /// Делает предыдущий слой активным после объединения.
    /// </summary>
    private void MergeWithPreviousLayer()
    {
        if (Canvas?.ActiveLayer == null) return;
        
        var current = Canvas.ActiveLayer;
        var index = Canvas.Layers.IndexOf(current);
        
        if (index <= 0)
        {
            StatusMessage = "Нет предыдущего слоя для объединения";
            return;
        }
        
        var previous = Canvas.Layers[index - 1];
        
        // Переносим все фигуры из текущего в предыдущий
        foreach (var figure in current.Figures.ToList())
        {
            current.Figures.Remove(figure);
            previous.Figures.Add(figure);
        }
        
        // Удаляем текущий слой
        Canvas.Layers.Remove(current);
        Canvas.ActiveLayer = previous;
        
        StatusMessage = $"Слои '{current.Name}' и '{previous.Name}' объединены";
    }

    /// <summary>
    /// Перемещает активный слой на один уровень вверх (ближе к переднему плану отрисовки).
    /// Если слой уже на переднем плане — выводит соответствующее сообщение.
    /// </summary>
    private void BringLayerForward()
    {
        if (Canvas?.ActiveLayer == null) return;
        
        var layer = Canvas.ActiveLayer;
        var index = Canvas.Layers.IndexOf(layer);
        
        if (index < Canvas.Layers.Count - 1)
        {
            Canvas.Layers.Move(index, index + 1);
            StatusMessage = $"Слой '{layer.Name}' перемещён вверх";
        }
        else
        {
            StatusMessage = "Слой уже на переднем плане";
        }
    }
    
    /// <summary>
    /// Перемещает активный слой на один уровень вниз (ближе к заднему плану отрисовки).
    /// Если слой уже на заднем плане — выводит соответствующее сообщение.
    /// </summary>
    private void SendLayerBackward()
    {
        if (Canvas?.ActiveLayer == null) return;
        
        var layer = Canvas.ActiveLayer;
        var index = Canvas.Layers.IndexOf(layer);
        
        if (index > 0)
        {
            Canvas.Layers.Move(index, index - 1);
            StatusMessage = $"Слой '{layer.Name}' перемещён вниз";
        }
        else
        {
            StatusMessage = "Слой уже на заднем плане";
        }
    }

    /// <summary>
    /// Перемещает активный слой на самый передний план отрисовки (последний в коллекции).
    /// </summary>
    private void BringLayerToFront()
    {
        if (Canvas?.ActiveLayer == null) return;
        
        var layer = Canvas.ActiveLayer;
        var index = Canvas.Layers.IndexOf(layer);
        
        if (index < Canvas.Layers.Count - 1)
        {
            Canvas.Layers.Move(index, Canvas.Layers.Count - 1);
            StatusMessage = $"Слой '{layer.Name}' перемещён на передний план";
        }
    }

    /// <summary>
    /// Перемещает активный слой на самый задний план отрисовки (первый в коллекции).
    /// </summary>
    private void SendLayerToBack()
    {
        if (Canvas?.ActiveLayer == null) return;
        
        var layer = Canvas.ActiveLayer;
        var index = Canvas.Layers.IndexOf(layer);
        
        if (index > 0)
        {
            Canvas.Layers.Move(index, 0);
            StatusMessage = $"Слой '{layer.Name}' перемещён на задний план";
        }
    }

    /// <summary>
    /// Завершает ввод текста: создаёт финальную фигуру с введённым текстом или отменяет ввод.
    /// Удаляет предварительную фигуру и сбрасывает состояние рисования.
    /// </summary>
    public void FinishTextInput()
    {
        if (_previewFigure is TextViewModel text && !string.IsNullOrWhiteSpace(text.Text))
        {
            // Создаём финальную копию с введённым текстом
            var finalText = new TextViewModel(
                text.Vertices[0].X, text.Vertices[0].Y,
                text.Text,
                text.FontSize,
                text.FontFamily,
                text.LineColor,
                text.FillColor,
                text.Opacity);
            
            // Удаляем превью и добавляем финальный текст
            Canvas?.ActiveLayer?.Figures.Remove(_previewFigure);
            Canvas?.AddFigure(finalText);
            
            StatusMessage = "Текст добавлен";
        }
        else
        {
            // Если текст пустой — просто удаляем превью
            Canvas?.ActiveLayer?.Figures.Remove(_previewFigure);
            StatusMessage = "Ввод текста отменён (пустой)";
        }
        
        ResetDrawingState();
    }

    /// <summary>
    /// Отменяет ввод текста: удаляет предварительную фигуру и сбрасывает состояние рисования.
    /// Выводит сообщение об отмене операции.
    /// </summary>
    public void CancelTextInput()
    {
        Canvas?.ActiveLayer?.Figures.Remove(_previewFigure);
        ResetDrawingState();
        StatusMessage = "Ввод текста отменён";
    }
}