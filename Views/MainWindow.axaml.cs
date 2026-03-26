// Views/MainWindow.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using graphic_editor.Controls;
using graphic_editor.Models;
using graphic_editor.Geometry;
using ReactiveUI;

using graphic_editor.Helpers;
using graphic_editor.IO.Export;
using graphic_editor.IO.Services;
using graphic_editor.Services;
using graphic_editor.State;
using graphic_editor.ViewModels;

namespace graphic_editor;

/// <summary>
/// Основное (главное) окно графического редактора.
/// Отвечает за инициализацию UI, привязку ViewModel и обработку событий ввода с холста.
/// </summary> 
public partial class MainWindow : Window
{
	/// <summary>Приватное поле для хранения экземпляра ViewModel главного окна.</summary>
	private MainWindowViewModel? _viewModel;
	private readonly ProjectService _projectService = new();
	private string? _currentFilePath;

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
		var vectorCanvas = this.FindControl<VectorCanvasControl>("VectorCanvas");
        if (vectorCanvas != null)
        {
            vectorCanvas.AddHandler(PointerPressedEvent, OnCanvasPointerPressed, handledEventsToo: true);
            vectorCanvas.AddHandler(PointerMovedEvent, OnCanvasPointerMoved, handledEventsToo: true);
            vectorCanvas.AddHandler(PointerReleasedEvent, OnCanvasPointerReleased, handledEventsToo: true);
			vectorCanvas.AddHandler(KeyDownEvent, OnWindowKeyDown, handledEventsToo: true);
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

    private void FillColorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ColorPickerControl == null) return;
        
