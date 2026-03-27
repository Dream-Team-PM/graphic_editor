// Tests/Geometry/Figures/Primitives/PrimitivesTests.cs

using System;
using System.Drawing;
using System.Linq;
using Xunit;
using graphic_editor.Geometry;
using graphic_editor.Models;

namespace graphic_editorTests;

// ─── EllipseViewModel ─────────────────────────────────────────────────────────

public class EllipseViewModelTests
{
    private static EllipseViewModel Create(double x = 0, double y = 0, double w = 100, double h = 60)
        => new EllipseViewModel(x, y, w, h, Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_SetsCorrectXY()
    {
        var e = Create(10, 20);
        Assert.Equal(10, e.X);
        Assert.Equal(20, e.Y);
    }

    [Fact]
    public void Constructor_SetsCorrectWidthHeight()
    {
        var e = Create(0, 0, 150, 80);
        Assert.Equal(150, e.Width);
        Assert.Equal(80, e.Height);
    }

    [Fact]
    public void Constructor_SetsCorrectRadii()
    {
        var e = Create(0, 0, 100, 60);
        Assert.Equal(50, e.RadiusX);
        Assert.Equal(30, e.RadiusY);
    }

    [Fact]
    public void Constructor_SetsCorrectCenter()
    {
        var e = Create(0, 0, 100, 60);
        Assert.Equal(50, e.Center.X, 10);
        Assert.Equal(30, e.Center.Y, 10);
    }

    [Fact]
    public void Move_ShiftsPosition()
    {
        var e = Create(0, 0);
        e.Move(20, 30);
        Assert.Equal(20, e.X);
        Assert.Equal(30, e.Y);
    }

    [Fact]
    public void Move_PreservesSize()
    {
        var e = Create(0, 0, 100, 60);
        e.Move(50, 50);
        Assert.Equal(100, e.Width, 10);
        Assert.Equal(60, e.Height, 10);
    }

    [Fact]
    public void Scale_ChangesSize()
    {
        var e = Create(0, 0, 100, 60);
        e.Scale(2, 2);
        Assert.Equal(200, e.Width, 10);
        Assert.Equal(120, e.Height, 10);
    }

    [Fact]
    public void Scale_PreservesCenter()
    {
        var e = Create(0, 0, 100, 60);
        var before = e.Center;
        e.Scale(3, 3);
        Assert.Equal(before.X, e.Center.X, 10);
        Assert.Equal(before.Y, e.Center.Y, 10);
    }

    [Fact]
    public void Rotate_360_RestoresVertices()
    {
        var e = Create(0, 0, 100, 60);
        var before = e.Vertices.Select(v => (v.X, v.Y)).ToList();
        e.Rotate(360);
        for (int i = 0; i < e.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, e.Vertices[i].X, 10);
            Assert.Equal(before[i].Y, e.Vertices[i].Y, 10);
        }
    }

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var e = Create(0, 0, 100, 60);
        var before = e.Center;
        e.Rotate(45);
        Assert.Equal(before.X, e.Center.X, 10);
        Assert.Equal(before.Y, e.Center.Y, 10);
    }

    [Fact]
    public void IsIn_Center_ReturnsTrue()
        => Assert.True(Create().IsIn(Create().Center));

    [Fact]
    public void IsIn_Outside_ReturnsFalse()
        => Assert.False(Create().IsIn(new Point2D(1000, 1000)));

    [Fact]
    public void GetVertexPoint_Returns4Points()
        => Assert.Equal(4, Create().GetVertexPoint().Count());

    [Fact]
    public void Clone_ReturnsEllipseWithSameProperties()
    {
        var e = Create(10, 20, 100, 60);
        var clone = (EllipseViewModel)e.Clone();
        Assert.Equal(e.X, clone.X, 10);
        Assert.Equal(e.Y, clone.Y, 10);
        Assert.Equal(e.Width, clone.Width, 10);
        Assert.Equal(e.Height, clone.Height, 10);
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var e = Create(0, 0, 100, 60);
        var clone = (EllipseViewModel)e.Clone();
        clone.Move(50, 50);
        Assert.Equal(0, e.X);
    }
}

// ─── CircleViewModel ──────────────────────────────────────────────────────────

