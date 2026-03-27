// graphic_editor.Tests/Geometry/Point2DTests.cs
using FluentAssertions;
using graphic_editor.Geometry;
using Xunit;

namespace graphic_editor.Tests.Geometry;

public class Point2DTests
{
    [Fact]
    public void Constructor_ShouldSetCoordinates()
    {
        var p = new Point2D(3.5, 4.2);
        p.X.Should().Be(3.5);
        p.Y.Should().Be(4.2);
    }

    [Fact]
    public void Zero_ShouldReturnOrigin()
    {
        var zero = Point2D.Zero;
        zero.X.Should().Be(0);
        zero.Y.Should().Be(0);
    }

    [Fact]
    public void Operators_ShouldWork()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);

        (a + b).Should().Be(new Point2D(4, 6));
        (a - b).Should().Be(new Point2D(-2, -2));
        (a * 2).Should().Be(new Point2D(2, 4));
        (2 * a).Should().Be(new Point2D(2, 4));
        (a / 2).Should().Be(new Point2D(0.5, 1));
    }

    [Fact]
    public void DistanceTo_ShouldReturnCorrectDistance()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        a.DistanceTo(b).Should().Be(5);
    }

    [Fact]
    public void DistanceToSq_ShouldReturnSquaredDistance()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        a.DistanceToSq(b).Should().Be(25);
    }

    [Fact]
    public void ScalePoint_StaticMethod_ShouldScale()
    {
        var p = new Point2D(2, 2);
        var center = new Point2D(1, 1);
        var scaled = Point2D.ScalePoint(p, center, 2, 3);
        scaled.Should().Be(new Point2D(3, 4));
    }

    [Fact]
    public void DistancePointToSegment_ShouldReturnCorrectDistance()
    {
        var p = new Point2D(0, 0);
        var a = new Point2D(1, 0);
        var b = new Point2D(1, 1);
        Point2D.DistancePointToSegment(p, a, b).Should().Be(1);
    }

    [Fact]
    public void IsPointNearSegment_ShouldReturnTrueWhenNear()
    {
        var p = new Point2D(1.001, 0);
        var a = new Point2D(1, 0);
        var b = new Point2D(1, 1);
        Point2D.IsPointNearSegment(p, a, b, 0.01).Should().BeTrue();
    }

    [Fact]
    public void IsPointNearSegment_ShouldReturnFalseWhenFar()
    {
        var p = new Point2D(1.1, 0);
        var a = new Point2D(1, 0);
        var b = new Point2D(1, 1);
        Point2D.IsPointNearSegment(p, a, b, 0.01).Should().BeFalse();
    }
}