        // Очищаем старую подписку перед новой
        ColorPickerControl.ColorSelected -= OnFillColorSelected;
        ColorPickerControl.ColorSelected += OnFillColorSelected;
        ColorPickerControl.Cancelled += OnFillColorPickerCancelled;
        // Инициализируем текущим цветом заливки
        if (DataContext is MainWindowViewModel vm)
        {
            var currentColor = Color.FromArgb(
                vm.FillColor.Color.A,
                vm.FillColor.Color.R,
                vm.FillColor.Color.G,
                vm.FillColor.Color.B
            );
        }
        ColorPopup.IsOpen = true;
    }

    private void StrokeColorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (StrokeColorPickerControl == null) return;
        
        StrokeColorPickerControl.ColorSelected -= OnStrokeColorSelected;
        StrokeColorPickerControl.ColorSelected += OnStrokeColorSelected;
        StrokeColorPickerControl.Cancelled += OnStrokeColorPickerCancelled;
        
        // Инициализируем текущим цветом обводки
        if (DataContext is MainWindowViewModel vm)
        {
            var currentColor = Avalonia.Media.Color.FromArgb(
                _viewModel.StrokeColor.Color.A,
                _viewModel.StrokeColor.Color.R,
                _viewModel.StrokeColor.Color.G,
                _viewModel.StrokeColor.Color.B
            );
            StrokeColorPickerControl.SetColor(currentColor);
        }
        StrokeColorPopup.IsOpen = true;
    }

    private void OnFillColorSelected(Avalonia.Media.Color color)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.FillColor.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            vm.IsColorPickerOpen = false;
            vm.StatusMessage = $"Цвет заливки: #{color.ToString()}";
        }
        
        ColorPopup.IsOpen = false;
    }

    private void OnStrokeColorSelected(Avalonia.Media.Color color)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.StrokeColor.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            vm.IsStrokeColorPickerOpen = false;
            vm.StatusMessage = $"Цвет обводки изменён: #{color.ToString()}";
        }
        StrokeColorPopup.IsOpen = false;
    }

    private void OnFillColorCancelled()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.IsColorPickerOpen = false;
        }
    }

    private void OnStrokeColorCancelled()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.IsStrokeColorPickerOpen = false;
        }
    }
    
    private void OnStrokeColorPickerCancelled()
    {
        StrokeColorPopup.IsOpen = false;
    }

    private void OnFillColorPickerCancelled()
    {
        ColorPopup.IsOpen = false;
    }

	// ── Сохранение / Открытие / Экспорт ─────────────────────────────────────

    private async void SaveMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentFilePath == null)
            await SaveAs();
        else
            await DoSave(_currentFilePath);
    }

    private async void SaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        await SaveAs();
    }

    private async void OpenMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Открыть проект",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Поддерживаемые форматы") { Patterns = ["*.vec", "*.json", "*.svg"] },
                new FilePickerFileType("Проект INKognida (*.vec)") { Patterns = ["*.vec"] },
                new FilePickerFileType("SVG изображение (*.svg)") { Patterns = ["*.svg"] },
                new FilePickerFileType("JSON (*.json)") { Patterns = ["*.json"] },
            }
        });

        if (files.Count == 0 || _viewModel == null) return;

        var path = files[0].Path.LocalPath;
        _viewModel.StatusMessage = "Открытие...";
        var ok = await _projectService.LoadProjectAsync(path, _viewModel.Canvas);
        if (ok)
        {
            _currentFilePath = path;
            Title = $"INKognida — {Path.GetFileName(path)}";
            _viewModel.StatusMessage = $"Открыто: {Path.GetFileName(path)} ✓";
        }
        else
        {
            _viewModel.StatusMessage = "Ошибка открытия ✗";
        }
    }

    private async void ExportButton_Click(object? sender, RoutedEventArgs e)
    {
		if (_viewModel?.Canvas == null) return;
		var fileTypes = new[]
    	{
        	new FilePickerFileType("PNG изображение (*.png)") { Patterns = ["*.png"] },
        	new FilePickerFileType("JPEG изображение (*.jpg;*.jpeg)") { Patterns = ["*.jpg", "*.jpeg"] },
        	new FilePickerFileType("BMP изображение (*.bmp)") { Patterns = ["*.bmp"] },
        	new FilePickerFileType("PDF документ (*.pdf)") { Patterns = ["*.pdf"] },
        	// new FilePickerFileType("RTF документ (*.rtf)") { Patterns = ["*.rtf"] },
    	};

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Экспорт изображения",
        	DefaultExtension = "png",
        	FileTypeChoices = fileTypes,
        	SuggestedFileName = "Безымянный"
        });

        if (file?.Path?.LocalPath is not string path) return;

        var vectorCanvas = this.FindControl<VectorCanvasControl>("VectorCanvas");
        if (vectorCanvas == null) return;

        _viewModel.StatusMessage = "Экспорт изображения...";
        try
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
        
        	Task exportTask = extension switch
        	{
            	".png" => PngExporter.ExportAsync(path, vectorCanvas),
            	".jpg" or ".jpeg" => JpegExporter.ExportAsync(path, vectorCanvas, quality: 90),
            	".bmp" => BmpExporter.ExportAsync(path, vectorCanvas),
            	".pdf" => PdfExporter.ExportAsync(path, vectorCanvas, _viewModel.Canvas),
				// ".rtf" => RtfExporter.ExportAsync(path, vectorCanvas),
           		 _ => throw new NotSupportedException($"Формат {extension} не поддерживается")
        	};
			await exportTask;
        
       	 	_viewModel.StatusMessage = $"Экспортировано: {Path.GetFileName(path)} ✓";
        }
        catch (Exception ex)
        {
            DebugLog.Write($"[ERROR] Export failed: {ex.Message}");
        	_viewModel.StatusMessage = $"Ошибка экспорта: {ex.Message} ✗";
        }
    }

    private async Task SaveAs()
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить проект",
            DefaultExtension = "vec",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Проект INKognida (*.vec)") { Patterns = ["*.vec"] },
                new FilePickerFileType("SVG изображение (*.svg)") { Patterns = ["*.svg"] },
            }
        });

        if (file?.Path?.LocalPath is string path)
        {
            _currentFilePath = path;
            await DoSave(path);
        }
    }

    private async Task DoSave(string path)
    {
        if (_viewModel == null) return;
        _viewModel.StatusMessage = "Сохранение...";
        var ok = await _projectService.SaveProjectAsync(path, _viewModel.Canvas);
        if (ok)
        {
            Title = $"INKognida — {Path.GetFileName(path)}";
            _viewModel.StatusMessage = $"Сохранено: {Path.GetFileName(path)} ✓";
        }
        else
        {
            _viewModel.StatusMessage = "Ошибка сохранения ✗";
        }
    }

protected override void OnKeyDown(KeyEventArgs e)
{
    base.OnKeyDown(e);
    
    if (_viewModel?.IsDrawing == true && _viewModel.CurrentTool == DrawingTool.Text)
    {
        if (e.Key == Key.Enter)
        {
            // Завершаем ввод текста
            _viewModel.FinishTextInput();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            // Отменяем ввод
            _viewModel.CancelTextInput();
            e.Handled = true;
        }
    }
}

