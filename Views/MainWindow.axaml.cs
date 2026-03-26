using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using graphic_editor.Controls;
using graphic_editor.Helpers;
using graphic_editor.IO.Export;
using graphic_editor.IO.Services;
using graphic_editor.Services;
using graphic_editor.State;
using graphic_editor.ViewModels;
using graphic_editor.Geometry;
using graphic_editor.Models;
using ReactiveUI;

namespace graphic_editor;

/// <summary>
/// Основное окно графического редактора INKognida.
/// Координирует все UI-компоненты и связывает их с ViewModel через ReactiveUI.
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private readonly ProjectService _projectService = new();
    private string? _currentFilePath;

    public MainWindow()
    {
        InitializeComponent();
        InitializeViewModel();
        SubscribeToComponentEvents();
        InitializeUIState();
    }

    private void InitializeViewModel()
    {
        var fileService = new FileService();
        var history = new HistoryViewModel();
        _viewModel = new MainWindowViewModel(fileService, history);
        DataContext = _viewModel;
    }

    private void SubscribeToComponentEvents()
    {
        // ── MainMenuBar ──
        MainMenuBarControl.OpenClicked += OnOpenMenuItem_Click;
        MainMenuBarControl.SaveClicked += OnSaveMenuItem_Click;
        MainMenuBarControl.SaveAsClicked += OnSaveAsMenuItem_Click;
        MainMenuBarControl.ExportClicked += OnExportButton_Click;
        MainMenuBarControl.AboutClicked += OnAboutMenuItem_Click;
        MainMenuBarControl.ShortcutsClicked += OnShortcutsMenuItem_Click;
        MainMenuBarControl.DocumentationClicked += OnDocumentationMenuItem_Click;
        MainMenuBarControl.ReportIssueClicked += OnReportIssueMenuItem_Click;
        MainMenuBarControl.ThemeToggleChanged += ThemeToggle_Changed;

        // ── ToolSettingsBar ──
        ToolSettingsBarControl.FillColorClicked += OnFillColorButton_Click;
        ToolSettingsBarControl.StrokeColorClicked += OnStrokeColorButton_Click;
        ToolSettingsBarControl.StrokeSliderValueChanged += StrokeSlider_ValueChanged;

        // ── ToolsSidebar ──
        ToolsSidebarControl.ToolButtonChecked += ToolButton_Checked;

        // ── EditorWorkspace ──
        EditorWorkspaceControl.VectorCanvasElement.PointerMoved += OnCanvasPointerMoved;
        EditorWorkspaceControl.VectorCanvasElement.PointerPressed += OnCanvasPointerPressed;
        EditorWorkspaceControl.VectorCanvasElement.PointerReleased += OnCanvasPointerReleased;

        // ── ColorPickerPopup ──
        ToolSettingsBarControl.ColorPickerControlElement.ColorSelected += OnFillColorSelected;
        ToolSettingsBarControl.ColorPickerControlElement.Cancelled += OnColorPickerCancelled;
        ToolSettingsBarControl.StrokeColorPickerControlElement.ColorSelected += OnStrokeColorSelected;
        ToolSettingsBarControl.StrokeColorPickerControlElement.Cancelled += OnColorPickerCancelled;

        // ── Global KeyDown для ввода текста ──
        KeyDown += OnWindowKeyDown;
    }

    private void InitializeUIState()
    {
        ToolSettingsBarControl.SelectedToolTextElement.Text = "Выделение";
        ToolSettingsBarControl.StrokePercentTextElement.Text = "50%";
        MainMenuBarControl.ThemeToggleElement.IsChecked = _viewModel?.CurrentTheme == ThemeVariant.Light;
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчики холста ───────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_viewModel == null) return;
        var screenPos = e.GetPosition(EditorWorkspaceControl.VectorCanvasElement);
        var canvasPoint = EditorWorkspaceControl.VectorCanvasElement.ScreenToCanvas(screenPos);
        _viewModel.Commands.UpdateCoordinates.Execute((canvasPoint.X, canvasPoint.Y));
        _viewModel.HandlePointerMoved(e);
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewModel == null) return;
        var screenPos = e.GetPosition(EditorWorkspaceControl.VectorCanvasElement);
        EditorWorkspaceControl.VectorCanvasElement.ScreenToCanvas(screenPos);
        _viewModel.HandlePointerPressed(e);
    }

    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_viewModel == null) return;
        var screenPos = e.GetPosition(EditorWorkspaceControl.VectorCanvasElement);
        EditorWorkspaceControl.VectorCanvasElement.ScreenToCanvas(screenPos);
        _viewModel.HandlePointerReleased(e);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчики инструментов ─────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private void ToolButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton btn && btn.IsChecked == true && btn.Tag is string toolName)
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: Setting SelectedTool to '{toolName}'");
            _viewModel?.SetToolByName(toolName);
            ToolSettingsBarControl.SelectedToolTextElement.Text = toolName;
        }
    }

    private void StrokeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        ToolSettingsBarControl.StrokePercentTextElement.Text = $"{(int)e.NewValue}%";
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчик темы ──────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private void ThemeToggle_Changed(object? sender, bool isLightTheme)
    {
        if (_viewModel == null) return;
        var targetTheme = isLightTheme ? ThemeVariant.Light : ThemeVariant.Dark;
        if (_viewModel.CurrentTheme != targetTheme)
        {
            _viewModel.ToggleTheme();
        }
        RequestedThemeVariant = _viewModel.CurrentTheme;
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчики цветовых пикеров ─────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private void OnFillColorButton_Click(object? sender, RoutedEventArgs e)
    {
        var colorPicker = ToolSettingsBarControl.ColorPickerControlElement;
        colorPicker.ColorSelected -= OnFillColorSelected;
        colorPicker.ColorSelected += OnFillColorSelected;
        colorPicker.Cancelled -= OnColorPickerCancelled;
        colorPicker.Cancelled += OnColorPickerCancelled;
        ToolSettingsBarControl.ColorPopupElement.IsOpen = true;
    }

    private void OnStrokeColorButton_Click(object? sender, RoutedEventArgs e)
    {
        var strokePicker = ToolSettingsBarControl.StrokeColorPickerControlElement;
        strokePicker.ColorSelected -= OnStrokeColorSelected;
        strokePicker.ColorSelected += OnStrokeColorSelected;
        strokePicker.Cancelled -= OnColorPickerCancelled;
        strokePicker.Cancelled += OnColorPickerCancelled;

        if (_viewModel != null)
        {
            var currentColor = Avalonia.Media.Color.FromArgb(
                _viewModel.StrokeColor.Color.A,
                _viewModel.StrokeColor.Color.R,
                _viewModel.StrokeColor.Color.G,
                _viewModel.StrokeColor.Color.B);
            strokePicker.SetColor(currentColor);
        }
        ToolSettingsBarControl.StrokeColorPopupElement.IsOpen = true;
    }

    private void OnFillColorSelected(Avalonia.Media.Color color)
    {
        if (_viewModel != null)
        {
            _viewModel.FillColor.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            _viewModel.StatusMessage = $"Цвет заливки: #{color}";
        }
        ToolSettingsBarControl.ColorPopupElement.IsOpen = false;
    }

    private void OnStrokeColorSelected(Avalonia.Media.Color color)
    {
        if (_viewModel != null)
        {
            _viewModel.StrokeColor.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            _viewModel.StatusMessage = $"Цвет обводки изменён: #{color}";
        }
        ToolSettingsBarControl.StrokeColorPopupElement.IsOpen = false;
    }

    private void OnColorPickerCancelled()
    {
        ToolSettingsBarControl.ColorPopupElement.IsOpen = false;
        ToolSettingsBarControl.StrokeColorPopupElement.IsOpen = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчики меню "Файл" ──────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private async void OnOpenMenuItem_Click(object? sender, RoutedEventArgs e)
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

    private async void OnSaveMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentFilePath == null)
            await SaveAsAsync();
        else
            await DoSaveAsync(_currentFilePath);
    }

    private async void OnSaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        await SaveAsAsync();
    }

    private async Task SaveAsAsync()
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
            await DoSaveAsync(path);
        }
    }

    private async Task DoSaveAsync(string path)
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

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчик экспорта ──────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private async void OnExportButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.Canvas == null) return;

        var fileTypes = new[]
        {
            new FilePickerFileType("PNG изображение (*.png)") { Patterns = ["*.png"] },
            new FilePickerFileType("JPEG изображение (*.jpg;*.jpeg)") { Patterns = ["*.jpg", "*.jpeg"] },
            new FilePickerFileType("BMP изображение (*.bmp)") { Patterns = ["*.bmp"] },
            new FilePickerFileType("PDF документ (*.pdf)") { Patterns = ["*.pdf"] },
        };

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Экспорт изображения",
            DefaultExtension = "png",
            FileTypeChoices = fileTypes,
            SuggestedFileName = "Безымянный"
        });

        if (file?.Path?.LocalPath is not string path) return;

        var vectorCanvas = EditorWorkspaceControl.VectorCanvasElement;
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

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчики меню "Справка" ───────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    private void OnAboutMenuItem_Click(object? sender, RoutedEventArgs e)
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
                    new Button { Content = "Закрыть", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right }
                }
            }
        };

        if (about.Content is StackPanel panel && panel.Children.LastOrDefault() is Button closeBtn)
        {
            closeBtn.Click += (s, args) => about.Close();
        }
        about.Show(this);
    }

    private void OnShortcutsMenuItem_Click(object? sender, RoutedEventArgs e)
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

    private async void OnDocumentationMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher != null)
                await launcher.LaunchUriAsync(new Uri("https://github.com/Dream-Team-PM/graphic_editor/wiki"));
        }
        catch { /* ignore */ }
    }

    private async void OnReportIssueMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher != null)
                await launcher.LaunchUriAsync(new Uri("https://github.com/Dream-Team-PM/graphic_editor/issues"));
        }
        catch { /* ignore */ }
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Обработчик ввода текста на холсте ────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_viewModel?.IsDrawing == true && _viewModel.CurrentTool == DrawingTool.Text)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.FinishTextInput();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
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
        if (keyText?.Length == 1)
        {
            var finalChar = e.KeyModifiers.HasFlag(KeyModifiers.Shift)
                ? keyText.ToUpper()
                : keyText.ToLower();
            text.Text += finalChar;
            text.NotifyTextChanged();
            e.Handled = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // ── Очистка ресурсов ─────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────

    protected override void OnClosed(EventArgs e)
    {
        // Отписка от событий для предотвращения утечек
        MainMenuBarControl.OpenClicked -= OnOpenMenuItem_Click;
        MainMenuBarControl.SaveClicked -= OnSaveMenuItem_Click;
        MainMenuBarControl.SaveAsClicked -= OnSaveAsMenuItem_Click;
        MainMenuBarControl.ExportClicked -= OnExportButton_Click;
        MainMenuBarControl.AboutClicked -= OnAboutMenuItem_Click;
        MainMenuBarControl.ShortcutsClicked -= OnShortcutsMenuItem_Click;
        MainMenuBarControl.DocumentationClicked -= OnDocumentationMenuItem_Click;
        MainMenuBarControl.ReportIssueClicked -= OnReportIssueMenuItem_Click;
        MainMenuBarControl.ThemeToggleChanged -= ThemeToggle_Changed;

        ToolSettingsBarControl.FillColorClicked -= OnFillColorButton_Click;
        ToolSettingsBarControl.StrokeColorClicked -= OnStrokeColorButton_Click;
        ToolSettingsBarControl.StrokeSliderValueChanged -= StrokeSlider_ValueChanged;

        ToolsSidebarControl.ToolButtonChecked -= ToolButton_Checked;

        EditorWorkspaceControl.VectorCanvasElement.PointerMoved -= OnCanvasPointerMoved;
        EditorWorkspaceControl.VectorCanvasElement.PointerPressed -= OnCanvasPointerPressed;
        EditorWorkspaceControl.VectorCanvasElement.PointerReleased -= OnCanvasPointerReleased;

        KeyDown -= OnWindowKeyDown;

        base.OnClosed(e);
    }
}