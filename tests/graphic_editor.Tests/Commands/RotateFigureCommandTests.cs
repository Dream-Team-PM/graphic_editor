// graphic_editor.Tests/Commands/RotateFigureCommandTests.cs
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class RotateFigureCommandTests
{
    [Fact]
    public void Execute_ShouldRotateFigure()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var rect = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(rect);
        var cmd = new RotateFigureCommand(new List<Guid> { rect.Id }, 90);
        cmd.Execute(canvas);
        // после поворота вершины должны измениться
        rect.Vertices[0].ToPoint().Should().NotBe(new Point2D(0, 0));
    }
}