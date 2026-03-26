using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace graphic_editor.Views.Components;

public partial class MainMenuBar : UserControl
{
    // ── События для меню "Файл" ──
    public event EventHandler<RoutedEventArgs>? OpenClicked;
    public event EventHandler<RoutedEventArgs>? SaveClicked;
    public event EventHandler<RoutedEventArgs>? SaveAsClicked;
    
    // ── Событие для кнопки экспорта ──
    public event EventHandler<RoutedEventArgs>? ExportClicked;
    
    // ── События для меню "Справка" ──
    public event EventHandler<RoutedEventArgs>? AboutClicked;
    public event EventHandler<RoutedEventArgs>? ShortcutsClicked;
    public event EventHandler<RoutedEventArgs>? DocumentationClicked;
    public event EventHandler<RoutedEventArgs>? ReportIssueClicked;
    
    // ── Событие переключения темы ──
    public event EventHandler<bool>? ThemeToggleChanged;

    public ToggleSwitch ThemeToggleElement => ThemeToggle;
    public Button ExportButtonElement => ExportButton;

    public MainMenuBar()
    {
        InitializeComponent();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        // Меню "Файл" - добавляем x:Name в XAML для этих элементов
        if (OpenMenuItem is MenuItem openItem)
            openItem.Click += (s, e) => OpenClicked?.Invoke(s, e);
        
        if (SaveMenuItem is MenuItem saveItem)
            saveItem.Click += (s, e) => SaveClicked?.Invoke(s, e);
        
        if (SaveAsMenuItem is MenuItem saveAsItem)
            saveAsItem.Click += (s, e) => SaveAsClicked?.Invoke(s, e);

        // Кнопка экспорта
        ExportButton.Click += (s, e) => ExportClicked?.Invoke(s, e);

        // Меню "Справка"
        if (AboutMenuItem is MenuItem aboutItem)
            aboutItem.Click += (s, e) => AboutClicked?.Invoke(s, e);
        
        if (ShortcutsMenuItem is MenuItem shortcutsItem)
            shortcutsItem.Click += (s, e) => ShortcutsClicked?.Invoke(s, e);
        
        if (DocumentationMenuItem is MenuItem docItem)
            docItem.Click += (s, e) => DocumentationClicked?.Invoke(s, e);
        
        if (ReportIssueMenuItem is MenuItem reportItem)
            reportItem.Click += (s, e) => ReportIssueClicked?.Invoke(s, e);

        // Переключатель темы
        ThemeToggle.IsCheckedChanged += OnThemeToggleChanged;
    }

    private void OnThemeToggleChanged(object? sender, EventArgs e)
    {
        ThemeToggleChanged?.Invoke(this, ThemeToggle.IsChecked == true);
    }
}