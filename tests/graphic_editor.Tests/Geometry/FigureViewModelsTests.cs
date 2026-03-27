// graphic_editor.Tests/Geometry/FigureViewModelsTests.cs
using System.Drawing;
using System.Linq;
using FluentAssertions;
using graphic_editor.Geometry;
using graphic_editor.ViewModels;
using Xunit;

namespace graphic_editor.Tests.Geometry;

public class FigureViewModelsTests
{
    [Fact]
    public void RectangleViewModel_Properties_ShouldBeCorrect()
    {
        var rect = new RectangleViewModel(10, 20, 100, 50, Color.Black, 2, Color.Red, 0.8);
        rect.X.Should().Be(10);
        rect.Y.Should().Be(20);
        rect.Width.Should().Be(100);
        rect.Height.Should().Be(50);
        rect.Center.Should().Be(new Point2D(60, 45));
        rect.LineColor.Should().Be(Color.Black);
        rect.Thickness.Should().Be(2);
        rect.FillColor.Should().Be(Color.Red);
        rect.Opacity.Should().Be(0.8);
    }

    [Fact]
    public void RectangleViewModel_Move_ShouldShiftVertices()
    {
        var rect = new RectangleViewModel(10, 20, 100, 50, Color.Black, 2, Color.Red, 1);
        rect.Move(5, -3);
        rect.X.Should().Be(15);
        rect.Y.Should().Be(17);
        rect.Center.Should().Be(new Point2D(65, 42));
    }

    [Fact]
    public void RectangleViewModel_Rotate_ShouldRotateVertices()
    {
        var rect = new RectangleViewModel(0, 0, 100, 50, Color.Black, 1, Color.Transparent, 1);
        rect.Rotate(90);
        rect.Center.Should().Be(new Point2D(50, 25));
        // Проверяем, что после поворота координаты вершин изменились (не равны исходным)
        rect.Vertices[0].ToPoint().Should().NotBe(new Point2D(0, 0));
        rect.Vertices[0].X.Should().BeApproximately(75, 1e-6);
        rect.Vertices[0].Y.Should().BeApproximately(-25, 1e-6);
    }

    [Fact]
    public void RectangleViewModel_Scale_ShouldScaleVertices()
    {
        var rect = new RectangleViewModel(10, 10, 100, 100, Color.Black, 1, Color.Transparent, 1);
        rect.Scale(2, 0.5);
        // Проверяем, что размеры изменились в соответствии с коэффициентами
        rect.Width.Should().Be(200);
        rect.Height.Should().Be(50);
        // Центр должен остаться тем же, так как масштабирование относительно центра
        rect.Center.Should().Be(new Point2D(60, 60));
    }

    [Fact]
    public void RectangleViewModel_IsIn_ShouldReturnTrueForPointInside()
    {
        var rect = new RectangleViewModel(10, 20, 100, 50, Color.Black, 1, Color.Transparent, 1);
        rect.IsIn(new Point2D(60, 45)).Should().BeTrue();
    }

    [Fact]
    public void RectangleViewModel_IsIn_ShouldReturnFalseForPointOutside()
    {
        var rect = new RectangleViewModel(10, 20, 100, 50, Color.Black, 1, Color.Transparent, 1);
        rect.IsIn(new Point2D(0, 0)).Should().BeFalse();
    }



    [Fact]
    public void CircleViewModel_Scale_ShouldKeepCircularShape()
    {
        var circle = new CircleViewModel(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1);
        circle.Scale(2, 1);
        // После масштабирования круг должен остаться кругом (ширина == высота)
        circle.Width.Should().Be(circle.Height);
        // Размеры должны увеличиться (хоть бы немного)
        circle.Width.Should().BeGreaterThan(100);
    }

    [Fact]
    public void LineViewModel_IsIn_ShouldDetectPointNearLine()
    {
        var line = new LineViewModel(0, 0, 10, 0, Color.Black, 1, Color.Transparent, 1);
        line.IsIn(new Point2D(5, 0.01), 0.02).Should().BeTrue();
        line.IsIn(new Point2D(5, 0.1), 0.02).Should().BeFalse();
    }

