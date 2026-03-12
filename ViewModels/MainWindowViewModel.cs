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
    private bool _isDragging;
    private Point2D _dragStart;
    private Dictionary<Guid, List<(double X, double Y)>> _originalVertices; // для каждой фигуры список исходных координат вершин
    
    /// <summary>
    /// Публичный доступ к ViewModel истории для привязки в UI.
    /// </summary>
    public HistoryViewModel History => _history;

    // ========== ПОЛЯ ==========
    
    private readonly ObservableAsPropertyHelper<string> _coordinatesText; 
    /// <summary>Реактивное свойство для отображения текущих координат курсора в формате "X: .. Y: ..".</summary>
    
    private string _statusMessage = "Готово"; 
    /// <summary>Текст статуса для отображения в нижней панели окна.</summary>
    
    private DrawingTool _selectedTool = DrawingTool.Select; 
    /// <summary>Текущий выбранный инструмент рисования.</summary>
    
    /// <summary>
    /// Получает человеко-читаемое название выбранного инструмента.
    /// </summary>
    public string SelectedToolDisplayName => _selectedTool.ToDisplayName();
    
    private ColorViewModel _fillColor = new ColorViewModel(Color.FromArgb(255, 74, 144)); 
    /// <summary>Текущий выбранный цвет заливки для новых фигур.</summary>
    
    private ColorViewModel _strokeColor = new ColorViewModel(Color.Black); 
    /// <summary>Текущий выбранный цвет обводки для новых фигур.</summary>
    
    private ThemeVariant _currentTheme = ThemeVariant.Dark; 
    /// <summary>Текущая тема оформления интерфейса (светлая или тёмная).</summary>
    
    private Point2D _drawingStartPoint; 
    /// <summary>Точка начала операции рисования (координаты при нажатии мыши).</summary>
    
    private bool _hasDrawingStart; 
    /// <summary>Флаг, указывающий, была ли инициализирована точка начала рисования.</summary>
    
    private FigureViewModel? _previewFigure; 
    /// <summary>Предварительная фигура для визуализации в процессе рисования.</summary>
    
    private DrawingTool _currentDrawingTool; 
    /// <summary>Инструмент, который в данный момент используется для рисования.</summary>
    
    private List<Point2D> _penPoints = new(); 
    /// <summary>Коллекция точек для инструмента "Перо" (многоточечное рисование).</summary>
    
    private const double MinFigureSize = 5.0; 
    /// <summary>Минимальный допустимый размер фигуры для предотвращения создания микро-объектов.</summary>
    
    private const double DefaultZoomMin = 0.1; 
    /// <summary>Минимально допустимый коэффициент масштабирования канваса.</summary>
    
    private const double DefaultZoomMax = 10.0; 
    /// <summary>Максимально допустимый коэффициент масштабирования канваса.</summary>
    
    private bool _isSelectingArea; 
    /// <summary>Флаг, указывающий, что пользователь выполняет выделение областью (marquee selection).</summary>
    
    private Point2D _selectionStart; 
    /// <summary>Начальная точка выделения областью.</summary>
    
    private Point2D _selectionEnd; 
    /// <summary>Текущая конечная точка выделения областью (обновляется при движении мыши).</summary>
    
    /// <summary>
    /// Получает текущий активный инструмент рисования.
    /// </summary>
    private DrawingTool CurrentTool => _selectedTool;
    
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
    
    private bool _isColorPickerOpen; /// <summary>Флаг открытия палитры цветов для заливки.</summary>
    private bool _isStrokeColorPickerOpen; /// <summary>Флаг открытия палитры цветов для обводки.</summary>

    // ========== СВОЙСТВА ==========
    
    /// <summary>
    /// Текст сообщения статуса для отображения пользователю.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, выполняется ли в данный момент выделение областью.
    /// </summary>
    public bool IsSelectingArea
    {
        get => _isSelectingArea;
        set => this.RaiseAndSetIfChanged(ref _isSelectingArea, value);
    }
    
    /// <summary>
    /// Начальная точка выделения областью в координатах канваса.
    /// </summary>
    public Point2D SelectionStart
    {
        get => _selectionStart;
        set => this.RaiseAndSetIfChanged(ref _selectionStart, value);
    }
    
    /// <summary>
    /// Текущая конечная точка выделения областью (обновляется при движении мыши).
    /// </summary>
    public Point2D SelectionEnd
    {
        get => _selectionEnd;
        set => this.RaiseAndSetIfChanged(ref _selectionEnd, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, что пользователь находится в процессе рисования фигуры.
    /// </summary>
    public bool IsDrawing
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Предварительная фигура для визуального отображения в процессе рисования.
    /// </summary>
    public FigureViewModel? PreviewFigure
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Толщина линии обводки в пикселях для новых и выделенных фигур.
    /// </summary>
    public int StrokeWidth 
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Коэффициент непрозрачности фигур в процентах (0–100).
    /// </summary>
    public double Opacity
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Модель цвета заливки с поддержкой реактивных уведомлений.
    /// </summary>
    public ColorViewModel FillColor
    {
        get => _fillColor;
        set => this.RaiseAndSetIfChanged(ref _fillColor, value);
    }
    
    /// <summary>
    /// Модель цвета обводки с поддержкой реактивных уведомлений.
    /// </summary>
    public ColorViewModel StrokeColor
    {
        get => _strokeColor;
        set => this.RaiseAndSetIfChanged(ref _strokeColor, value);
    }
    
    /// <summary>
    /// Текущая тема оформления приложения (светлая или тёмная).
    /// </summary>
    public ThemeVariant CurrentTheme
    {
        get => _currentTheme;
        set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
    }
    
    /// <summary>
    /// Текущая координата X курсора мыши в координатах канваса.
    /// </summary>
    public double MouseX
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Текущая координата Y курсора мыши в координатах канваса.
    /// </summary>
    public double MouseY
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, открыта ли палитра выбора цвета заливки.
    /// </summary>
    public bool IsColorPickerOpen
    {
        get => _isColorPickerOpen;
        set => this.RaiseAndSetIfChanged(ref _isColorPickerOpen, value);
    }
    
    /// <summary>
    /// Флаг, указывающий, открыта ли палитра выбора цвета обводки.
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
            OpenStrokeColorPickerCommand: ReactiveCommand.Create(() => { IsStrokeColorPickerOpen = true; })
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
    /// </summary>
    /// <param name="toolName">Строковое название инструмента.</param>
    public void SetToolByName(string toolName)
    {
        if (DrawingToolExtensions.TryParse(toolName, out var tool))
        {
            SetTool(tool);
        }
    }
    
    /// <summary>
    /// Устанавливает активный инструмент рисования и сбрасывает состояние рисования при смене.
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
        this.RaisePropertyChanged(nameof(SelectedToolDisplayName));
        StatusMessage = $"Установлен инструмент: {SelectedToolDisplayName}";
    }

    /// <summary>
    /// Перемещает выделенные фигуры на заданный вектор и добавляет действие в историю.
    /// </summary>
    /// <param name="dx">Смещение по оси X.</param>
    /// <param name="dy">Смещение по оси Y.</param>
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
    /// Требует выделения минимум двух фигур.
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
    /// Разгруппировывает выбранную группу, добавляя её дочерние фигуры обратно на слой.
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
    /// </summary>
    /// <param name="apply">Действие, изменяющее свойства фигуры (цвет, толщина и т.д.).</param>
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
    /// Добавляет пентаграмму (пятиконечную звезду) с текущими настройками стиля на активный слой.
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
    /// Удаляет все выделенные фигуры и добавляет действие в историю.
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
    /// Дублирует выбранную фигуру со смещением (10, 10) и добавляет в историю.
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
    /// Выполняет вертикальное отражение выделенных фигур.
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
    /// Выполняет горизонтальное отражение выделенных фигур.
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
    /// Обновляет отображаемые координаты курсора мыши.
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
    /// Создаёт новый слой и делает его активным для рисования.
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
    /// </summary>
    /// <param name="e">Аргументы события PointerPressedEventArgs.</param>
    public void HandlePointerPressed(PointerPressedEventArgs e)
    {
        CanvasPointerPressed(e);
    }
    
    /// <summary>
    /// Публичный обработчик события перемещения мыши над канвасом.
    /// </summary>
    /// <param name="e">Аргументы события PointerEventArgs.</param>
    public void HandlePointerMoved(PointerEventArgs e)
    {
        CanvasPointerMoved(e);
    }
    
    /// <summary>
    /// Публичный обработчик события отпускания кнопки мыши на канвасе.
    /// </summary>
    /// <param name="e">Аргументы события PointerReleasedEventArgs.</param>
    public void HandlePointerReleased(PointerReleasedEventArgs e)
    {
        CanvasPointerReleased(e);
    }

    private bool s_area = false;
    private Point2D s_start = null;
    private Point2D s_end = null;
    /// <summary>
    /// Обрабатывает нажатие кнопки мыши: начинает рисование, выделение или выбор фигуры.
    /// </summary>
    /// <param name="e">Аргументы события PointerPressedEventArgs.</param>
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
    }

    /// <summary>
    /// Инициализирует режим рисования пером: создаёт первую точку и предварительную фигуру.
    /// </summary>
    /// <param name="startPoint">Точка начала рисования.</param>
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
    /// </summary>
    /// <param name="e">Аргументы события PointerEventArgs.</param>
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
    /// </summary>
    /// <param name="e">Аргументы события PointerReleasedEventArgs.</param>
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
    
    public class DragMoveCommand : IHistoryAction
    {
        private readonly List<Guid> _figureIds;
        private readonly Dictionary<Guid, List<(double X, double Y)>> _originalVertices;
        private readonly Point2D _delta;
        private CanvasViewModel? _canvas;

        public string Description => "Перемещение";

        public DragMoveCommand(List<Guid> figureIds, Dictionary<Guid, List<(double X, double Y)>> originalVertices, Point2D delta)
        {
            _figureIds = figureIds;
            _originalVertices = originalVertices;
            _delta = delta;
        }

        public void SetCanvas(CanvasViewModel canvas) => _canvas = canvas;

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

        private FigureViewModel? FindFigure(Guid id) =>
            _canvas?.Layers.SelectMany(l => l.Figures).FirstOrDefault(f => f.Id == id);
    }

    /// <summary>
    /// Выделяет все фигуры, полностью попавшие в прямоугольную область выделения.
    /// </summary>
    /// <param name="start">Начальная точка области выделения.</param>
    /// <param name="end">Конечная точка области выделения.</param>
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
    /// </summary>
    /// <param name="startPoint">Точка начала рисования.</param>
    /// <param name="tool">Инструмент для рисования.</param>
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
    /// </summary>
    /// <param name="endPoint">Конечная точка рисования.</param>
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
    /// Добавляет новую точку в режиме рисования пером.
    /// </summary>
    /// <param name="point">Координаты добавляемой точки.</param>
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
    /// </summary>
    /// <param name="point">Новые координаты для предварительной точки.</param>
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
            _ => null
        };
        if (figure != null)
            ApplyStyle(figure);
        return figure;
    }

    /// <summary>
    /// Сбрасывает все флаги и поля, связанные с режимом рисования.
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
            case RegularPolygonViewModel polygon:
                polygon.UpdateVertices(center, radius);
                break;
            case PentagramViewModel star:
                star.UpdateVertices(center, radius);
                break;
            case TriangleViewModel triangle:
                UpdatePolygonBoundingBox(triangle, start, end);
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

    /// <summary>
    /// Преобразует экранные координаты события мыши в координаты канваса с учётом зума и смещения.
    /// </summary>
    /// <param name="e">Аргументы события PointerEventArgs.</param>
    /// <returns>Точка в координатах канваса.</returns>
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
}