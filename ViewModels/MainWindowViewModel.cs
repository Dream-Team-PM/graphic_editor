// ViewModels/MainWindowViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using System.Reactive;
using System.Reactive.Linq;

using Avalonia.Styling;
using Avalonia.Input;
using Avalonia.Interactivity;

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
	public string SelectedToolDisplayName => _selectedTool.ToDisplayName();
    private int _strokeWidth = 1; /// <summary>Приватное свойство - толщина линии.</summary>
	private double _opacity = 100; /// <summary>Приватное свойство - степень прозрачности.</summary>
	private ColorViewModel _fillColor = new ColorViewModel(Color.FromArgb(255, 74, 144)); /// <summary>Приватное свойство - цвет заполнения.</summary>
	private ColorViewModel _strokeColor = new ColorViewModel(Color.Black); /// <summary>Приватное свойство - цвет обводки.</summary>
	private ThemeVariant _currentTheme = ThemeVariant.Dark; /// <summary>Приватное свойство - текущая тема (светлая или темная).</summary>

	private bool _isDrawing; /// <summary>Приватный флаг - ведётся отрисовка или нет.</summary>
	private Point_1 _drawingStartPoint; /// <summary>Приватная точка - точка начала отрисовки.</summary>
	private bool _hasDrawingStart; /// <summary>Приватный флаг - проверка начала отрисовки.</summary>
	private FigureViewModel? _previewFigure; /// <summary>Приватный флаг - предварительная отрисовка фигуры.</summary>
	private DrawingTool _currentDrawingTool; /// <summary>Приватное свойство - текущий инструмент для рисования.</summary>
	private List<Point_1> _penPoints = new(); /// <summary>Приватное коллекция точек для отрисовки.</summary>

	private double _mouseX; /// <summary>Приватное свойство - позиция мыши по оси Ox.</summary>
	private double _mouseY; /// <summary>Приватное свойство - позиция мыши по оси Oy.</summary>
	private const double MinFigureSize = 5.0;
	private const double PenPointRadius = 3.0;
	private const double DefaultZoomMin = 0.1;
	private const double DefaultZoomMax = 10.0;
	/// <summary>Внутреннее свойство для логики (enum)</summary>
	private DrawingTool CurrentTool => _selectedTool;
    // ========== КОМАНДЫ ==========
	/// <summary>Связь команд в Reactive UI c обычными командами.</summary>
	public EditorCommands Commands { get; }

	// ========== СВОЙСТВА ==========
    public CanvasViewModel Canvas { get; } /// <summary>Публичный канвас.</summary>
    
	/// <summary>Публичное свойство установки статуса.</summary>
	public string StatusMessage { 
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}
    
	private void ApplyStyle<T>(T figure, bool solidFill = false) where T : FigureViewModel
	{
		figure.LineColor = StrokeColor.Color;
		figure.FillColor = solidFill ? StrokeColor.Color : FillColor.Color;
		figure.Thickness = StrokeWidth;
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

	/// <summary>Публичное свойство выбранного инструмента.</summary>
	public string SelectedTool
    {
        get => _selectedTool.ToDisplayName();
        set 
    	{
		    if (DrawingToolExtensions.TryParse(value, out var tool))
		    {
			    if (IsDrawing && _selectedTool != tool)
			    {
				    // Если рисовали пером, удаляем предварительную фигуру
				    if (_selectedTool == DrawingTool.Pen && _previewFigure != null && Canvas?.ActiveLayer != null)
				    {
					    Canvas.ActiveLayer.Figures.Remove(_previewFigure);
				    }

				    ResetDrawingState();
			    }

			    _selectedTool = tool;
			    this.RaiseAndSetIfChanged(ref _selectedTool, tool);
			    this.RaisePropertyChanged(nameof(SelectedTool)); // для string-binding
			    this.RaisePropertyChanged(nameof(SelectedToolDisplayName));
		    }
	    }
    }

	public ObservableCollection<string> Tools { get; } /// <summary>Публичная коллекция инструментов.</summary>
	
	/// <summary>Публичное свойство толщины линии.</summary>
	public int StrokeWidth {
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	/// <summary>Публичное свойство прозрачности.</summary>
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

	/// <summary>Публичное свойство уствновки позиции мыши по Ox.</summary>
	public double MouseX
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

	/// <summary>Публичное свойство уствновки позиции мыши по Oy.</summary>
	public double MouseY
	{
		get => field;
		set => this.RaiseAndSetIfChanged(ref field, value);
	}

    public string CoordinatesText => _coordinatesText.Value; /// <summary>Публичное свойство - значения координат.</summary>
    public bool HasSelection => Canvas?.HasSelection ?? false; /// <summary>Публичное свойство - проверка выбранности канваса.</summary>
    // Поля для управления Popup

    // ========== КОНСТРУКТОР ==========
	public MainWindowViewModel() 
    {
        Canvas = new CanvasViewModel();
        Tools = new ObservableCollection<string> { 
        "Выделение", "Прямоугольник", "Эллипс", "Линия",
        "Многоугольник", "Перо", "Текст", "Рука", "Масштаб"
        };
        SelectedTool = Tools[0];
        Commands = new EditorCommands(
	        AddRectangle: ReactiveCommand.Create(AddRectangle),
	        AddEllipse: ReactiveCommand.Create(AddEllipse),
	        AddLine: ReactiveCommand.Create(AddLine),
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
	        CanvasClicked: ReactiveCommand.Create<Point_1>(CanvasClicked),
			SaveCommand: ReactiveCommand.CreateFromTask(SaveAsync),
	        SetStrokeColorCommand: ReactiveCommand.Create<Avalonia.Media.Color>(c => StrokeColor.Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B)),
			SetFillColorCommand: ReactiveCommand.Create<Avalonia.Media.Color>(c => FillColor.Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B)),
	        OpenFillColorPickerCommand: ReactiveCommand.Create(() => { IsColorPickerOpen = true; }),
			OpenStrokeColorPickerCommand: ReactiveCommand.Create(() => { IsStrokeColorPickerOpen = true; })
        );

        _coordinatesText = this
            .WhenAnyValue(x => x.MouseX, x => x.MouseY)
            .Select(_ => $"X: {MouseX:F10}  Y: {MouseY:F10}")
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
	
	private bool _isColorPickerOpen;
	private bool _isStrokeColorPickerOpen;

	public bool IsColorPickerOpen
	{
		get => _isColorPickerOpen;
		set => this.RaiseAndSetIfChanged(ref _isColorPickerOpen, value);
	}

	public bool IsStrokeColorPickerOpen
	{
		get => _isStrokeColorPickerOpen;
		set => this.RaiseAndSetIfChanged(ref _isStrokeColorPickerOpen, value);
	}
    
    private void OpenColorPicker()
    {
	    // Здесь можно открыть диалог выбора цвета
	    // Или просто использовать ColorPicker в отдельном окне
	    StatusMessage = "Открытие палитры цветов...";
    }
    
    private void ApplyStyleToSelected(Action<FigureViewModel> apply)
    {
	    if (Canvas?.SelectedFigure is FigureViewModel figure)
	    {
		    apply(figure);  // Фигура сама уведомит UI через RaisePropertyChanged
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

	/// <summary>Приватная функция для добавления прямоугольника.</summary>
    private void AddRectangle()
    {
        var rect = new RectangleViewModel(100, 100, 150, 100, StrokeColor.Color, StrokeWidth, FillColor.Color, Opacity / 100.0);
        ApplyStyle(rect);
        Canvas?.AddFigure(rect);
        StatusMessage = "Добавлен прямоугольник";
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
    private void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeVariant.Dark 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        StatusMessage = $"Тема: {(CurrentTheme == ThemeVariant.Light ? "Светлая ☀️" : "Тёмная 🌙")}";
    }

	/// <summary>Приватная функция обновления координат.</summary>
	private void UpdateCoordinates((double x, double y) coords) 
    {
		MouseX = coords.x;
		MouseY = coords.y;
        this.RaisePropertyChanged(nameof(CoordinatesText));
	}

	/// <summary>Приватная функция для активации канваса.</summary>
	public void CanvasClicked(Point_1 point) 
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
	        // Для примитивов: начинаем рисование через команду
	        if (CurrentTool.IsPrimitive())
	        {
		        Commands.CanvasClicked.Execute(point); // или отдельная команда StartDrawingCommand
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
	        { // Начинаем рисование пера
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
        	Canvas.SelectFigureAt(point);
            StatusMessage = HasSelection ? "Объект выделен" : "Выделение снято";
        }
    }

	/// <summary>Приватная функция - начало отрисовки точек.</summary>
	private void StartPenDrawing(Point_1 startPoint)
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
	}

	/// <summary>Приватный метод для начала рисования.</summary>
	private void StartDrawing(Point_1 startPoint, DrawingTool tool)
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
	private void FinishDrawingPrimitive(Point_1 endPoint)
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
	private void AddPenPoint(Point_1 point)
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
	private void UpdatePreviewPoint(Point_1 point)
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
	private FigureViewModel? CreateFinalFigure(Point_1 start, Point_1 end, DrawingTool tool)
    {
	    FigureViewModel? figure = tool switch
	    {
		    DrawingTool.Line => new LineViewModel(
			    start.X, start.Y, end.X, end.Y,
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
	private FigureViewModel? CreatePreviewFigure(Point_1 start, Point_1 end, DrawingTool tool)
	{
		FigureViewModel? figure = tool switch
		{
			DrawingTool.Line => new LineViewModel(
				start.X, start.Y, end.X, end.Y,
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
    private void UpdatePreviewFigure(FigureViewModel preview, Point_1 start, Point_1 end)
    {
        switch (preview)
        {
            case LineViewModel line:
                line.Vertices[1].X = end.X;
                line.Vertices[1].Y = end.Y;
                line.RaisePropertyChanged(nameof(LineViewModel.X2));
                line.RaisePropertyChanged(nameof(LineViewModel.Y2));
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
        }
    }

	/// <summary>Приватный метод получения точки на канвасе.</summary>
    private Point_1 GetCanvasPoint(PointerEventArgs e)
    {
	    var screenPos = e.GetPosition((Avalonia.Visual?)e.Source);
	    if (Canvas != null)
	    {
		    return new Point_1(
			    (screenPos.X - Canvas.OffsetX) / Canvas.Zoom,
			    (screenPos.Y - Canvas.OffsetY) / Canvas.Zoom
		    );
	    }
	    return new Point_1(screenPos.X, screenPos.Y);
    }
}