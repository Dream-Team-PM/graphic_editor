// ViewModels/MainWindowViewModel.cs

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

namespace graphic_editor.ViewModels;

/// <summary>
/// ViewModel для главного окна графического редактора "Магический графический редактор".
/// Управляет состоянием UI, видимостью экранов и игровой логикой.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    // ========== ПОЛЯ ==========
    private readonly ObservableAsPropertyHelper<string> _coordinatesText; /// <summary>Приватное свойство - текст координат.</summary>
	private string _statusMessage = "Готово"; /// <summary>Приватное свойство - статус выполнения.</summary>
	private DrawingTool _selectedTool = DrawingTool.Select; /// <summary>Публичное свойство - выбранный инструмент.</summary>
	public string SelectedToolDisplayName => _selectedTool.ToDisplayName(); /// <summary>Публичное свойство - строковое представление выбранного инструмента.</summary>
	private ColorViewModel _fillColor = new ColorViewModel(Color.FromArgb(255, 74, 144)); /// <summary>Приватное свойство - цвет заполнения.</summary>
	private ColorViewModel _strokeColor = new ColorViewModel(Color.Black); /// <summary>Приватное свойство - цвет обводки.</summary>
	private ThemeVariant _currentTheme = ThemeVariant.Dark; /// <summary>Приватное свойство - текущая тема (светлая или темная).</summary>
	private Point2D _drawingStartPoint; /// <summary>Приватная точка - точка начала отрисовки.</summary>
	private bool _hasDrawingStart; /// <summary>Приватный флаг - проверка начала отрисовки.</summary>
	private FigureViewModel? _previewFigure; /// <summary>Приватный флаг - предварительная отрисовка фигуры.</summary>
	private DrawingTool _currentDrawingTool; /// <summary>Приватное свойство - текущий инструмент для рисования.</summary>
	private List<Point2D> _penPoints = new(); /// <summary>Приватное коллекция точек для отрисовки.</summary>
	private const double MinFigureSize = 5.0; /// <summary>Минимальный размер фигуры.</summary>
	//private const double PenPointRadius = 3.0;
	private const double DefaultZoomMin = 0.1; /// <summary>Дефолтное значение минимума зума.</summary>
	private const double DefaultZoomMax = 10.0; /// <summary>Дефолтное значение максимума зума.</summary>
	private bool _isSelectingArea;  /// <summary>Приватное свойство - флаг выделения области.</summary>
	private Point2D _selectionStart;  /// <summary>Приватное свойство - начало выделения.</summary>
	private Point2D _selectionEnd;  /// <summary>Приватное свойство - конец выделения (текущая позиция мыши).</summary>
	private DrawingTool CurrentTool => _selectedTool; /// <summary>Приватное свойство текущего инструмента.</summary>
	public EditorCommands Commands { get; } /// <summary>Связь команд в Reactive UI c обычными командами.</summary>
	public CanvasViewModel Canvas { get; } /// <summary>Публичный канвас.</summary>
	public string CoordinatesText => _coordinatesText.Value; /// <summary>Публичное свойство - значения координат.</summary>
	public bool HasSelection => Canvas?.HasSelection ?? false; /// <summary>Публичное свойство - проверка выбранности канваса.</summary>
	private bool _isColorPickerOpen;
	private bool _isStrokeColorPickerOpen;
    // ========== СВОЙСТВА ==========
	/// <summary>Публичное свойство - текст статуса.</summary>
	public string StatusMessage
	{
		get => _statusMessage;
		set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
	}
	/// <summary>Публичное свойство - флаг выбранности зоны.</summary>
    public bool IsSelectingArea
    {
	    get => _isSelectingArea;
	    set => this.RaiseAndSetIfChanged(ref _isSelectingArea, value);
    }
	/// <summary>Публичное свойство - начало выделения.</summary>
    public Point2D SelectionStart
    {
	    get => _selectionStart;
	    set => this.RaiseAndSetIfChanged(ref _selectionStart, value);
    }
	/// <summary>Публичное свойство - конец выделения (текущая позиция мыши).</summary>
    public Point2D SelectionEnd
    {
	    get => _selectionEnd;
	    set => this.RaiseAndSetIfChanged(ref _selectionEnd, value);
    }
	
	/// <summary>Публичное свойство установки статуса рисования.</summary>
	public bool IsDrawing
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	/// <summary>Публичное свойство для отображения отрисовываемой фигуры.</summary>
	public FigureViewModel? PreviewFigure
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	
	/// <summary>Публичное свойство толщины линии.</summary>
	public int StrokeWidth {
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	/// <summary>Публичное свойство степени прозрачности.</summary>
	public double Opacity
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	/// <summary>Публичное свойство цвета заполнения.</summary>
	public ColorViewModel FillColor
	{
		get => _fillColor;
		set => this.RaiseAndSetIfChanged(ref _fillColor, value);
	}

	/// <summary>Публичное свойство цвета обводки.</summary>
	public ColorViewModel StrokeColor
	{
		get => _strokeColor;
		set => this.RaiseAndSetIfChanged(ref _strokeColor, value);
	}

	/// <summary>Публичное свойство выбора темы.</summary>
	public ThemeVariant CurrentTheme
	{
		get => _currentTheme;
		set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
	}

	/// <summary>Публичное свойство установки позиции мыши по Ox.</summary>
	public double MouseX
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	/// <summary>Публичное свойство установки позиции мыши по Oy.</summary>
	public double MouseY
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
	
	/// <summary>Публичное свойство проверки открытия палитры в меню заполнения.</summary>
	public bool IsColorPickerOpen
	{
		get => _isColorPickerOpen;
		set => this.RaiseAndSetIfChanged(ref _isColorPickerOpen, value);
	}
	/// <summary>Публичное свойство проверки открытия палитры в меню цвета линии.</summary>
	public bool IsStrokeColorPickerOpen
	{
		get => _isStrokeColorPickerOpen;
		set => this.RaiseAndSetIfChanged(ref _isStrokeColorPickerOpen, value);
	}
	
	private void ApplyStyle<T>(T figure, bool solidFill = false) where T : FigureViewModel
	{
		figure.LineColor = StrokeColor.Color;
		figure.FillColor = solidFill ? StrokeColor.Color : FillColor.Color;
		figure.Thickness = StrokeWidth;
	}

    // ========== КОНСТРУКТОР ==========
	public MainWindowViewModel() 
    {
        Canvas = new CanvasViewModel();
        SetTool(DrawingTool.Select);
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
	        ZoomIn: ReactiveCommand.Create(ZoomIn),
	        ZoomOut: ReactiveCommand.Create(ZoomOut),
	        ZoomFit: ReactiveCommand.Create(ZoomFit),
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
        _coordinatesText = this
            .WhenAnyValue(x => x.MouseX, x => x.MouseY)
            .Select(_ => $"X: {MouseX:F1}  Y: {MouseY:F1}")
            .ToProperty(this, x => x.CoordinatesText);
        this.WhenAnyValue(x => x.Canvas.SelectedFigure)
	        .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelection)));
        this.WhenAnyValue(x => x.StrokeColor.Color)
	        .Subscribe(color => ApplyStyleToSelected(f => f.LineColor = color));
        this.WhenAnyValue(x => x.FillColor.Color)
	        .Subscribe(color => ApplyStyleToSelected(f => f.FillColor = color));
        this.WhenAnyValue(x => x.StrokeWidth)
	        .Subscribe(thickness => ApplyStyleToSelected(f => f.Thickness = thickness));
        this.WhenAnyValue(x => x.Opacity)
	        .Subscribe(opacity => ApplyStyleToSelected(f => f.Opacity = opacity / 100.0));
    }
		
	/// <summary>Публичный метод установки инструмента по имени (из Tag кнопки)</summary>
	public void SetToolByName(string toolName)
	{
		if (DrawingToolExtensions.TryParse(toolName, out var tool))
		{
			SetTool(tool);
		}
	}

	/// <summary>Публичный метод установки инструмента (enum)</summary>
	public void SetTool(DrawingTool tool)
	{
		// Если были в режиме рисования и меняем инструмент — сбрасываем
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
	
	private void MoveSelected(double dx, double dy)
	{
		if (Canvas?.SelectedFigures?.Any() == true)
		{
			foreach (var figure in Canvas.SelectedFigures)
			{
				figure.Move(dx, dy);
			}
			StatusMessage = $"Перемещено на ({dx}, {dy})";
		}
	}
	
	/// <summary>Приватная функция для группировки выделенных фигур.</summary>
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
    
		// Удаляем старые фигуры из слоя
		foreach (var figure in figuresToGroup)
		{
			activeLayer.Figures.Remove(figure);
		}
		activeLayer.Figures.Add(group);
    
		// Добавляем группу
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
	
	/// <summary>Приватная функция для разгруппировки выбранной группы.</summary>
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
		// Разгруппировываем
		var children = group.Ungroup();
		// Удаляем группу
		activeLayer.Figures.Remove(group);
		// Добавляем детей
		foreach (var child in children)
		{
			activeLayer.Figures.Add(child);
		}
		Canvas.SelectedFigures.Clear();
		Canvas.SelectedFigure = null;
		StatusMessage = $"Группа разгруппирована на {children.Count()} фигур";
		DebugLog.Write($"Группа разгруппирована на {children.Count()} фигур");
	}
    
	/// <summary>Приватная функция для применения стиля к выделенным фигурам.</summary>
	/// <param name="apply">Действие, применяемое к фигуре (изменение цвета, толщины и т.д.).</param>
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
    }
    
    private async Task SaveAsync()
    {
	    StatusMessage = "Сохранение...";
	    try
	    {
		    // TODO: реальная реализация
		    await Task.Delay(100); // имитация
		    DebugLog.Write("Файл сохранён");
		    StatusMessage = "Файл сохранён ✓";
	    }
	    catch (Exception ex)
	    {
		    DebugLog.Write("Ошибка сохранения");
		    StatusMessage = "Ошибка сохранения ✗";
	    }
    }
    /// Функции для добавления примитивов.
	/// <summary>Приватная функция для добавления квадрата.</summary>
	private void AddSquare()
	{
		var sq = new SquareViewModel(100, 100, 150, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(sq);
		Canvas?.AddFigure(sq);
		StatusMessage = "Добавлен квадрат";
	}
	/// <summary>Приватная функция для добавления прямоугольника.</summary>
    private void AddRectangle()
    {
        var rect = new RectangleViewModel(100, 100, 150, 100, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(rect);
        Canvas?.AddFigure(rect);
        StatusMessage = "Добавлен прямоугольник";
    }
	
	/// <summary>Приватная функция для добавления круга.</summary>
	private void AddCircle()
	{
		var circle = new CircleViewModel(100, 100, 150, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(circle);
		Canvas?.AddFigure(circle);
		StatusMessage = "Добавлен круг";
	}
	
	/// <summary>Приватная функция для добавления эллипса.</summary>
    private void AddEllipse()
    {
	    var ellipse = new EllipseViewModel(100, 100, 150, 100, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
	    ApplyStyle(ellipse);
        Canvas?.AddFigure(ellipse);
        StatusMessage = "Добавлен эллипс";
    }
	
	/// <summary>Приватная функция для добавления линии.</summary>	
    private void AddLine()
    {
        var line = new LineViewModel(100, 100, 300, 300, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        Canvas?.AddFigure(line);
        StatusMessage = "Добавлена линия";
    }
	
	/// <summary>Приватная функция для добавления пятиугольника.</summary>	
	private void AddPentagon()
	{
		var pentagon = new PentagonViewModel(
			new Point2D(200, 200), 75,
			StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(pentagon);
		Canvas?.AddFigure(pentagon);
		StatusMessage = "Добавлен пятиугольник";
	}

	/// <summary>Приватная функция для добавления шестиугольника.</summary>	
	private void AddHexagon()
	{
		var hexagon = new HexagonViewModel(
			new Point2D(200, 200), 75,
			StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(hexagon);
		Canvas?.AddFigure(hexagon);
		StatusMessage = "Добавлен шестиугольник";
	}
	
	private void AddHeptagon()
	{
		var heptagon = new HeptagonViewModel(
			new Point2D(200, 200), 75,
			StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(heptagon);
		Canvas?.AddFigure(heptagon);
		StatusMessage = "Добавлен семиугольник";
	}
	
	private void AddOctagon()
	{
		var octagon = new OctagonViewModel(
			new Point2D(200, 200), 75,
			StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(octagon);
		Canvas?.AddFigure(octagon);
		StatusMessage = "Добавлен восьмиугольник";
	}
	
	private void AddTriangle()
	{
		var triangle = new TriangleViewModel(
			new Point2D(200, 200), new Point2D(100, 200), new Point2D(200, 100),
			StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(triangle);
		Canvas?.AddFigure(triangle);
		StatusMessage = "Добавлен треугольник";
	}
	
	private void AddPentagram()
	{
		var pentagram = new PentagramViewModel(
			new Point2D(200, 200), 50,
			StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
		ApplyStyle(pentagram);
		Canvas?.AddFigure(pentagram);
		StatusMessage = "Добавлена пентаграмма";
	}

	/// <summary>Приватная функция для удаления выбранных фигур.</summary>	
	private void DeleteSelected()
    {
        Canvas?.RemoveSelectedFigure();
        StatusMessage = "Объект удалён";
    }

	/// <summary>Приватная функция для дубликации выбранных фигур.</summary>
    private void DuplicateSelected()
    {
        Canvas?.DuplicateSelectedFigure();
        StatusMessage = "Объект дублирован";
    }

	/// <summary>Приватная функция для вращения фигуры влево.</summary>
    private void RotateLeft()
    {
        Canvas?.RotateSelectedFigure(-90);
        StatusMessage = "Поворот на -90°";
    }

	/// <summary>Приватная функция для вращения фигуры вправо.</summary>
    private void RotateRight()
    {
        Canvas?.RotateSelectedFigure(90);
        StatusMessage = "Поворот на 90°";
    }
	
	/// <summary>Приватная функция для вращения фигуры на 180 градусов.</summary>
	private void RotateFull()
	{
		Canvas?.RotateSelectedFigure(180);
		StatusMessage = "Поворот на 180°";
	}

	/// <summary>Приватная функция для приближения на холсте.</summary>
    private void ZoomIn()
    {
        if (Canvas != null)
        {
            Canvas.Zoom *= 1.5;
            StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
        }
    }

	/// <summary>Приватная функция для отдаления на холсте.</summary>
    private void ZoomOut()
    {
        if (Canvas != null)
        {
            Canvas.Zoom *= 0.5;
            StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
        }
    }

	/// <summary>Приватная функция для зума по размеру окна.</summary>
    private void ZoomFit()
    {
        if (Canvas != null) {
            Canvas.Zoom = 1.0;
            StatusMessage = "Масштаб: по размеру окна";
        }
    }

	/// <summary>Приватная функция для выбора темы.</summary>
    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeVariant.Dark 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        StatusMessage = $"Тема: {(CurrentTheme == ThemeVariant.Light ? "Светлая ☀️" : "Тёмная 🌙")}";
    }

	/// <summary>Приватная функция для обновления координат курсора.</summary>
	/// <param name="coords">Кортеж с координатами (X, Y).</param>
	private void UpdateCoordinates((double x, double y) coords) 
    {
		MouseX = coords.x;
		MouseY = coords.y;
        this.RaisePropertyChanged(nameof(CoordinatesText));
	}

	/// <summary>Приватная функция для активации канваса.</summary>
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

	/// <summary>Приватная функция для сохранения состояния (не реализована).</summary>
    private void Save()
    {
        StatusMessage = "Сохранение...";
        // TODO: Реализовать сохранение
    }

	/// <summary>Приватная функция для открытия и загрузки изображения (не реализована).</summary>
    private void Open()
    {
        StatusMessage = "Открытие файла...";
        // TODO: Реализовать открытие
    }

	/// <summary>Приватная функция для экспорта изображения (не реализована).</summary>
    private void Export()
    {
        StatusMessage = "Экспорт...";
        // TODO: Реализовать экспорт
    }

	/// <summary>Приватная функция для создания нового слоя по кнопке.</summary>
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

	/// <summary>Обработчик события при нажатии.</summary>
	public void HandlePointerPressed(PointerPressedEventArgs e)
	{
    	CanvasPointerPressed(e);
	}

	/// <summary>Обработчик события при перемещении.</summary>
	public void HandlePointerMoved(PointerEventArgs e)
	{
    	CanvasPointerMoved(e);
	}

	/// <summary>Обработчик события при реализации действия.</summary>
	public void HandlePointerReleased(PointerReleasedEventArgs e)
	{
    	CanvasPointerReleased(e);
	}

	/// <summary>Приватная функция - нажатие на канвас.</summary>
	private void CanvasPointerPressed(PointerPressedEventArgs e)
    {
        if (Canvas == null) return;
        var point = GetCanvasPoint(e);
        DebugLog.Write($"[DEBUG] PointerPressed at {point}, Tool={CurrentTool.ToDisplayName()}, IsDrawing={IsDrawing}");
        if (CurrentTool.IsPrimitive())
        {
			if (!IsDrawing || _currentDrawingTool != CurrentTool)
        	{
            	// Если были в режиме пера, сначала сбрасываем
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
	        else if (_currentDrawingTool == DrawingTool.Pen) // Добавляем точку только если рисуем пером
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
	        if (figure == null)
	        {
		        // Начинаем выделение областью
		        _isSelectingArea = true;
		        _selectionStart = point;
		        _selectionEnd = point;
		        this.RaisePropertyChanged(nameof(IsSelectingArea));
		        this.RaisePropertyChanged(nameof(SelectionStart));
		        this.RaisePropertyChanged(nameof(SelectionEnd));
		        DebugLog.Write($"Начато выделение областью");
	        }
	        else
	        {
		        // Клик на фигуре — обычное выделение
		        var addToSelection = e.KeyModifiers.HasFlag(KeyModifiers.Control);
		        Canvas.SelectFigureAt(point, addToSelection);
		        DebugLog.Write($"Объект {HasSelection} и addToSelection =  {addToSelection}");
		        StatusMessage = HasSelection ? "Объект выделен" : "Выделение снято";
	        }
	        
        }
    }

	/// <summary>Приватная функция - начало отрисовки точек.</summary>
	private void StartPenDrawing(Point2D startPoint)
	{
    	IsDrawing = true;
    	_currentDrawingTool = DrawingTool.Pen;
    	_penPoints.Clear();
    	_penPoints.Add(startPoint);
    	
    	// Создаем первую точку (она остается на холсте)
	    var firstPoint = new PenPointViewModel(startPoint.X, startPoint.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
	    ApplyStyle(firstPoint, solidFill: true);
    	Canvas?.AddFigure(firstPoint);
    	
    	// Создаем предварительную точку для следующего клика
	    _previewFigure = new PenPointViewModel(startPoint.X, startPoint.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
	    ApplyStyle(_previewFigure, solidFill: true);
    	Canvas?.AddFigure(_previewFigure);
    	_drawingStartPoint = startPoint;
    	_hasDrawingStart = true;
    	StatusMessage = "Рисование пером: кликайте для добавления точек (Enter для завершения)";
	}

	/// <summary>Приватная функция - перемещение по канвасу.</summary>
    private void CanvasPointerMoved(PointerEventArgs e)
	{
    	if (Canvas == null) return;
    
    	var point = GetCanvasPoint(e);
    	UpdateCoordinates((point.X, point.Y));

    	// Используем HasValue для nullable типа
    	if (IsDrawing && _hasDrawingStart && _previewFigure != null)
    	{
        	if (_currentDrawingTool == DrawingTool.Pen)
            {
                // В режиме пера показываем предварительную точку
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
                // Для примитивов обновляем предварительную фигуру
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

	/// <summary>Реализация действия на канвасе.</summary>
	private void CanvasPointerReleased(PointerReleasedEventArgs e)
	{
    	if (Canvas == null) return;
    
    	var point = GetCanvasPoint(e);
    	DebugLog.Write($"[DEBUG] PointerReleased at {point}, IsDrawing={IsDrawing}");

    	// Используем HasValue и Value для nullable типа
    	if (IsDrawing && _hasDrawingStart && CurrentTool.IsPrimitive())
    	{
       	 	FinishDrawingPrimitive(point);
        	e.Handled = true;
    	}
	    if (CurrentTool == DrawingTool.Select && _isSelectingArea)
	    {
		    _isSelectingArea = false;
		    this.RaisePropertyChanged(nameof(IsSelectingArea));
    
		    // 🔥 Выделяем все фигуры в прямоугольнике
		    SelectFiguresInArea(_selectionStart, _selectionEnd);
    
		    e.Handled = true;
	    }
	}
	
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
    
		// Выделяем найденные фигуры
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

	/// <summary>Приватный метод для начала рисования.</summary>
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

	/// <summary>Приватный метод для окончания отрисовки примитива.</summary>
	private void FinishDrawingPrimitive(Point2D endPoint)
    {
        var start = _drawingStartPoint;
        var end = endPoint;

        // Удаляем предварительную фигуру
        if (_previewFigure != null && Canvas.ActiveLayer != null)
        {
            Canvas.ActiveLayer.Figures.Remove(_previewFigure);
        }
        
        // Проверяем минимальный размер
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

	/// <summary>Приватный метод для добавления точки.</summary>
	private void AddPenPoint(Point2D point)
    {
        // Добавляем точку в коллекцию
        _penPoints.Add(point);
		if (_previewFigure != null && Canvas.ActiveLayer != null)
    	{
        	Canvas.ActiveLayer.Figures.Remove(_previewFigure);
    	}
        
        // Создаем и добавляем точку на канвас
        var penPoint = new PenPointViewModel(point.X, point.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(penPoint, solidFill: true);
        
        Canvas?.AddFigure(penPoint);
        _previewFigure = new PenPointViewModel(point.X, point.Y, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(_previewFigure, solidFill: true);
    	Canvas?.AddFigure(_previewFigure);
        StatusMessage = $"Точка {_penPoints.Count}: ({point.X:F0}, {point.Y:F0})";
        
        // Обновляем предварительный просмотр
        _drawingStartPoint = point; // Для предварительного просмотра следующей точки
    }

	/// <summary>Приватный метод обновления для новой точки.</summary>
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

	/// <summary>Приватный метод окончания рисования точек.</summary>
    private void FinishPenDrawing()
    {
        if (_previewFigure != null && Canvas.ActiveLayer != null)
        {
            Canvas.ActiveLayer.Figures.Remove(_previewFigure);
        }
        
        StatusMessage = $"Рисование пером завершено. Всего точек: {_penPoints.Count}";
        ResetDrawingState();
    }

	/// <summary>Приватный метод создания финальной фигуры.</summary>
	private FigureViewModel? CreateFinalFigure(Point2D start, Point2D end, DrawingTool tool)
    {
	    var size = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
	    var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
	    var radius = size / 2;
	    FigureViewModel? figure = tool switch
	    {
		    DrawingTool.Line => new LineViewModel(
			    start.X, start.Y, end.X, end.Y,
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
		    DrawingTool.Square => new SquareViewModel(
			    Math.Min(start.X, end.X),
			    Math.Min(start.Y, end.Y),
			    size,  // ← ширина = size
			    size,  // ← высота = size (такая же!) 
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
		    DrawingTool.Circle => new CircleViewModel(
			    Math.Min(start.X, end.X),  // левый-верхний угол
			    Math.Min(start.Y, end.Y),
			    size,  // ← ширина = size
			    size,  // ← высота = size (такая же!)
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
		    DrawingTool.Rectangle => new RectangleViewModel(
			    Math.Min(start.X, end.X),
			    Math.Min(start.Y, end.Y),
			    Math.Abs(end.X - start.X),
			    Math.Abs(end.Y - start.Y),
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
		    DrawingTool.Ellipse => new EllipseViewModel(
			    Math.Min(start.X, end.X),
			    Math.Min(start.Y, end.Y),
			    Math.Abs(end.X - start.X),
			    Math.Abs(end.Y - start.Y),
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
		    DrawingTool.Pentagon => new PentagonViewModel(
			    center, radius,
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
		    DrawingTool.Hexagon => new HexagonViewModel(
			    center, radius,
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
		    DrawingTool.Octagon => new OctagonViewModel(
			    center, radius,
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
		    DrawingTool.Heptagon => new HeptagonViewModel(
			    center, radius,
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
		    DrawingTool.Pentagram => new PentagramViewModel(
			    center, radius,
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
		    DrawingTool.Triangle => new TriangleViewModel(
			    new Point2D(center.X, center.Y - radius),           // Верх
			    new Point2D(center.X - radius, center.Y + radius),  // Лево-низ
			    new Point2D(center.X + radius, center.Y + radius),  // Право-низ
			    StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
		    _ => null
	    };
	    if (figure != null)
		    ApplyStyle(figure);
    
	    return figure;
    }

	/// <summary>Приватный метод сброса состояния рисования.</summary>
    private void ResetDrawingState()
    {
        IsDrawing = false;
        _hasDrawingStart = false;
        _previewFigure = null;
        _currentDrawingTool = default;
		_penPoints.Clear();
    }

	/// <summary>Приватный метод создания отображаемой фигуры.</summary>
	private FigureViewModel? CreatePreviewFigure(Point2D start, Point2D end, DrawingTool tool)
	{
		var size = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
		var center = new Point2D((start.X + end.X) / 2, (start.Y + end.Y) / 2);
		var radius = size / 2;
		FigureViewModel? figure = tool switch
		{
			DrawingTool.Line => new LineViewModel(
				start.X, start.Y, end.X, end.Y,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
			
			DrawingTool.Square => new SquareViewModel(
				Math.Min(start.X, end.X),
				Math.Min(start.Y, end.Y),
				size, size,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
			DrawingTool.Circle => new CircleViewModel(
				Math.Min(start.X, end.X),
				Math.Min(start.Y, end.Y),
				size, size,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
			DrawingTool.Rectangle => new RectangleViewModel(
				Math.Min(start.X, end.X),
				Math.Min(start.Y, end.Y),
				Math.Abs(end.X - start.X),
				Math.Abs(end.Y - start.Y),
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
			DrawingTool.Ellipse => new EllipseViewModel(
				Math.Min(start.X, end.X),
				Math.Min(start.Y, end.Y),
				Math.Abs(end.X - start.X),
				Math.Abs(end.Y - start.Y),
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
			
			DrawingTool.Pen => new PenPointViewModel(start.X, start.Y,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
			
			DrawingTool.Pentagon => new PentagonViewModel(
				center, radius,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
			DrawingTool.Hexagon => new HexagonViewModel(
				center, radius,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
			DrawingTool.Octagon => new OctagonViewModel(
				center, radius,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
			DrawingTool.Heptagon => new HeptagonViewModel(
				center, radius,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
			DrawingTool.Pentagram => new PentagramViewModel(
				center, radius,
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
		    
			DrawingTool.Triangle => new TriangleViewModel(
				new Point2D(center.X, center.Y - Math.Max(1, radius)),           
				new Point2D(center.X - Math.Max(1, radius), center.Y + Math.Max(1, radius)),  
				new Point2D(center.X + Math.Max(1, radius), center.Y + Math.Max(1, radius)),  
				StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0),
        
			_ => null
		};
    
		// Применяем стиль, если фигура создана
		if (figure != null)
		{
			if (tool == DrawingTool.Pen)
			{
				ApplyStyle(figure, solidFill: true);
			}
			else
			{
				// Для остальных фигур: стандартный стиль
				ApplyStyle(figure);
			}
		}
    
		return figure;
	}

	/// <summary>Приватный метод обновления отображаемой фигуры.</summary>
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
	            rect.Vertices[2].Y = rect.Vertices[0].Y + size;  // ← size, не height!
	            rect.Vertices[3].X = rect.Vertices[0].X;
	            rect.Vertices[3].Y = rect.Vertices[0].Y + size;
	            rect.RaisePropertyChanged(nameof(RectangleViewModel.X));
	            rect.RaisePropertyChanged(nameof(RectangleViewModel.Y));
	            rect.RaisePropertyChanged(nameof(RectangleViewModel.Width));
	            rect.RaisePropertyChanged(nameof(RectangleViewModel.Height));
	            break;
                
            case RectangleViewModel rect:
                // Обновляем вершины прямоугольника
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
	            ellipse.Vertices[2].Y = ellipse.Vertices[0].Y + size;  // ← size, не height!
	            ellipse.Vertices[3].X = ellipse.Vertices[0].X;
	            ellipse.Vertices[3].Y = ellipse.Vertices[0].Y + size;
	            ellipse.RaisePropertyChanged(nameof(EllipseViewModel.X));
	            ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Y));
	            ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Width));
	            ellipse.RaisePropertyChanged(nameof(EllipseViewModel.Height));
	            break;
                
            case EllipseViewModel ellipse:
                // Обновляем ограничивающий прямоугольник эллипса
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
            
            // ✅ Правильные многоугольники (пяти-, шести-, семи-, восьмиугольник)
            case RegularPolygonViewModel polygon:
	            polygon.UpdateVertices(center, radius);
	            break;
        
            // ✅ Пентаграмма (звезда)
            case PentagramViewModel star:
	            star.UpdateVertices(center, radius);
	            break;
        
            // ✅ Произвольный треугольник — масштабируем bounding box
            case TriangleViewModel triangle:
	            UpdatePolygonBoundingBox(triangle, start, end);
	            break;
        }
    }
	
	/// <summary>Вспомогательный метод для масштабирования bounding box полигона</summary>
	/// <summary>Вспомогательный метод для масштабирования bounding box полигона</summary>
	private void UpdatePolygonBoundingBox(PolygonViewModel polygon, Point2D start, Point2D end)
	{
		var minX = polygon.Vertices.Min(v => v.X);
		var maxX = polygon.Vertices.Max(v => v.X);
		var minY = polygon.Vertices.Min(v => v.Y);
		var maxY = polygon.Vertices.Max(v => v.Y);
    
		// 🔥 ЗАЩИТА ОТ НУЛЕВОГО РАЗМЕРА
		var origWidth = Math.Max(maxX - minX, 1.0);   // Минимум 1 пиксель
		var origHeight = Math.Max(maxY - minY, 1.0);  // Минимум 1 пиксель
    
		var targetWidth = Math.Abs(end.X - start.X);
		var targetHeight = Math.Abs(end.Y - start.Y);
    
		var scaleX = targetWidth / origWidth;
		var scaleY = targetHeight / origHeight;
		var scale = Math.Max(scaleX, scaleY);
    
		var center = new Point2D((minX + maxX) / 2, (minY + maxY) / 2);
		var newCenter = new Point2D(Math.Min(start.X, end.X) + targetWidth/2,
			Math.Min(start.Y, end.Y) + targetHeight/2);
    
		foreach (var vertex in polygon.Vertices)
		{
			var dx = vertex.X - center.X;
			var dy = vertex.Y - center.Y;
			vertex.X = newCenter.X + dx * scale;
			vertex.Y = newCenter.Y + dy * scale;
        
			// 🔥 Явно уведомляем каждую вершину для реактивности
			vertex.RaisePropertyChanged(nameof(PointViewModel.X));
			vertex.RaisePropertyChanged(nameof(PointViewModel.Y));
		}
    
		// 🔥 Дополнительно уведомляем о изменении центра и коллекции
		polygon.RaisePropertyChanged(nameof(PolygonViewModel.Center));
		polygon.RaisePropertyChanged(nameof(PolygonViewModel.Vertices));
	}

	/// <summary>Приватный метод получения точки на канвасе.</summary>
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