public class CircleViewModelTests
{
    private static CircleViewModel Create(double cx = 50, double cy = 50, double r = 50)
        => new CircleViewModel(cx, cy, r, Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_SetsEqualRadii()
    {
        var c = Create(0, 0, 50);
        Assert.Equal(c.RadiusX, c.RadiusY, 10);
    }

    [Fact]
    public void Constructor_SetsCorrectRadius()
    {
        var c = Create(0, 0, 50);
        Assert.Equal(50, c.Radius, 10);
    }

    [Fact]
    public void Scale_PreservesCircleShape()
    {
        var c = Create(50, 50, 50);
        c.Scale(2, 3); // Неравномерное — должно усреднять
        Assert.Equal(c.RadiusX, c.RadiusY, 10);
    }

    [Fact]
    public void IsIn_Center_ReturnsTrue()
    {
        var c = Create(50, 50, 50);
        Assert.True(c.IsIn(new Point2D(50, 50)));
    }

    [Fact]
    public void IsIn_Outside_ReturnsFalse()
    {
        var c = Create(50, 50, 50);
        Assert.False(c.IsIn(new Point2D(200, 200)));
    }

    [Fact]
    public void IsIn_OnBoundary_ReturnsTrue()
    {
        var c = Create(50, 50, 50);
        Assert.True(c.IsIn(new Point2D(100, 50))); // Правая граница
    }

    [Fact]
    public void Move_PreservesRadius()
    {
        var c = Create(50, 50, 50);
        var before = c.Radius;
        c.Move(30, 30);
        Assert.Equal(before, c.Radius, 10);
    }
}

// ─── RectangleViewModel ───────────────────────────────────────────────────────

public class RectangleViewModelTests
{
    private static RectangleViewModel Create(double x = 0, double y = 0, double w = 100, double h = 80)
        => new RectangleViewModel(x, y, w, h, Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_SetsCorrectXYWH()
    {
        var r = Create(10, 20, 150, 75);
        Assert.Equal(10, r.X);
        Assert.Equal(20, r.Y);
        Assert.Equal(150, r.Width);
        Assert.Equal(75, r.Height);
    }

    [Fact]
    public void Constructor_SetsCorrectCenter()
    {
        var r = Create(0, 0, 100, 80);
        Assert.Equal(50, r.Center.X, 10);
        Assert.Equal(40, r.Center.Y, 10);
    }

    [Fact]
    public void Move_ShiftsPosition()
    {
        var r = Create(0, 0);
        r.Move(25, 35);
        Assert.Equal(25, r.X);
        Assert.Equal(35, r.Y);
    }

    [Fact]
    public void Move_PreservesSize()
    {
        var r = Create(0, 0, 100, 80);
        r.Move(50, 50);
        Assert.Equal(100, r.Width, 10);
        Assert.Equal(80, r.Height, 10);
    }

    [Fact]
    public void Scale_ChangesSize()
    {
        var r = Create(0, 0, 100, 80);
        r.Scale(2, 2);
        Assert.Equal(200, r.Width, 10);
        Assert.Equal(160, r.Height, 10);
    }

    [Fact]
    public void Scale_PreservesCenter()
    {
        var r = Create(0, 0, 100, 80);
        var before = r.Center;
        r.Scale(3, 3);
        Assert.Equal(before.X, r.Center.X, 10);
        Assert.Equal(before.Y, r.Center.Y, 10);
    }

    [Fact]
    public void Rotate_360_RestoresVertices()
    {
        var r = Create(0, 0, 100, 80);
        var before = r.Vertices.Select(v => (v.X, v.Y)).ToList();
        r.Rotate(360);
        for (int i = 0; i < r.Vertices.Count; i++)
        {
            Assert.Equal(before[i].X, r.Vertices[i].X, 10);
            Assert.Equal(before[i].Y, r.Vertices[i].Y, 10);
        }
    }

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var r = Create(0, 0, 100, 80);
        var before = r.Center;
        r.Rotate(45);
        Assert.Equal(before.X, r.Center.X, 10);
        Assert.Equal(before.Y, r.Center.Y, 10);
    }

    [Fact]
    public void IsIn_Center_ReturnsTrue()
    {
        var r = Create(0, 0, 100, 80);
        Assert.True(r.IsIn(r.Center));
    }

    [Fact]
    public void IsIn_Outside_ReturnsFalse()
    {
        var r = Create(0, 0, 100, 80);
        Assert.False(r.IsIn(new Point2D(200, 200)));
    }

