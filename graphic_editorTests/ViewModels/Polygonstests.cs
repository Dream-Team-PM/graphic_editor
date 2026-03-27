using System;
using System.Drawing;
using System.Linq;
using ReactiveUI;
using Xunit;
using graphic_editor.Geometry;
using graphic_editor.Models;

namespace graphic_editorTests;

// Tests/Geometry/Figures/Polygons/PolygonsTests.cs
// ─── Базовый класс для правильных многоугольников ─────────────────────────────

public abstract class RegularPolygonTestsBase
{
    protected abstract RegularPolygonViewModel CreatePolygon(
        Point2D center, double radius);

    protected abstract int ExpectedSides { get; }

    private static readonly Point2D DefaultCenter = new Point2D(100, 100);
    private const double DefaultRadius = 50;

    private RegularPolygonViewModel Default()
        => CreatePolygon(DefaultCenter, DefaultRadius);

    // ─── Конструктор ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsCorrectSidesCount()
        => Assert.Equal(ExpectedSides, Default().Sides);

    [Fact]
    public void Constructor_SetsCorrectRadius()
        => Assert.Equal(DefaultRadius, Default().Radius, 10);

    [Fact]
    public void Constructor_SetsCorrectVertexCount()
        => Assert.Equal(ExpectedSides, Default().Vertices.Count);

    [Fact]
    public void Constructor_SetsCorrectCenter()
    {
        var p = Default();
        Assert.Equal(DefaultCenter.X, p.Center.X, 5);
        Assert.Equal(DefaultCenter.Y, p.Center.Y, 5);
    }

    [Fact]
    public void Constructor_SetsNonEmptyName()
        => Assert.False(string.IsNullOrWhiteSpace(Default().Name));

    [Fact]
    public void Constructor_AllVerticesOnCircumscribedCircle()
    {
        var p = Default();
        var center = new Point2D(DefaultCenter.X, DefaultCenter.Y);
        foreach (var v in p.Vertices)
        {
            var dist = Math.Sqrt(Math.Pow(v.X - center.X, 2) + Math.Pow(v.Y - center.Y, 2));
            Assert.Equal(DefaultRadius, dist, 5);
        }
    }

    // ─── Move ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Move_ShiftsCenter()
    {
        var p = Default();
        var before = p.Center;
        p.Move(20, 30);
        Assert.Equal(before.X + 20, p.Center.X, 10);
        Assert.Equal(before.Y + 30, p.Center.Y, 10);
    }

    [Fact]
    public void Move_PreservesVertexCount()
    {
        var p = Default();
        p.Move(10, 10);
        Assert.Equal(ExpectedSides, p.Vertices.Count);
    }

    [Fact]
    public void Move_Zero_DoesNotChangeCenter()
    {
        var p = Default();
        var before = p.Center;
        p.Move(0, 0);
        Assert.Equal(before.X, p.Center.X, 10);
        Assert.Equal(before.Y, p.Center.Y, 10);
    }

    // ─── Scale ────────────────────────────────────────────────────────────────

    [Fact]
    public void Scale_PreservesCenter()
    {
        var p = Default();
        var before = p.Center;
        p.Scale(2, 2);
        Assert.Equal(before.X, p.Center.X, 5);
        Assert.Equal(before.Y, p.Center.Y, 5);
    }

    [Fact]
    public void Scale_Doubles_DoublesDistancesFromCenter()
    {
        var p = Default();
        var center = p.Center;
        var distBefore = Math.Sqrt(
            Math.Pow(p.Vertices[0].X - center.X, 2) +
            Math.Pow(p.Vertices[0].Y - center.Y, 2));
        p.Scale(2, 2);
        var distAfter = Math.Sqrt(
            Math.Pow(p.Vertices[0].X - center.X, 2) +
            Math.Pow(p.Vertices[0].Y - center.Y, 2));
        Assert.Equal(distBefore * 2, distAfter, 5);
    }

