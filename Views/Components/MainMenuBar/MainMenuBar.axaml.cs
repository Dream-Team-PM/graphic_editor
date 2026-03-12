using System;
using Avalonia.Controls;

namespace graphic_editor.Views.Components;

public partial class MainMenuBar : UserControl
{
    public MainMenuBar()
    {
        InitializeComponent();
        ThemeToggle.IsCheckedChanged += OnThemeToggleCheckedChanged;
    }

    public ToggleSwitch ThemeToggleElement => ThemeToggle;

    public event EventHandler<bool>? ThemeToggleChanged;

    private void OnThemeToggleCheckedChanged(object? sender, EventArgs e)
    {
        ThemeToggleChanged?.Invoke(this, ThemeToggle.IsChecked == true);
    }
}
