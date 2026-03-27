// graphic_editor.Tests/ViewModels/LayerViewModelTests.cs
using FluentAssertions;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using Xunit;

namespace graphic_editor.Tests.ViewModels;

public class LayerViewModelTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        var layer = new LayerViewModel("MyLayer");
        layer.Name.Should().Be("MyLayer");
        layer.IsVisible.Should().BeTrue();
        layer.IsLocked.Should().BeFalse();
        layer.Figures.Should().BeEmpty();
        layer.FigureCount.Should().Be(0);
    }

    [Fact]
    public void AddFigure_ShouldAddToCollectionAndUpdateCount()
    {
        var layer = new LayerViewModel("Test");
        var figure = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        layer.AddFigure(figure);
        layer.Figures.Should().Contain(figure);
        layer.FigureCount.Should().Be(1);
    }

    [Fact]
    public void RemoveFigure_ShouldRemoveFromCollectionAndUpdateCount()
    {
        var layer = new LayerViewModel("Test");
        var figure = new RectangleViewModel(0, 0, 10, 10, System.Drawing.Color.Black, 1, System.Drawing.Color.Transparent, 1);
        layer.AddFigure(figure);
        layer.RemoveFigure(figure);
        layer.Figures.Should().BeEmpty();
        layer.FigureCount.Should().Be(0);
    }
}