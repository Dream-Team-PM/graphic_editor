// graphic_editor.Tests/Commands/AddFigureCommandTests.cs
using System.Linq;
using FluentAssertions;
using graphic_editor.Commands;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using Moq;
using Xunit;

namespace graphic_editor.Tests.Commands;

public class AddFigureCommandTests
{
    [Fact]
    public void Execute_ShouldAddFigureToActiveLayer()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var figure = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var cmd = new AddFigureCommand(figure);
        cmd.Execute(canvas);
        canvas.ActiveLayer.Figures.Should().Contain(figure);
    }

    [Fact]
    public void Execute_ShouldNotAddIfAlreadyAdded()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var figure = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var cmd = new AddFigureCommand(figure);
        cmd.Execute(canvas);
        cmd.Execute(canvas);
        canvas.ActiveLayer.Figures.Should().HaveCount(1);
    }

    [Fact]
    public void Undo_ShouldRemoveAddedFigure()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var figure = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var cmd = new AddFigureCommand(figure);
        cmd.Execute(canvas);
        cmd.Undo();
        canvas.ActiveLayer.Figures.Should().BeEmpty();
    }

    [Fact]
    public void Redo_ShouldReAddFigure()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var figure = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var cmd = new AddFigureCommand(figure);
        cmd.Execute(canvas);
        cmd.Undo();
        cmd.Redo();
        canvas.ActiveLayer.Figures.Should().Contain(figure);
    }
}