// Views/MainWindow.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Input;

using graphic_editor.ViewModels;
using graphic_editor.Helpers;
using graphic_editor.State;
using graphic_editor.Services;

namespace graphic_editor;

/// <summary>
/// Основное (главное) окно графического редактора.
/// Отвечает за инициализацию UI, привязку ViewModel и обработку событий ввода с холста.
/// </summary> 
public partial class MainWindow : Window
{
	/// <summary>Приватное поле для хранения экземпляра ViewModel главного окна.</summary>
	private MainWindowViewModel? _viewModel;

	/// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MainWindow"/>.
    /// Выполняет загрузку XAML, создание зависимостей (FileService, HistoryViewModel) 
    /// и установку DataContext.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        var defaultStyle = new StyleSettings(
            System.Drawing.Color.Black,
            System.Drawing.Color.Transparent,
            2.0);
        
		var fileService = new FileService();
		var history = new HistoryViewModel();
		_viewModel = new MainWindowViewModel(fileService, history);
		DataContext = _viewModel;

        // Начальные значения UI-элементов
        SelectedToolText.Text = "Выделение";
        StrokePercentText.Text = "50%";

		// Подписка на события указателя для канваса
		if (this.FindControl<Canvas>("MainCanvas") is Canvas canvas) 
		{
			canvas.AddHandler(PointerMovedEvent, OnCanvasPointerMoved);
			canvas.AddHandler(PointerPressedEvent, OnCanvasPointerPressed);
		}

		// Инициализация слайдера темы
        if (ThemeSlider != null)
        {
            ThemeSlider.Value = _viewModel.CurrentTheme == ThemeVariant.Light ? 1 : 0;
        }
    }

	/// <summary>
    /// Обработчик события перемещения указателя мыши над холстом.
    /// Преобразует экранные координаты в координаты канваса и обновляет ViewModel.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события указателя.</param>
	private void OnCanvasPointerMoved(object? sender, PointerEventArgs e) 
	{
    	if (_viewModel == null) return;
    	// Обновляем координаты курсора
    	var screenPos = e.GetPosition(VectorCanvas);
    	var canvasPoint = VectorCanvas.ScreenToCanvas(screenPos);
    	_viewModel.Commands.UpdateCoordinates.Execute((canvasPoint.X, canvasPoint.Y));
    	_viewModel.HandlePointerMoved(e);
	}

	/// <summary>
    /// Обработчик события нажатия кнопки мыши на холсте.
    /// Преобразует координаты и передаёт событие в ViewModel для обработки.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события нажатия указателя.</param>
	private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e) 
	{
    	if (_viewModel == null) return;
    	var screenPos = e.GetPosition(VectorCanvas);
    	var point = VectorCanvas.ScreenToCanvas(screenPos);
    	_viewModel.HandlePointerPressed(e);
	}

	/// <summary>
    /// Обработчик события отпускания кнопки мыши на холсте.
    /// Завершает операции рисования или выделения и передаёт событие в ViewModel.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события отпускания указателя.</param>
	private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e) 
	{
    	if (_viewModel == null) return;
    	var screenPos = e.GetPosition(VectorCanvas);
    	var point = VectorCanvas.ScreenToCanvas(screenPos);
    	_viewModel.HandlePointerReleased(e);
	}

	/// <summary>
    /// Обработчик изменения состояния кнопки выбора инструмента.
    /// Устанавливает выбранный инструмент в ViewModel по значению Tag кнопки.
    /// </summary>
    /// <param name="sender">Источник события (RadioButton).</param>
    /// <param name="e">Аргументы события.</param>
    private void ToolButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton btn && btn.IsChecked == true && btn.Tag is string toolName)
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: Setting SelectedTool to '{toolName}' (Tag={btn.Tag})");
            _viewModel.SetToolByName(toolName);
            SelectedToolText.Text = toolName;
        }
        else
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: sender={sender?.GetType()}, IsChecked={(sender as RadioButton)?.IsChecked}, Tag={(sender as RadioButton)?.Tag}");
        }
    }

	/// <summary>
    /// Обработчик изменения значения слайдера толщины обводки.
    /// Обновляет текстовое отображение процента толщины в UI.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения значения.</param>
    private void StrokeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
		StrokePercentText.Text = $"{(int)e.NewValue}%";
    }

	/// <summary>
    /// Обработчик изменения значения слайдера темы.
    /// Переключает тему приложения между светлой и тёмной.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события изменения значения.</param>
	private void ThemeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_viewModel == null) return;
        
        _viewModel.CurrentTheme = e.NewValue >= 0.5 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
        
        this.RequestedThemeVariant = _viewModel.CurrentTheme;
        _viewModel.ToggleTheme();
    }
}