private void OnWindowKeyDown(object? sender, KeyEventArgs e)
{
    if (_viewModel?.IsDrawing != true || _viewModel.CurrentTool != DrawingTool.Text)
        return;
    
    if (_viewModel.PreviewFigure is not TextViewModel text)
        return;
    
    if (e.Key == Key.Enter)
    {
        _viewModel.FinishTextInput();
        e.Handled = true;
        return;
    }
    if (e.Key == Key.Escape)
    {
        _viewModel.CancelTextInput();
        e.Handled = true;
        return;
    }
    if (e.Key == Key.Back && text.Text.Length > 0)
    {
        text.Text = text.Text[..^1];
        text.NotifyTextChanged();
        e.Handled = true;
        return;
    }
    
    var keyText = e.Key.ToString();
    
    if (keyText.Length == 1)
    {
        var finalChar = e.KeyModifiers.HasFlag(KeyModifiers.Shift) 
            ? keyText.ToUpper() 
            : keyText.ToLower();
        
        text.Text += finalChar;
        text.NotifyTextChanged();
        e.Handled = true;
    }
}

private void AboutMenuItem_Click(object? sender, RoutedEventArgs e)
{
    var about = new Window
    {
        Title = "О программе",
        Width = 400,
        Height = 300,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = "INKognida", FontWeight = FontWeight.Bold, FontSize = 18 },
                new TextBlock { Text = "Версия 1.0.0", Foreground = Brushes.Gray },
                new TextBlock { Text = "Векторный графический редактор", Margin = new Thickness(0, 8, 0, 0) },
                new TextBlock { Text = "© 2026 Dream Team CO", Foreground = Brushes.Gray },
                // 🔥 Создаём кнопку БЕЗ подписки в инициализаторе
                new Button 
                { 
                    Content = "Закрыть", 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right 
                }
            }
        }
    };
    
    // 🔥 Подписываемся на событие ПОСЛЕ создания объекта
    if (about.Content is StackPanel panel && panel.Children.LastOrDefault() is Button closeBtn)
    {
        closeBtn.Click += (s, args) => about.Close();
    }
    
    about.Show(this);
}

private void ShortcutsMenuItem_Click(object? sender, RoutedEventArgs e)
{
    var shortcuts = new Window
    {
        Title = "Сочетания клавиш",
        Width = 500,
        Height = 400,
        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Файл", FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 4) },
                    new TextBlock { Text = "Ctrl+N — Новый проект" },
                    new TextBlock { Text = "Ctrl+O — Открыть" },
                    new TextBlock { Text = "Ctrl+S — Сохранить" },
                    new TextBlock { Text = "Ctrl+Shift+S — Сохранить как", Margin = new Thickness(0, 0, 0, 12) },
                    
                    new TextBlock { Text = "Редактирование", FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 4) },
                    new TextBlock { Text = "Ctrl+Z — Отменить" },
                    new TextBlock { Text = "Ctrl+Y — Повторить" },
                    new TextBlock { Text = "Ctrl+G — Сгруппировать" },
                    new TextBlock { Text = "Ctrl+Shift+G — Разгруппировать", Margin = new Thickness(0, 0, 0, 12) },
                    
                    new TextBlock { Text = "Слои", FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 4) },
                    new TextBlock { Text = "Ctrl+Shift+N — Новый слой" },
                    new TextBlock { Text = "Ctrl+Shift+D — Удалить слой" },
                    new TextBlock { Text = "Ctrl+J — Дублировать слой" },
                    new TextBlock { Text = "Ctrl+E — Объединить с предыдущим" },
                }
            }
        }
    };
    shortcuts.Show(this);
}

private async void DocumentationMenuItem_Click(object? sender, RoutedEventArgs e)
{
    try 
    {
        var launcher = TopLevel.GetTopLevel(this)?.Launcher;
        if (launcher != null)
            await launcher.LaunchUriAsync(new Uri("https://github.com/Dream-Team-PM/graphic_editor/wiki"));
    }
    catch { /* ignore */ }
}

private async void ReportIssueMenuItem_Click(object? sender, RoutedEventArgs e)
{
    try 
    {
        var launcher = TopLevel.GetTopLevel(this)?.Launcher;
        if (launcher != null)
            await launcher.LaunchUriAsync(new Uri("https://github.com/Dream-Team-PM/graphic_editor/issues"));
    }
    catch { /* ignore */ }
}
}