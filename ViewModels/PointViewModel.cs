// ViewModels/PointViewModel.cs
using graphic_editor.Models;
namespace graphic_editor.ViewModels;

public class PointViewModel : ViewModelBase
{
    private double _x;
    private double _y;

    public PointViewModel() : this(0, 0) { }

    public PointViewModel(double x, double y)
    {
        _x = x;
        _y = y;
    }

    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    public Point_1 ToPoint() => new Point_1(X, Y);

    public static PointViewModel FromPoint(Point_1 point) => new PointViewModel(point.X, point.Y);

    public override string ToString() => $"({X:F1}, {Y:F1})";
}