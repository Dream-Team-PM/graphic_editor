// ViewModels/MainWindowViewModel.cs

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Input;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using graphic_editor.Models;
using graphic_editor.Helpers;

namespace graphic_editor.ViewModels;


/// <summary>
/// ViewModel для главного окна графического редактора "Магический графический редактор".
/// Управляет состоянием UI, видимостью экранов и игровой логикой.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

	private string _statusMessage = "Готово";
	public string _selectedTool = "Выделение";
	private int _strokeWidth = 1;
	private double _opacity = 100;
	private ColorViewModel _fillColor = new ColorViewModel(Color.FromArgb(255, 74, 144));
	private ColorViewModel _strokeColor = new ColorViewModel(Color.Black);
	private ThemeVariant _currentTheme = ThemeVariant.Dark;
	private double _mouseX;
	private double _mouseY;

	[RelayCommand]
    public void TestCommand()
    {
        Console.WriteLine($"[DEBUG ViewModel] TestCommand called");
    }

	public MainWindowViewModel() {
		Canvas = new CanvasViewModel();
		Tools = new ObservableCollection<string> { 
			"Выделение", "Прямоугольник", "Эллипс", 
            "Многоугольник", "Перо", "Текст", "Рука", "Масштаб"
		};
		SelectedTool = Tools[0];
	}

	public CanvasViewModel Canvas { get; }
	public string StatusMessage { 
		get => _statusMessage;
		set => SetProperty(ref _statusMessage, value);
	}

	public string SelectedTool
    {
        get => _selectedTool;
        set => SetProperty(ref _selectedTool, value);
    }

	public ObservableCollection<string> Tools { get; }
	public int StrokeWidth {
		get => _strokeWidth;
		set => SetProperty(ref _strokeWidth, value);
	}

	public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public ColorViewModel FillColor
    {
        get => _fillColor;
        set => SetProperty(ref _fillColor, value);
    }

    public ColorViewModel StrokeColor
    {
        get => _strokeColor;
        set => SetProperty(ref _strokeColor, value);
    }

    public ThemeVariant CurrentTheme
    {
        get => _currentTheme;
        set => SetProperty(ref _currentTheme, value);
    }

    public double MouseX
    {
        get => _mouseX;
        set => SetProperty(ref _mouseX, value);
    }

	public double MouseY
    {
        get => _mouseY;
        set => SetProperty(ref _mouseY, value);
    }

	public string CoordinatesText => $"X: {MouseX:F0}  Y: {MouseY:F0}";

    public bool HasSelection => Canvas.HasSelection;

    [RelayCommand]
    private void AddRectangle()
    {
        var rect = new RectangleViewModel(100, 100, 150, 100)
        {
            LineColor = StrokeColor.Color,
            FillColor = FillColor.Color,
            Thickness = StrokeWidth
        };
        Canvas.AddFigure(rect);
        StatusMessage = "Добавлен прямоугольник";
    }

	[RelayCommand]
    private void AddEllipse()
    {
        var ellipse = new EllipseViewModel(100, 100, 150, 100)
        {
            LineColor = StrokeColor.Color,
            FillColor = FillColor.Color,
            Thickness = StrokeWidth
        };
        Canvas.AddFigure(ellipse);
        StatusMessage = "Добавлен эллипс";
    }

	[RelayCommand]
	private void DeleteSelected()
    {
        Canvas.RemoveSelectedFigure();
        StatusMessage = "Объект удалён";
    }

	[RelayCommand]
    private void DuplicateSelected()
    {
        Canvas.DuplicateSelectedFigure();
        StatusMessage = "Объект дублирован";
    }

    [RelayCommand]
    private void RotateLeft()
    {
        Canvas.RotateSelectedFigure(-90);
        StatusMessage = "Поворот на -90°";
    }

    [RelayCommand]
    private void RotateRight()
    {
        Canvas.RotateSelectedFigure(90);
        StatusMessage = "Поворот на 90°";
    }

    [RelayCommand]
    private void ZoomIn()
    {
        Canvas.Zoom *= 1.5;
        StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
    }

    [RelayCommand]
    private void ZoomOut()
    {
        Canvas.Zoom *= 0.5;
        StatusMessage = $"Масштаб: {Canvas.Zoom:P0}";
    }

	[RelayCommand]
    private void ZoomFit()
    {
        Canvas.Zoom = 1.0;
        StatusMessage = "Масштаб: по размеру окна";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ThemeVariant.Dark 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        StatusMessage = $"Тема: {(CurrentTheme == ThemeVariant.Light ? "Светлая ☀️" : "Тёмная 🌙")}";
    }

	[RelayCommand]
	private void UpdateCoordinates((double x, double y) coords) {
		MouseX = coords.x;
		MouseY = coords.y;
		OnPropertyChanged(nameof(CoordinatesText));
	}

	[RelayCommand]
	private void CanvasClicked(Point_1 point) 
	{
        DebugLog.Write($"[DEBUG] CanvasClicked: Tool={SelectedTool}, IsCanvasActive={Canvas.IsCanvasActive}");
        if (!Canvas.IsCanvasActive)
        {
            Canvas.ActivateCanvas();
            StatusMessage = "Слой создан. Можно рисовать! ✏️";
        }
		if (SelectedTool == "Выделение")
        {
            Canvas.SelectFigureAt(point);
            StatusMessage = HasSelection ? "Объект выделен" : "Выделение снято";
        }
        else if (SelectedTool == "Прямоугольник")
        {
            AddRectangle();
        }
        else if (SelectedTool == "Эллипс")
        {
            AddEllipse();
        } 
        else if (SelectedTool == "Перо")
        {
            var penPoint = new PenPointViewModel(point.X, point.Y)
            {
                LineColor = StrokeColor.Color,      // Цвет обводки = цвет точки
                FillColor = StrokeColor.Color,      // Заливка = цвет обводки
                Thickness = StrokeWidth              // Толщина влияет на размер точки
            };
            Canvas.AddFigure(penPoint);
            StatusMessage = $"Точка: ({point.X:F0}, {point.Y:F0})";
        }
	}

	[RelayCommand]
    private void Save()
    {
        StatusMessage = "Сохранение...";
        // TODO: Реализовать сохранение
    }

    [RelayCommand]
    private void Open()
    {
        StatusMessage = "Открытие файла...";
        // TODO: Реализовать открытие
    }

    [RelayCommand]
    private void Export()
    {
        StatusMessage = "Экспорт...";
        // TODO: Реализовать экспорт
    }









}