    [Fact]
    public void IsIn_InsidePoint_ReturnsTrue()
    {
        var r = Create(0, 0, 100, 80);
        Assert.True(r.IsIn(new Point2D(10, 10)));
    }

    [Fact]
    public void Clone_ReturnsCopyWithSameProperties()
    {
        var r = Create(10, 20, 100, 80);
        var clone = (RectangleViewModel)r.Clone();
        Assert.Equal(r.X, clone.X, 10);
        Assert.Equal(r.Y, clone.Y, 10);
        Assert.Equal(r.Width, clone.Width, 10);
        Assert.Equal(r.Height, clone.Height, 10);
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var r = Create(0, 0, 100, 80);
        var clone = (RectangleViewModel)r.Clone();
        clone.Move(50, 50);
        Assert.Equal(0, r.X);
    }

    [Fact]
    public void GetVertexPoint_Returns4Points()
        => Assert.Equal(4, Create().GetVertexPoint().Count());
}

// ─── SquareViewModel ──────────────────────────────────────────────────────────

public class SquareViewModelTests
{
    private static SquareViewModel Create(double x = 0, double y = 0, double side = 100)
        => new SquareViewModel(x, y, side, Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_SetsEqualWidthAndHeight()
    {
        var s = Create();
        Assert.Equal(s.Width, s.Height, 10);
    }

    [Fact]
    public void Constructor_SetsCorrectSide()
    {
        var s = Create(0, 0, 80);
        Assert.Equal(80, s.Side, 10);
    }

    [Fact]
    public void Scale_WithSingleFactor_PreservesSquareShape()
    {
        var s = Create(0, 0, 100);
        s.Scale(2.0);
        Assert.Equal(s.Width, s.Height, 10);
    }

    [Fact]
    public void Move_PreservesSide()
    {
        var s = Create(0, 0, 100);
        s.Move(30, 30);
        Assert.Equal(100, s.Side, 10);
    }
}

// ─── LineViewModel ────────────────────────────────────────────────────────────

public class LineViewModelTests
{
    private static LineViewModel Create(double x1 = 0, double y1 = 0, double x2 = 100, double y2 = 0)
        => new LineViewModel(x1, y1, x2, y2, Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_SetsEndpoints()
    {
        var l = Create(10, 20, 110, 120);
        Assert.Equal(10, l.X1);
        Assert.Equal(20, l.Y1);
        Assert.Equal(110, l.X2);
        Assert.Equal(120, l.Y2);
    }

    [Fact]
    public void Constructor_SetsCorrectCenter()
    {
        var l = Create(0, 0, 100, 0);
        Assert.Equal(50, l.Center.X, 10);
        Assert.Equal(0, l.Center.Y, 10);
    }

    [Fact]
    public void Length_IsCorrect()
    {
        var l = Create(0, 0, 100, 0);
        Assert.Equal(100, l.Length, 10);
    }

    [Fact]
    public void Length_DiagonalIsCorrect()
    {
        var l = Create(0, 0, 3, 4);
        Assert.Equal(5, l.Length, 10);
    }

    [Fact]
    public void Angle_HorizontalLine_IsZero()
    {
        var l = Create(0, 0, 100, 0);
        Assert.Equal(0, l.Angle, 10);
    }

    [Fact]
    public void Move_ShiftsBothEndpoints()
    {
        var l = Create(0, 0, 100, 0);
        l.Move(10, 20);
        Assert.Equal(10, l.X1);
        Assert.Equal(20, l.Y1);
        Assert.Equal(110, l.X2);
        Assert.Equal(20, l.Y2);
    }

    [Fact]
    public void Move_PreservesLength()
    {
        var l = Create(0, 0, 100, 0);
        l.Move(50, 50);
        Assert.Equal(100, l.Length, 10);
    }

    [Fact]
    public void Scale_ChangesLength()
    {
        var l = Create(0, 0, 100, 0);
        l.Scale(2, 2);
        Assert.Equal(200, l.Length, 10);
    }

    [Fact]
    public void Rotate_360_RestoresEndpoints()
    {
        var l = Create(0, 0, 100, 0);
        var x1 = l.X1; var y1 = l.Y1;
        l.Rotate(360);
        Assert.Equal(x1, l.X1, 10);
        Assert.Equal(y1, l.Y1, 10);
    }

    [Fact]
    public void Rotate_PreservesCenter()
    {
        var l = Create(0, 0, 100, 0);
        var before = l.Center;
        l.Rotate(45);
        Assert.Equal(before.X, l.Center.X, 10);
        Assert.Equal(before.Y, l.Center.Y, 10);
    }

    [Fact]
    public void IsIn_PointOnLine_ReturnsTrue()
    {
        var l = Create(0, 0, 100, 0);
        Assert.True(l.IsIn(new Point2D(50, 0)));
    }

    [Fact]
    public void IsIn_PointFarFromLine_ReturnsFalse()
    {
        var l = Create(0, 0, 100, 0);
        Assert.False(l.IsIn(new Point2D(50, 100)));
    }

    [Fact]
    public void GetVertexPoint_Returns2Points()
        => Assert.Equal(2, Create().GetVertexPoint().Count());

    [Fact]
    public void Clone_ReturnsCopyWithSameEndpoints()
    {
        var l = Create(10, 20, 110, 80);
        var clone = (LineViewModel)l.Clone();
        Assert.Equal(l.X1, clone.X1, 10);
        Assert.Equal(l.Y1, clone.Y1, 10);
        Assert.Equal(l.X2, clone.X2, 10);
        Assert.Equal(l.Y2, clone.Y2, 10);
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var l = Create(0, 0, 100, 0);
        var clone = (LineViewModel)l.Clone();
        clone.Move(50, 0);
        Assert.Equal(0, l.X1);
    }
}

// ─── PenPointViewModel ────────────────────────────────────────────────────────

public class PenPointViewModelTests
{
    private static PenPointViewModel Create(double x = 50, double y = 50)
        => new PenPointViewModel(x, y, Color.Black, 1, Color.Transparent, 1.0);

