// graphic_editor.Tests/Commands/DeleteFigureCommandTests.cs
using System.Collections.Generic;
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class DeleteFigureCommandTests
{
    [Fact]
    public void Execute_ShouldRemoveFiguresFromLayer()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var fig1 = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var fig2 = new RectangleViewModel(20, 20, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(fig1);
        canvas.AddFigure(fig2);
        var cmd = new DeleteFigureCommand(new List<FigureViewModel> { fig1, fig2 });
        cmd.Execute(canvas);
        canvas.ActiveLayer.Figures.Should().BeEmpty();
    }

    [Fact]
    public void Undo_ShouldRestoreDeletedFigures()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var fig1 = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var fig2 = new RectangleViewModel(20, 20, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(fig1);
        canvas.AddFigure(fig2);
        var cmd = new DeleteFigureCommand(new List<FigureViewModel> { fig1, fig2 });
        cmd.Execute(canvas);
        cmd.Undo();
        canvas.ActiveLayer.Figures.Should().HaveCount(2);
        canvas.ActiveLayer.Figures.Should().Contain(fig1);
        canvas.ActiveLayer.Figures.Should().Contain(fig2);
    }
}