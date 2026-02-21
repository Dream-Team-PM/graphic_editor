// ViewModels/MainWindowViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using System.Reactive;
using System.Reactive.Linq;

using Avalonia.Styling;

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
    private readonly ObservableAsPropertyHelper<string> _coordinatesText;
	private string _statusMessage = "Готово";
	public string _selectedTool = "Выделение";
	private int _strokeWidth = 1;
	private double _opacity = 100;
	private ColorViewModel _fillColor = new ColorViewModel(Color.FromArgb(255, 74, 144));
	private ColorViewModel _strokeColor = new ColorViewModel(Color.Black);
	private ThemeVariant _currentTheme = ThemeVariant.Dark;
	private double _mouseX;
	private double _mouseY;

    // ========== КОМАНДЫ ==========
    public ReactiveCommand<Unit, Unit> AddRectangleCommand { get; }
    public ReactiveCommand<Unit, Unit> AddEllipseCommand { get; }
    public ReactiveCommand<Unit, Unit> AddLineCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; }
    public ReactiveCommand<Unit, Unit> DuplicateSelectedCommand { get; }
    public ReactiveCommand<Unit, Unit> RotateLeftCommand { get; }
    public ReactiveCommand<Unit, Unit> RotateRightCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomInCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomFitCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; }
    public ReactiveCommand<(double x, double y), Unit> UpdateCoordinatesCommand { get; }
    public ReactiveCommand<Point_1, Unit> CanvasClickedCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateNewLayerCommand { get; }

	// ========== СВОЙСТВА ==========
    public string Greeting { get; } = "Welcome to Avalonia!";
    public CanvasViewModel Canvas { get; }
    public string StatusMessage { 
		get => _statusMessage;
		set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
	}

	public string SelectedTool
    {
        get => _selectedTool;
        set => this.RaiseAndSetIfChanged(ref _selectedTool, value);
    }

	public ObservableCollection<string> Tools { get; }
	public int StrokeWidth {
		get => _strokeWidth;
		set => this.RaiseAndSetIfChanged(ref _strokeWidth, value);
	}

	public double Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }

    public ColorViewModel FillColor
    {
        get => _fillColor;
        set => this.RaiseAndSetIfChanged(ref _fillColor, value);
    }

    public ColorViewModel StrokeColor
    {
        get => _strokeColor;
        set => this.RaiseAndSetIfChanged(ref _strokeColor, value);
    }

    public ThemeVariant CurrentTheme
    {
        get => _currentTheme;
        set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
    }

    public double MouseX
    {
        get => _mouseX;
        set => this.RaiseAndSetIfChanged(ref _mouseX, value);
    }

	public double MouseY
    {
        get => _mouseY;
        set => this.RaiseAndSetIfChanged(ref _mouseY, value);
    }

    public string CoordinatesText => _coordinatesText.Value;
    public bool HasSelection => Canvas?.HasSelection ?? false;

    // ========== КОНСТРУКТОР ==========
	public MainWindowViewModel() 
    {
        Canvas = new CanvasViewModel();
        Tools = new ObservableCollection<string> { 
        "Выделение", "Прямоугольник", "Эллипс", "Линия",
        "Многоугольник", "Перо", "Текст", "Рука", "Масштаб"
        };
        SelectedTool = Tools[0];
        AddRectangleCommand = ReactiveCommand.Create(AddRectangle);
        AddEllipseCommand = ReactiveCommand.Create(AddEllipse);
        AddLineCommand = ReactiveCommand.Create(AddLine);
        DeleteSelectedCommand = ReactiveCommand.Create(DeleteSelected);
        DuplicateSelectedCommand = ReactiveCommand.Create(DuplicateSelected);
        RotateLeftCommand = ReactiveCommand.Create(RotateLeft);
        RotateRightCommand = ReactiveCommand.Create(RotateRight);
        ZoomInCommand = ReactiveCommand.Create(ZoomIn);
        ZoomOutCommand = ReactiveCommand.Create(ZoomOut);
        ZoomFitCommand = ReactiveCommand.Create(ZoomFit);
        ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
        UpdateCoordinatesCommand = ReactiveCommand.Create<(double x, double y)>(UpdateCoordinates);
        CanvasClickedCommand = ReactiveCommand.Create<Point_1>(CanvasClicked);
        SaveCommand = ReactiveCommand.Create(Save);
        OpenCommand = ReactiveCommand.Create(Open);
        ExportCommand = ReactiveCommand.Create(Export);
        CreateNewLayerCommand = ReactiveCommand.Create(CreateNewLayer);

        _coordinatesText = this
            .WhenAnyValue(x => x.MouseX, x => x.MouseY)
            .Select(_ => $"X: {MouseX:F0}  Y: {MouseY:F0}")
            .ToProperty(this, x => x.CoordinatesText);
    }

    private void AddRectangle()
    {
        var rect = new RectangleViewModel(100, 100, 150, 100)
        {
            LineColor = StrokeColor.Color,
            FillColor = FillColor.Color,
            Thickness = StrokeWidth
        };
        Canvas?.AddFigure(rect);
        StatusMessage = "Добавлен прямоугольник";
    }

    private void AddEllipse()
    {
        var ellipse = new EllipseViewModel(100, 100, 150, 100)
        {
            LineColor = StrokeColor.Color,
            FillColor = FillColor.Color,
            Thickness = StrokeWidth
        };
        Canvas?.AddFigure(ellipse);
        StatusMessage = "Добавлен эллипс";
    }

    private void AddLine()
    {
        var line = new LineViewModel(100, 100, 300, 300)
        {
            LineColor = StrokeColor.Color,
            FillColor = FillColor.Color,
            Thickness = StrokeWidth
        };
        Canvas?.AddFigure(line);
        StatusMessage = "Добавлена линия";
    }

	private void DeleteSelected()
    {
        Canvas?.RemoveSelectedFigure();
        StatusMessage = "Объект удалён";
    }

    private void DuplicateSelected()
    {
        Canvas?.DuplicateSelectedFigure();
        StatusMessage = "Объект дублирован";
    }

    private void RotateLeft()
    {
        Canvas?.RotateSelectedFigure(-90);
        StatusMessage = "Поворот на -90°";
    }

    private void RotateRight()
    {
        Canvas?.RotateSelectedFigure(90);
        StatusMessage = "Поворот на 90°";
    }

    private void ZoomIn()
    {
        if (Canvas != null)
        {
            Canvas.Zoom *= 1.5;
            StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
        }
    }

    private void ZoomOut()
    {
        if (Canvas != null)
        {
            Canvas.Zoom *= 0.5;
            StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
        }
    }

    private void ZoomFit()
    {
        if (Canvas != null) {
            Canvas.Zoom = 1.0;
            StatusMessage = "Масштаб: по размеру окна";
        }
    }

    private void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeVariant.Dark 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        StatusMessage = $"Тема: {(CurrentTheme == ThemeVariant.Light ? "Светлая ☀️" : "Тёмная 🌙")}";
    }

	private void UpdateCoordinates((double x, double y) coords) 
    {
		MouseX = coords.x;
		MouseY = coords.y;
        this.RaisePropertyChanged(nameof(CoordinatesText));
	}

	public void CanvasClicked(Point_1 point) 
    {
        DebugLog.Write($"[DEBUG] CanvasClicked START: Tool='{SelectedTool}', IsCanvasActive={Canvas?.IsCanvasActive}, Canvas={Canvas?.GetHashCode()}");
    
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
    
        DebugLog.Write($"[DEBUG] SelectedTool value: '{SelectedTool}' (Length={SelectedTool?.Length})");
    
        if (SelectedTool == "Выделение")
        {
            DebugLog.Write("[DEBUG] Tool=Выделение, calling SelectFigureAt");
            Canvas.SelectFigureAt(point);
            StatusMessage = HasSelection ? "Объект выделен" : "Выделение снято";
        }
        else if (SelectedTool == "Прямоугольник")
        {
            DebugLog.Write("[DEBUG] Tool=Прямоугольник, calling AddRectangle");
            AddRectangle();
        }
        else if (SelectedTool == "Эллипс")
        {
            DebugLog.Write("[DEBUG] Tool=Эллипс, calling AddEllipse");
            AddEllipse();
        } 
        else if (SelectedTool == "Линия")
        {
            DebugLog.Write("[DEBUG] Tool=Линия, calling AddLine");
            AddLine();
        }
        else if (SelectedTool == "Перо")
        {
            DebugLog.Write("[DEBUG] Tool=Перо, creating PenPoint");
            var penPoint = new PenPointViewModel(point.X, point.Y)
            {
                LineColor = StrokeColor.Color,
                FillColor = StrokeColor.Color,
                Thickness = StrokeWidth
            };
            Canvas.AddFigure(penPoint);
            StatusMessage = $"Точка: ({point.X:F0}, {point.Y:F0})";
        }
        else
        {
           DebugLog.Write($"[WARN] Unknown tool: '{SelectedTool}' - no action taken");
        }
        DebugLog.Write($"[DEBUG] CanvasClicked END");
    }

    private void Save()
    {
        StatusMessage = "Сохранение...";
        // TODO: Реализовать сохранение
    }

    private void Open()
    {
        StatusMessage = "Открытие файла...";
        // TODO: Реализовать открытие
    }

    private void Export()
    {
        StatusMessage = "Экспорт...";
        // TODO: Реализовать экспорт
    }

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
}