    // ─── Rotate ───────────────────────────────────────────────────────────────

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var p = Default();
        var before = p.Center;
        p.Rotate(45);
        Assert.Equal(before.X, p.Center.X, 5);
        Assert.Equal(before.Y, p.Center.Y, 5);
    }

    [Fact]
    public void Rotate_360_RestoresVertices()
    {
        var p = Default();
        var before = p.Vertices.Select(v => (v.X, v.Y)).ToList();
        p.Rotate(360);
        for (int i = 0; i < p.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, p.Vertices[i].X, 5);
            Assert.Equal(before[i].Y, p.Vertices[i].Y, 5);
        }
    }

    [Fact]
    public void Rotate_0_DoesNotChangeVertices()
    {
        var p = Default();
        var before = p.Vertices.Select(v => (v.X, v.Y)).ToList();
        p.Rotate(0);
        for (int i = 0; i < p.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, p.Vertices[i].X, 10);
            Assert.Equal(before[i].Y, p.Vertices[i].Y, 10);
        }
    }

    // ─── IsIn ─────────────────────────────────────────────────────────────────

    [Fact]
    public void IsIn_Center_ReturnsTrue()
    {
        var p = Default();
        Assert.True(p.IsIn(p.Center));
    }

    [Fact]
    public void IsIn_FarOutside_ReturnsFalse()
    {
        var p = Default();
        Assert.False(p.IsIn(new Point2D(10000, 10000)));
    }

    // ─── GetVertexPoint ───────────────────────────────────────────────────────

    [Fact]
    public void GetVertexPoint_ReturnsCorrectCount()
        => Assert.Equal(ExpectedSides, Default().GetVertexPoint().Count());

    // ─── Clone ────────────────────────────────────────────────────────────────

    [Fact]
    public void Clone_ReturnsCopyWithSameCenter()
    {
        var p = Default();
        var clone = p.Clone();
        Assert.Equal(p.Center.X, clone.Center.X, 5);
        Assert.Equal(p.Center.Y, clone.Center.Y, 5);
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var p = Default();
        var clone = p.Clone();
        clone.Move(500, 500);
        Assert.Equal(DefaultCenter.X, p.Center.X, 5);
        Assert.Equal(DefaultCenter.Y, p.Center.Y, 5);
    }

    // ─── RegularPolygonViewModel.UpdateVertices ───────────────────────────────

    [Fact]
    public void UpdateVertices_ChangesVertexPositions()
    {
        var p = Default();
        var distBefore = Math.Sqrt(
            Math.Pow(p.Vertices[0].X - DefaultCenter.X, 2) +
            Math.Pow(p.Vertices[0].Y - DefaultCenter.Y, 2));
        p.UpdateVertices(DefaultCenter, DefaultRadius * 2);
        var distAfter = Math.Sqrt(
            Math.Pow(p.Vertices[0].X - DefaultCenter.X, 2) +
            Math.Pow(p.Vertices[0].Y - DefaultCenter.Y, 2));
        Assert.NotEqual(distBefore, distAfter, 5);
    }
}

// ─── Конкретные тест-классы для правильных многоугольников ───────────────────

public class PentagonViewModelTests : RegularPolygonTestsBase
{
    protected override int ExpectedSides => 5;
    protected override RegularPolygonViewModel CreatePolygon(Point2D center, double radius)
        => new PentagonViewModel(center, radius, Color.Black, 1, Color.Transparent, 1.0);
}

public class HexagonViewModelTests : RegularPolygonTestsBase
{
    protected override int ExpectedSides => 6;
    protected override RegularPolygonViewModel CreatePolygon(Point2D center, double radius)
        => new HexagonViewModel(center, radius, Color.Black, 1, Color.Transparent, 1.0);
}

public class HeptagonViewModelTests : RegularPolygonTestsBase
{
    protected override int ExpectedSides => 7;
    protected override RegularPolygonViewModel CreatePolygon(Point2D center, double radius)
        => new HeptagonViewModel(center, radius, Color.Black, 1, Color.Transparent, 1.0);
}

public class OctagonViewModelTests : RegularPolygonTestsBase
{
    protected override int ExpectedSides => 8;
    protected override RegularPolygonViewModel CreatePolygon(Point2D center, double radius)
        => new OctagonViewModel(center, radius, Color.Black, 1, Color.Transparent, 1.0);
}

// ─── RegularPolygonViewModel — граничные случаи ───────────────────────────────

public class RegularPolygonEdgeCaseTests
{
    [Fact]
    public void Pentagon_HasCorrectSidesCount()
    {
        var p = new PentagonViewModel(new Point2D(0, 0), 50,
            Color.Black, 1, Color.Transparent, 1.0);
        Assert.Equal(5, p.Sides);
    }
}

// ─── PentagramViewModel ───────────────────────────────────────────────────────

