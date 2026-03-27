// graphic_editor.Tests/ViewModels/CanvasViewModelTests.cs
using System;
using System.Linq;
using FluentAssertions;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using Xunit;

namespace graphic_editor.Tests.ViewModels;

public class CanvasViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyLayers()
    {
        var canvas = new CanvasViewModel();
        canvas.Layers.Should().BeEmpty();
        canvas.ActiveLayer.Should().BeNull();
        canvas.IsCanvasActive.Should().BeFalse();
    }

    [Fact]
    public void ActivateCanvas_ShouldCreateLayerAndSetActive()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        canvas.Layers.Should().HaveCount(1);
        canvas.ActiveLayer.Should().NotBeNull();
        canvas.IsCanvasActive.Should().BeTrue();
    }

    [Fact]
    public void AddFigure_ShouldAddToActiveLayer()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var figure = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(figure);
        canvas.ActiveLayer.Figures.Should().Contain(figure);
        canvas.SelectedFigure.Should().Be(figure);
    }

    [Fact]
    public void RemoveSelectedFigure_ShouldRemoveFromLayer()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var figure = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(figure);
        canvas.RemoveSelectedFigure();
        canvas.ActiveLayer.Figures.Should().BeEmpty();
        canvas.SelectedFigure.Should().BeNull();
    }

    [Fact]
    public void SelectFigureAt_ShouldSelectFigure()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var rect = new RectangleViewModel(0, 0, 100, 100, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(rect);
        canvas.SelectFigureAt(new Point2D(50, 50));
        canvas.SelectedFigure.Should().Be(rect);
        rect.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void SelectFigureAt_WithAddToSelection_ShouldAddToSelection()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var rect1 = new RectangleViewModel(0, 0, 50, 50, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var rect2 = new RectangleViewModel(60, 60, 50, 50, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(rect1);
        canvas.AddFigure(rect2);
        canvas.SelectFigureAt(new Point2D(25, 25));
        canvas.SelectFigureAt(new Point2D(85, 85), addToSelection: true);
        canvas.SelectedFigures.Should().HaveCount(2);
        canvas.SelectedFigures.Should().Contain(rect1);
        canvas.SelectedFigures.Should().Contain(rect2);
    }

    [Fact]
    public void DuplicateSelectedFigure_ShouldCloneAndMove()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var rect = new RectangleViewModel(0, 0, 50, 50, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(rect);
        canvas.DuplicateSelectedFigure();
        canvas.ActiveLayer.Figures.Should().HaveCount(2);
        var clone = canvas.ActiveLayer.Figures.Last();
        clone.Should().NotBe(rect);
        clone.Center.X.Should().Be(rect.Center.X + 10);
        clone.Center.Y.Should().Be(rect.Center.Y + 10);
    }

    [Fact]
    public void Zoom_ShouldClampToRange()
    {
        var canvas = new CanvasViewModel();
        canvas.Zoom = 0.05;
        canvas.Zoom.Should().Be(0.1);
        canvas.Zoom = 20;
        canvas.Zoom.Should().Be(10);
    }

    [Fact]
    public void SelectedFigure_ShouldUpdateSelectedFiguresCollection()
    {
        var canvas = new CanvasViewModel();
        var figure = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.SelectedFigure = figure;
        canvas.SelectedFigures.Should().ContainSingle().Which.Should().Be(figure);
        canvas.SelectedFigure = null;
        canvas.SelectedFigures.Should().BeEmpty();
    }

    [Fact]
    public void BringToFront_ShouldMoveFigureToEndOfCollection()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var fig1 = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var fig2 = new RectangleViewModel(0, 0, 20, 20, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(fig1);
        canvas.AddFigure(fig2);
        canvas.SelectedFigure = fig1;
        canvas.BringToFront();
        canvas.ActiveLayer.Figures.Last().Should().Be(fig1);
    }

    [Fact]
    public void SendToBack_ShouldMoveFigureToBeginning()
    {
        var canvas = new CanvasViewModel();
        canvas.ActivateCanvas();
        var fig1 = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        var fig2 = new RectangleViewModel(0, 0, 20, 20, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        canvas.AddFigure(fig1);
        canvas.AddFigure(fig2);
        canvas.SelectedFigure = fig2;
        canvas.SendToBack();
        canvas.ActiveLayer.Figures.First().Should().Be(fig2);
    }
}