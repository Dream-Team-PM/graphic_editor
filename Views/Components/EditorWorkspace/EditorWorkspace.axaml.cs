using Avalonia.Controls;
using graphic_editor.Controls;

namespace graphic_editor.Views.Components;

public partial class EditorWorkspace : UserControl
{
    public EditorWorkspace()
    {
        InitializeComponent();
    }

    public VectorCanvasControl VectorCanvasElement => VectorCanvas;
}