public class PentagramViewModelTests
{
    private static PentagramViewModel Create(double cx = 100, double cy = 100, double r = 50)
        => new PentagramViewModel(new Point2D(cx, cy), r,
            Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_Sets10Vertices()
        => Assert.Equal(10, Create().Vertices.Count);

    [Fact]
    public void Constructor_SetsCorrectOuterRadius()
        => Assert.Equal(50, Create().OuterRadius, 10);

    [Fact]
    public void Constructor_SetsCorrectCenter()
    {
        var p = Create(100, 100);
        Assert.Equal(100, p.Center.X, 5);
        Assert.Equal(100, p.Center.Y, 5);
    }

    [Fact]
    public void Constructor_SetsName()
        => Assert.False(string.IsNullOrWhiteSpace(Create().Name));

    [Fact]
    public void Move_ShiftsCenter()
    {
        var p = Create(100, 100);
        p.Move(50, 50);
        Assert.Equal(150, p.Center.X, 5);
        Assert.Equal(150, p.Center.Y, 5);
    }

    [Fact]
    public void Scale_PreservesCenter()
    {
        var p = Create(100, 100);
        var before = p.Center;
        p.Scale(2, 2);
        Assert.Equal(before.X, p.Center.X, 5);
        Assert.Equal(before.Y, p.Center.Y, 5);
    }

    [Fact]
    public void Rotate_360_RestoresVertices()
    {
        var p = Create();
        var before = p.Vertices.Select(v => (v.X, v.Y)).ToList();
        p.Rotate(360);
        for (int i = 0; i < p.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, p.Vertices[i].X, 5);
            Assert.Equal(before[i].Y, p.Vertices[i].Y, 5);
        }
    }

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var p = Create(100, 100);
        var before = p.Center;
        p.Rotate(45);
        Assert.Equal(before.X, p.Center.X, 5);
        Assert.Equal(before.Y, p.Center.Y, 5);
    }

    [Fact]
    public void IsIn_Center_ReturnsTrue()
    {
        var p = Create(100, 100);
        Assert.True(p.IsIn(p.Center));
    }

    [Fact]
    public void IsIn_FarOutside_ReturnsFalse()
    {
        var p = Create(100, 100);
        Assert.False(p.IsIn(new Point2D(10000, 10000)));
    }

    [Fact]
    public void GetVertexPoint_Returns10Points()
        => Assert.Equal(10, Create().GetVertexPoint().Count());

    [Fact]
    public void Clone_ReturnsCopyWithSameOuterRadius()
    {
        var p = Create(100, 100, 50);
        var clone = (PentagramViewModel)p.Clone();
        Assert.Equal(p.OuterRadius, clone.OuterRadius, 10);
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var p = Create(100, 100);
        var clone = p.Clone();
        clone.Move(500, 500);
        Assert.Equal(100, p.Center.X, 5);
    }

    [Fact]
    public void UpdateVertices_ChangesVertexPositions()
    {
        var p = Create(100, 100, 50);
        var before = p.Vertices[0].X;
        p.UpdateVertices(new Point2D(100, 100), 100);
        Assert.NotEqual(before, p.Vertices[0].X, 5);
    }
}

// ─── TriangleViewModel ────────────────────────────────────────────────────────

public class TriangleViewModelTests
{
    private static TriangleViewModel Create()
        => new TriangleViewModel(
            new Point2D(0, 0), new Point2D(100, 0), new Point2D(50, 80),
            Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_Sets3Vertices()
        => Assert.Equal(3, Create().Vertices.Count);

    [Fact]
    public void Constructor_SetsCorrectCenter()
    {
        var t = Create();
        Assert.Equal(50, t.Center.X, 5);
        Assert.InRange(t.Center.Y, 25, 30);
    }

    [Fact]
    public void Constructor_SetsName()
        => Assert.False(string.IsNullOrWhiteSpace(Create().Name));

    [Fact]
    public void Move_ShiftsCenter()
    {
        var t = Create();
        var before = t.Center;
        t.Move(30, 40);
        Assert.Equal(before.X + 30, t.Center.X, 10);
        Assert.Equal(before.Y + 40, t.Center.Y, 10);
    }

    [Fact]
    public void Move_PreservesVertexCount()
    {
        var t = Create();
        t.Move(10, 10);
        Assert.Equal(3, t.Vertices.Count);
    }

    [Fact]
    public void Scale_PreservesCenter()
    {
        var t = Create();
        var before = t.Center;
        t.Scale(2, 2);
        Assert.Equal(before.X, t.Center.X, 5);
        Assert.Equal(before.Y, t.Center.Y, 5);
    }

    [Fact]
    public void Rotate_360_RestoresVertices()
    {
        var t = Create();
        var before = t.Vertices.Select(v => (v.X, v.Y)).ToList();
        t.Rotate(360);
        for (int i = 0; i < t.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, t.Vertices[i].X, 5);
            Assert.Equal(before[i].Y, t.Vertices[i].Y, 5);
        }
    }

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var t = Create();
        var before = t.Center;
        t.Rotate(45);
        Assert.Equal(before.X, t.Center.X, 5);
        Assert.Equal(before.Y, t.Center.Y, 5);
    }

    [Fact]
    public void IsIn_Center_ReturnsTrue()
    {
        var t = Create();
        Assert.True(t.IsIn(t.Center));
    }

    [Fact]
    public void IsIn_FarOutside_ReturnsFalse()
    {
        var t = Create();
        Assert.False(t.IsIn(new Point2D(10000, 10000)));
    }

    [Fact]
    public void GetVertexPoint_Returns3Points()
        => Assert.Equal(3, Create().GetVertexPoint().Count());

    [Fact]
    public void Clone_ReturnsCopyWithSameVertices()
    {
        var t = Create();
        var clone = (TriangleViewModel)t.Clone();
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(t.Vertices[i].X, clone.Vertices[i].X, 10);
            Assert.Equal(t.Vertices[i].Y, clone.Vertices[i].Y, 10);
        }
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var t = Create();
        var clone = t.Clone();
        clone.Move(500, 500);
        Assert.Equal(0, t.Vertices[0].X);
    }
}