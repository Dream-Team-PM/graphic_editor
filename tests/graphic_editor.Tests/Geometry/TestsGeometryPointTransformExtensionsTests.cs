// graphic_editor.Tests/Geometry/PointTransformExtensionsTests.cs
using FluentAssertions;
using graphic_editor.Geometry;
using Xunit;

namespace graphic_editor.Tests.Geometry;

public class PointTransformExtensionsTests
{
    [Fact]
    public void Rotate_ShouldRotatePoint()
    {
        var center = new Point2D(0, 0);
        var point = new Point2D(1, 0);
        var rotated = point.Rotate(center, 90);
        rotated.X.Should().BeApproximately(0, 1e-6);
        rotated.Y.Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void Scale_ShouldScalePoint()
    {
        var center = new Point2D(0, 0);
        var point = new Point2D(2, 3);
        var scaled = point.Scale(center, 2, 0.5);
        scaled.Should().Be(new Point2D(4, 1.5));
    }

    [Fact]
    public void Reflect_ShouldReflectPointOverLine()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(1, 0); // horizontal line
        var p = new Point2D(0, 1);
        var reflected = p.Reflect(a, b);
        reflected.Should().Be(new Point2D(0, -1));
    }
}