    [Fact]
    public void Constructor_SetsCorrectXY()
    {
        var p = Create(30, 40);
        Assert.Equal(30, p.X);
        Assert.Equal(40, p.Y);
    }

    [Fact]
    public void Center_EqualsPosition()
    {
        var p = Create(30, 40);
        Assert.Equal(p.X, p.Center.X);
        Assert.Equal(p.Y, p.Center.Y);
    }

    [Fact]
    public void Move_ShiftsPosition()
    {
        var p = Create(0, 0);
        p.Move(10, 20);
        Assert.Equal(10, p.X);
        Assert.Equal(20, p.Y);
    }

    [Fact]
    public void Rotate_DoesNotChangePosition()
    {
        var p = Create(50, 50);
        p.Rotate(90);
        Assert.Equal(50, p.X);
        Assert.Equal(50, p.Y);
    }

    [Fact]
    public void Scale_DoesNotChangePosition()
    {
        var p = Create(50, 50);
        p.Scale(2, 2);
        Assert.Equal(50, p.X);
        Assert.Equal(50, p.Y);
    }

    [Fact]
    public void IsIn_NearPoint_ReturnsTrue()
    {
        var p = Create(50, 50);
        Assert.True(p.IsIn(new Point2D(50, 50)));
    }

    [Fact]
    public void IsIn_FarPoint_ReturnsFalse()
    {
        var p = Create(50, 50);
        Assert.False(p.IsIn(new Point2D(200, 200)));
    }

