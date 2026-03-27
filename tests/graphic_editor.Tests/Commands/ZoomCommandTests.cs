// graphic_editor.Tests/Commands/ZoomCommandTests.cs
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.ViewModels;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class ZoomCommandTests
{
    [Fact]
    public void UndoRedo_ShouldChangeZoom()
    {
        var canvas = new CanvasViewModel();
        canvas.Zoom = 1.0;
        var cmd = new ZoomCommand(1.0, 2.0);
        cmd.SetCanvas(canvas);
        cmd.Redo();
        canvas.Zoom.Should().Be(2.0);
        cmd.Undo();
        canvas.Zoom.Should().Be(1.0);
    }
}