using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

using graphic_editor.Helpers;
using graphic_editor.Services;
using graphic_editor.State;
using graphic_editor.ViewModels;

namespace graphic_editor;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

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

        ToolSettingsBarControl.SelectedToolTextElement.Text = "Выделение";
        ToolSettingsBarControl.StrokePercentTextElement.Text = "50%";

        MainMenuBarControl.ThemeToggleChanged += ThemeToggle_Changed;
        ToolSettingsBarControl.FillColorClicked += FillColorButton_Click;
        ToolSettingsBarControl.StrokeColorClicked += StrokeColorButton_Click;
        ToolSettingsBarControl.StrokeSliderValueChanged += StrokeSlider_ValueChanged;
        ToolsSidebarControl.ToolButtonChecked += ToolButton_Checked;

        EditorWorkspaceControl.VectorCanvasElement.PointerMoved += OnCanvasPointerMoved;
        EditorWorkspaceControl.VectorCanvasElement.PointerPressed += OnCanvasPointerPressed;
        EditorWorkspaceControl.VectorCanvasElement.PointerReleased += OnCanvasPointerReleased;

        MainMenuBarControl.ThemeToggleElement.IsChecked = _viewModel.CurrentTheme == ThemeVariant.Light;
    }

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
        var point = EditorWorkspaceControl.VectorCanvasElement.ScreenToCanvas(screenPos);
        _viewModel.HandlePointerPressed(e);
    }

    private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_viewModel == null) return;

        var screenPos = e.GetPosition(EditorWorkspaceControl.VectorCanvasElement);
        var point = EditorWorkspaceControl.VectorCanvasElement.ScreenToCanvas(screenPos);
        _viewModel.HandlePointerReleased(e);
    }

    private void ToolButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton btn && btn.IsChecked == true && btn.Tag is string toolName)
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: Setting SelectedTool to '{toolName}' (Tag={btn.Tag})");
            _viewModel?.SetToolByName(toolName);
            ToolSettingsBarControl.SelectedToolTextElement.Text = toolName;
        }
        else
        {
            DebugLog.Write($"[DEBUG] ToolButton_Checked: sender={sender?.GetType()}, IsChecked={(sender as RadioButton)?.IsChecked}, Tag={(sender as RadioButton)?.Tag}");
        }
    }

    private void StrokeSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        ToolSettingsBarControl.StrokePercentTextElement.Text = $"{(int)e.NewValue}%";
    }

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

    private void FillColorButton_Click(object? sender, RoutedEventArgs e)
    {
        var colorPicker = ToolSettingsBarControl.ColorPickerControlElement;
        colorPicker.ColorSelected -= OnFillColorSelected;
        colorPicker.ColorSelected += OnFillColorSelected;
        colorPicker.Cancelled += OnColorPickerCancelled;

        ToolSettingsBarControl.ColorPopupElement.IsOpen = true;
    }

    private void OnFillColorSelected(Avalonia.Media.Color color)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.FillColor.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            vm.StatusMessage = $"Цвет заливки: #{color}";
        }

        ToolSettingsBarControl.ColorPopupElement.IsOpen = false;
    }

    private void StrokeColorButton_Click(object? sender, RoutedEventArgs e)
    {
        var strokePicker = ToolSettingsBarControl.StrokeColorPickerControlElement;
        strokePicker.ColorSelected -= OnStrokeColorSelected;
        strokePicker.ColorSelected += OnStrokeColorSelected;
        strokePicker.Cancelled += OnColorPickerCancelled;

        if (_viewModel == null) return;

        var currentColor = Avalonia.Media.Color.FromArgb(
            _viewModel.StrokeColor.Color.A,
            _viewModel.StrokeColor.Color.R,
            _viewModel.StrokeColor.Color.G,
            _viewModel.StrokeColor.Color.B);
        strokePicker.SetColor(currentColor);

        ToolSettingsBarControl.StrokeColorPopupElement.IsOpen = true;
    }

    private void OnStrokeColorSelected(Avalonia.Media.Color color)
    {
        if (_viewModel != null)
        {
            var drawingColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            _viewModel.StrokeColor.Color = drawingColor;
            _viewModel.StatusMessage = $"Цвет обводки изменён: #{color}";
        }

        ToolSettingsBarControl.StrokeColorPopupElement.IsOpen = false;
    }

    private void OnColorPickerCancelled()
    {
        ToolSettingsBarControl.ColorPopupElement.IsOpen = false;
        ToolSettingsBarControl.StrokeColorPopupElement.IsOpen = false;
    }
}
