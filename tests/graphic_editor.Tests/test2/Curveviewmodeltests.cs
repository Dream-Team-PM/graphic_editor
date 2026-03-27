// Tests/Geometry/Figures/Curves/CurveViewModelTests.cs

using graphic_editor.Geometry;
using graphic_editor.Models;
using graphic_editor.ViewModels;
using System;
using System.Linq;
using Xunit;

namespace graphic_editor.Tests.Geometry;

/// <summary>
/// Абстрактный базовый класс тестов для всех кривых.
/// Конкретные реализации только указывают, какой класс создавать.
/// </summary>
public abstract class CurveViewModelTestsBase
{
    // Фабричный метод — переопределяется в каждом конкретном тест-классе
    protected abstract FigureViewModel CreateCurve(double x, double y, double width, double height);
    protected abstract FigureViewModel CreateCurve();

    // ─── Конструктор ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultArgs_SetsCorrectVertexCount()
    {
        var curve = CreateCurve();
        Assert.Equal(4, curve.Vertices.Count);
    }

    [Fact]
    public void Constructor_WithArgs_SetsCorrectXY()
    {
        var curve = CreateCurve(10, 20, 100, 200);
        Assert.Equal(10, GetX(curve));
        Assert.Equal(20, GetY(curve));
    }

    [Fact]
    public void Constructor_WithArgs_SetsCorrectWidthHeight()
    {
        var curve = CreateCurve(0, 0, 150, 75);
        Assert.Equal(150, GetWidth(curve));
        Assert.Equal(75, GetHeight(curve));
    }

