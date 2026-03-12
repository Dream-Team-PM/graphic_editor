using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace graphic_editor.Views.Components;

public partial class ToolsSidebar : UserControl
{
    public ToolsSidebar()
    {
        InitializeComponent();
    }

    public event EventHandler<RoutedEventArgs>? ToolButtonChecked;

    private void OnToolButtonChecked(object? sender, RoutedEventArgs e)
    {
        ToolButtonChecked?.Invoke(sender, e);
    }
}
