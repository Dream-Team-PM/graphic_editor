// graphic_editor.Tests/Commands/MoveFigureCommandTests.cs
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class MoveFigureCommandTests
{
    [Fact]
    public void Execute_ShouldMoveFigures()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var fig = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(fig);
        var cmd = new MoveFigureCommand(new List<Guid> { fig.Id }, 10, 5);
        cmd.Execute(canvas);
        fig.X.Should().Be(10);
        fig.Y.Should().Be(5);
    }

    [Fact]
    public void Undo_ShouldRestoreOriginalPosition()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var fig = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(fig);
        var cmd = new MoveFigureCommand(new List<Guid> { fig.Id }, 10, 5);
        cmd.Execute(canvas);
        cmd.Undo();
        fig.X.Should().Be(0);
        fig.Y.Should().Be(0);
    }
}