    [Fact]
    public void SquareViewModel_Properties_ShouldEnforceEqualSides()
    {
        var square = new SquareViewModel(0, 0, 200, 100, Color.Black, 1, Color.Transparent, 1);
        square.Width.Should().Be(200);
        square.Height.Should().Be(200);
        square.Side.Should().Be(200);
    }

    [Fact]
    public void TriangleViewModel_IsIn_ShouldReturnTrueForPointInside()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(10, 0);
        var c = new Point2D(0, 10);
        var triangle = new TriangleViewModel(a, b, c, Color.Black, 1, Color.Transparent, 1);
        triangle.IsIn(new Point2D(2, 2)).Should().BeTrue();
        triangle.IsIn(new Point2D(6, 6)).Should().BeFalse();
    }

    [Fact]
    public void RightTriangleViewModel_ShouldCreateThreeVertices()
    {
        var tri = new RightTriangleViewModel(10, 20, 100, 50, Color.Black, 1, Color.Transparent, 1);
        tri.Vertices.Count.Should().Be(3);
        tri.X.Should().Be(10);
        tri.Y.Should().Be(20);
        tri.Width.Should().Be(100);
        tri.Height.Should().Be(50);
    }

    [Fact]
    public void RhombusViewModel_ShouldCreateFourVertices()
    {
        var rhombus = new RhombusViewModel(100, 100, 80, 120, Color.Black, 1, Color.Transparent, 1);
        rhombus.Vertices.Count.Should().Be(4);
        rhombus.Center.Should().Be(new Point2D(100, 100));
        rhombus.Width.Should().Be(80);
        rhombus.Height.Should().Be(120);
    }

    [Fact]
    public void RegularPolygonViewModel_ShouldGenerateVertices()
    {
        var pentagon = new PentagonViewModel(new Point2D(0, 0), 50, Color.Black, 1, Color.Transparent, 1);
        pentagon.Vertices.Count.Should().Be(5);
        pentagon.Vertices[0].Y.Should().BeApproximately(-50, 1e-6);
    }

    [Fact]
    public void PentagramViewModel_ShouldGenerateTenVertices()
    {
        var star = new PentagramViewModel(new Point2D(0, 0), 50, Color.Black, 1, Color.Transparent, 1);
        star.Vertices.Count.Should().Be(10);
    }

    [Fact]
    public void GroupViewModel_ShouldMoveAllChildren()
    {
        var rect = new RectangleViewModel(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1);
        var circle = new CircleViewModel(0, 0, 50, Color.Black, 1, Color.Transparent, 1);
        var group = new GroupViewModel(new FigureViewModel[] { rect, circle });
        double rectX0 = rect.X;
        double rectY0 = rect.Y;
        double circleX0 = circle.X;
        double circleY0 = circle.Y;
        group.Move(10, 5);
        rect.X.Should().Be(rectX0 + 10);
        rect.Y.Should().Be(rectY0 + 5);
        circle.X.Should().Be(circleX0 + 10);
        circle.Y.Should().Be(circleY0 + 5);
    }

    [Fact]
    public void GroupViewModel_GetBoundingBox_ShouldEncompassAllChildren()
    {
        var rect = new RectangleViewModel(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1);
        var circle = new CircleViewModel(150, 150, 20, Color.Black, 1, Color.Transparent, 1);
        var group = new GroupViewModel(new FigureViewModel[] { rect, circle });
        var bbox = group.GetBoundingBox();
        bbox.MinX.Should().Be(0);
        bbox.MaxX.Should().Be(170);
        bbox.MinY.Should().Be(0);
        bbox.MaxY.Should().Be(170);
    }

    [Fact]
    public void GroupViewModel_Ungroup_ShouldReturnChildren()
    {
        var rect = new RectangleViewModel(0, 0, 100, 100, Color.Black, 1, Color.Transparent, 1);
        var circle = new CircleViewModel(0, 0, 50, Color.Black, 1, Color.Transparent, 1);
        var group = new GroupViewModel(new FigureViewModel[] { rect, circle });
        var children = group.Ungroup().ToList();
        children.Count.Should().Be(2);
        children.Should().Contain(rect);
        children.Should().Contain(circle);
    }
}