    [Fact]
    public void GetVertexPoint_Returns1Point()
        => Assert.Equal(1, Create().GetVertexPoint().Count());
}

// ─── RhombusViewModel (Clone = NotImplementedException) ───────────────────────

// public class RhombusViewModelTests
// {
//     private static RhombusViewModel Create(double x = 0, double y = 0, double w = 100, double h = 80)
//         => new RhombusViewModel(x, y, w, h);
//
//     [Fact]
//     public void Constructor_SetsCorrectXY()
//     {
//         var r = Create(10, 20);
//         Assert.Equal(10, r.X);
//         Assert.Equal(20, r.Y);
//     }
//
//     [Fact]
//     public void Constructor_SetsCorrectWidthHeight()
//     {
//         var r = Create(0, 0, 100, 80);
//         Assert.Equal(100, r.Width);
//         Assert.Equal(80, r.Height);
//     }
//
//     // [Fact]
//     // public void Move_ShiftsPosition()
//     // {
//     //     var r = Create(0, 0);
//     //     r.Move(15, 25);
//     //     Assert.Equal(15, r.X);
//     //     Assert.Equal(25, r.Y);
//     // }
//
//     [Fact]
//     public void Move_PreservesSize()
//     {
//         var r = Create(0, 0, 100, 80);
//         r.Move(50, 50);
//         Assert.Equal(100, r.Width, 10);
//         Assert.Equal(80, r.Height, 10);
//     }
//
//     [Fact]
//     public void Scale_ChangesSize()
//     {
//         var r = Create(0, 0, 100, 80);
//         r.Scale(2, 2);
//         Assert.Equal(200, r.Width, 10);
//         Assert.Equal(160, r.Height, 10);
//     }
//
//     [Fact]
//     public void Scale_PreservesCenter()
//     {
//         var r = Create(0, 0, 100, 80);
//         var before = r.Center;
//         r.Scale(2, 2);
//         Assert.Equal(before.X, r.Center.X, 10);
//         Assert.Equal(before.Y, r.Center.Y, 10);
//     }
//
//     [Fact]
//     public void Rotate_360_RestoresVertices()
//     {
//         var r = Create(0, 0, 100, 80);
//         var before = r.Vertices.Select(v => (v.X, v.Y)).ToList();
//         r.Rotate(360);
//         for (int i = 0; i < r.Vertices.Count; i++)
//         {
//             Assert.Equal(before[i].X, r.Vertices[i].X, 10);
//             Assert.Equal(before[i].Y, r.Vertices[i].Y, 10);
//         }
//     }
//
//     [Fact]
//     public void IsIn_Center_ReturnsTrue()
//         => Assert.True(Create().IsIn(Create().Center));
//
//     [Fact]
//     public void IsIn_Outside_ReturnsFalse()
//         => Assert.False(Create().IsIn(new Point2D(1000, 1000)));
//
//     [Fact]
//     public void Clone_ThrowsNotImplementedException()
//         => Assert.Throws<NotImplementedException>(() => Create().Clone());
// }

// ─── RightTriangleViewModel (Clone = NotImplementedException) ─────────────────

// public class RightTriangleViewModelTests
// {
//     private static RightTriangleViewModel Create(double x = 0, double y = 0, double w = 100, double h = 80)
//         => new RightTriangleViewModel(x, y, w, h);
//
//     [Fact]
//     public void Constructor_SetsCorrectXY()
//     {
//         var t = Create(5, 10);
//         Assert.Equal(5, t.X);
//         Assert.Equal(10, t.Y);
//     }
//
//     [Fact]
//     public void Constructor_SetsCorrectWidthHeight()
//     {
//         var t = Create(0, 0, 120, 90);
//         Assert.Equal(120, t.Width);
//         Assert.Equal(90, t.Height);
//     }
//
//     [Fact]
//     public void Move_ShiftsPosition()
//     {
//         var t = Create(0, 0);
//         t.Move(20, 10);
//         Assert.Equal(20, t.X);
//         Assert.Equal(10, t.Y);
//     }
//
//     [Fact]
//     public void Scale_ChangesSize()
//     {
//         var t = Create(0, 0, 100, 80);
//         t.Scale(2, 2);
//         Assert.Equal(200, t.Width, 10);
//         Assert.Equal(160, t.Height, 10);
//     }
//
//     [Fact]
//     public void Scale_PreservesCenter()
//     {
//         var t = Create(0, 0, 100, 80);
//         var before = t.Center;
//         t.Scale(2, 2);
//         Assert.Equal(before.X, t.Center.X, 10);
//         Assert.Equal(before.Y, t.Center.Y, 10);
//     }
//
//     [Fact]
//     public void Rotate_360_RestoresVertices()
//     {
//         var t = Create(0, 0, 100, 80);
//         var before = t.Vertices.Select(v => (v.X, v.Y)).ToList();
//         t.Rotate(360);
//         for (int i = 0; i < t.Vertices.Count; i++)
//         {
//             Assert.Equal(before[i].X, t.Vertices[i].X, 10);
//             Assert.Equal(before[i].Y, t.Vertices[i].Y, 10);
//         }
//     }
//
//     [Fact]
//     public void IsIn_Center_ReturnsTrue()
//         => Assert.True(Create().IsIn(Create().Center));
//
//     [Fact]
//     public void Clone_ThrowsNotImplementedException()
//         => Assert.Throws<NotImplementedException>(() => Create().Clone());
// }