    [Fact]
    public void Constructor_WithArgs_SetsCorrectCenter()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var center = curve.Center;
        Assert.Equal(50, center.X);
        Assert.Equal(50, center.Y);
    }

    [Fact]
    public void Constructor_WithArgs_SetsCorrectRadii()
    {
        var curve = CreateCurve(0, 0, 100, 60);
        Assert.Equal(50, GetRadiusX(curve));
        Assert.Equal(30, GetRadiusY(curve));
    }

    [Fact]
    public void Constructor_WithArgs_SetsNonEmptyName()
    {
        var curve = CreateCurve();
        Assert.False(string.IsNullOrWhiteSpace(curve.Name));
    }

    // ─── Move ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Move_ByDelta_ShiftsXAndY()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        curve.Move(30, 40);
        Assert.Equal(30, GetX(curve));
        Assert.Equal(40, GetY(curve));
    }

    [Fact]
    public void Move_ByDelta_PreservesWidthAndHeight()
    {
        var curve = CreateCurve(0, 0, 100, 80);
        curve.Move(50, 50);
        Assert.Equal(100, GetWidth(curve));
        Assert.Equal(80, GetHeight(curve));
    }

    [Fact]
    public void Move_NegativeDelta_MovesInOppositeDirection()
    {
        var curve = CreateCurve(50, 50, 100, 100);
        curve.Move(-50, -50);
        Assert.Equal(0, GetX(curve));
        Assert.Equal(0, GetY(curve));
    }

    [Fact]
    public void Move_ZeroDelta_DoesNotChangePosition()
    {
        var curve = CreateCurve(10, 20, 100, 100);
        curve.Move(0, 0);
        Assert.Equal(10, GetX(curve));
        Assert.Equal(20, GetY(curve));
    }

    // ─── Scale ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Scale_Doubles_DoublesWidthAndHeight()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        curve.Scale(2, 2);
        Assert.Equal(200, GetWidth(curve), precision: 10);
        Assert.Equal(200, GetHeight(curve), precision: 10);
    }

    [Fact]
    public void Scale_Half_HalvesWidthAndHeight()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        curve.Scale(0.5, 0.5);
        Assert.Equal(50, GetWidth(curve), precision: 10);
        Assert.Equal(50, GetHeight(curve), precision: 10);
    }

    [Fact]
    public void Scale_PreservesCenter()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var centerBefore = curve.Center;
        curve.Scale(3, 3);
        var centerAfter = curve.Center;
        Assert.Equal(centerBefore.X, centerAfter.X, precision: 10);
        Assert.Equal(centerBefore.Y, centerAfter.Y, precision: 10);
    }

    [Fact]
    public void Scale_NonUniform_ScalesAxesIndependently()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        curve.Scale(2, 0.5);
        Assert.Equal(200, GetWidth(curve), precision: 10);
        Assert.Equal(50, GetHeight(curve), precision: 10);
    }

    // ─── Rotate ────────────────────────────────────────────────────────────────

    [Fact]
    public void Rotate_By360_RestoresOriginalVertices()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var before = curve.Vertices.Select(v => (v.X, v.Y)).ToList();
        curve.Rotate(360);
        for (int i = 0; i < curve.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, curve.Vertices[i].X, precision: 10);
            Assert.Equal(before[i].Y, curve.Vertices[i].Y, precision: 10);
        }
    }

    [Fact]
    public void Rotate_By0_DoesNotChangeVertices()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var before = curve.Vertices.Select(v => (v.X, v.Y)).ToList();
        curve.Rotate(0);
        for (int i = 0; i < curve.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, curve.Vertices[i].X, precision: 10);
            Assert.Equal(before[i].Y, curve.Vertices[i].Y, precision: 10);
        }
    }

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var centerBefore = curve.Center;
        curve.Rotate(45);
        var centerAfter = curve.Center;
        Assert.Equal(centerBefore.X, centerAfter.X, precision: 10);
        Assert.Equal(centerBefore.Y, centerAfter.Y, precision: 10);
    }

    [Fact]
    public void Rotate_By180_FlipsVerticesAroundCenter()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var center = curve.Center;
        var firstBefore = (curve.Vertices[0].X, curve.Vertices[0].Y);
        curve.Rotate(180);
        // После поворота на 180° первая вершина должна быть симметрична центру
        Assert.Equal(2 * center.X - firstBefore.Item1, curve.Vertices[0].X, precision: 10);
        Assert.Equal(2 * center.Y - firstBefore.Item2, curve.Vertices[0].Y, precision: 10);
    }

    // ─── IsIn ──────────────────────────────────────────────────────────────────

    [Fact]
    public void IsIn_CenterPoint_ReturnsTrue()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        Assert.True(curve.IsIn(curve.Center));
    }

    [Fact]
    public void IsIn_PointFarOutside_ReturnsFalse()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        Assert.False(curve.IsIn(new Point2D(1000, 1000)));
    }

    [Fact]
    public void IsIn_PointOnEllipseBoundary_ReturnsTrue()
    {
        // Точка на правом краю эллипса (RadiusX, 0 от центра)
        var curve = CreateCurve(0, 0, 100, 100);
        var center = curve.Center;
        var boundaryPoint = new Point2D(center.X + GetRadiusX(curve), center.Y);
        Assert.True(curve.IsIn(boundaryPoint));
    }

    [Fact]
    public void IsIn_PointJustOutside_ReturnsFalse()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var center = curve.Center;
        // На 1 единицу дальше RadiusX
        var outside = new Point2D(center.X + GetRadiusX(curve) + 1, center.Y);
        Assert.False(curve.IsIn(outside, eps: 0.001));
    }

    // ─── GetVertexPoint ────────────────────────────────────────────────────────

    [Fact]
    public void GetVertexPoint_Returns4Points()
    {
        var curve = CreateCurve(0, 0, 100, 100);
        var points = curve.GetVertexPoint().ToList();
        Assert.Equal(4, points.Count);
    }

    [Fact]
    public void GetVertexPoint_FirstPointMatchesXY()
    {
        var curve = CreateCurve(10, 20, 100, 100);
        var first = curve.GetVertexPoint().First();
        Assert.Equal(10, first.X);
        Assert.Equal(20, first.Y);
    }

    // ─── Clone ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Clone_ThrowsNotImplementedException()
    {
        var curve = CreateCurve();
        Assert.Throws<NotImplementedException>(() => curve.Clone());
    }

    // ─── Вспомогательные методы через рефлексию ────────────────────────────────

    private static double GetX(FigureViewModel c) => (double)c.GetType().GetProperty("X")!.GetValue(c)!;
    private static double GetY(FigureViewModel c) => (double)c.GetType().GetProperty("Y")!.GetValue(c)!;
    private static double GetWidth(FigureViewModel c) => (double)c.GetType().GetProperty("Width")!.GetValue(c)!;
    private static double GetHeight(FigureViewModel c) => (double)c.GetType().GetProperty("Height")!.GetValue(c)!;
    private static double GetRadiusX(FigureViewModel c) => (double)c.GetType().GetProperty("RadiusX")!.GetValue(c)!;
    private static double GetRadiusY(FigureViewModel c) => (double)c.GetType().GetProperty("RadiusY")!.GetValue(c)!;
}

// ─── Конкретные тест-классы ────────────────────────────────────────────────────

public class BezieCurveViewModelTests : CurveViewModelTestsBase
{
    protected override FigureViewModel CreateCurve(double x, double y, double w, double h)
        => new BezieCurveViewModel(x, y, w, h);

    protected override FigureViewModel CreateCurve()
        => new BezieCurveViewModel();
}

public class CurveViewModelTests : CurveViewModelTestsBase
{
    protected override FigureViewModel CreateCurve(double x, double y, double w, double h)
        => new CurveViewModel(x, y, w, h);

    protected override FigureViewModel CreateCurve()
        => new CurveViewModel();
}

public class SplineViewModelTests : CurveViewModelTestsBase
{
    protected override FigureViewModel CreateCurve(double x, double y, double w, double h)
        => new SplineViewModel(x, y, w, h);

    protected override FigureViewModel CreateCurve()
        => new SplineViewModel();
}