// graphic_editor.Tests/Commands/CompositeCommandTests.cs
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.Geometry;
using graphic_editor.Interfaces;
using graphic_editor.ViewModels;
using Moq;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class CompositeCommandTests
{
    [Fact]
    public void Execute_ShouldExecuteAllCommands()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var mockCmd1 = new Mock<IHistoryAction>();
        var mockCmd2 = new Mock<IHistoryAction>();
        var composite = new CompositeCommand("Test", mockCmd1.Object, mockCmd2.Object);
        composite.Execute(canvas);
        mockCmd1.Verify(c => c.Redo(), Times.Once);
        mockCmd2.Verify(c => c.Redo(), Times.Once);
    }

    [Fact]
    public void Undo_ShouldUndoInReverseOrder()
    {
        var mockCmd1 = new Mock<IHistoryAction>();
        var mockCmd2 = new Mock<IHistoryAction>();
        var composite = new CompositeCommand("Test", mockCmd1.Object, mockCmd2.Object);
        composite.Undo();
        mockCmd2.Verify(c => c.Undo(), Times.Once);
        mockCmd1.Verify(c => c.Undo(), Times.Once);
    }
}