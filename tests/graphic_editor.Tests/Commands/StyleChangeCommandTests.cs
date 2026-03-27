// graphic_editor.Tests/Commands/StyleChangeCommandTests.cs
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class StyleChangeCommandTests
{
    [Fact]
    public void Execute_ShouldChangeLineColor()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var rect = new RectangleViewModel(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1);
        canvas.AddFigure(rect);
        var newColor = Color.Red;
        var cmd = new StyleChangeCommand(new List<Guid> { rect.Id }, newColor, null, null, null);
        cmd.Execute(canvas);
        rect.LineColor.Should().Be(newColor);
    }

    [Fact]
    public void Undo_ShouldRestoreOriginalStyle()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var rect = new RectangleViewModel(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1);
        canvas.AddFigure(rect);
        var newColor = Color.Red;
        var cmd = new StyleChangeCommand(new List<Guid> { rect.Id }, newColor, null, null, null);
        cmd.Execute(canvas);
        cmd.Undo();
        rect.LineColor.Should().Be(Color.Black);
    }
}