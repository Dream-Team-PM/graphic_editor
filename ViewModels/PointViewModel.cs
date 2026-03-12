// ViewModels/PointViewModel.cs

using ReactiveUI;

using graphic_editor.Models;
namespace graphic_editor.ViewModels;
using graphic_editor.Geometry;

/// <summary>
/// Модель точки, основывается на ViewModelBase.
/// </summary>
public class PointViewModel : ViewModelBase
{
    private double _x; /// <summary>Координата точки по X.</summary>
    private double _y; /// <summary>Координата точки по Y.</summary>

    public PointViewModel() : this(0, 0) { } /// <summary>Конструктор PointViewModel.</summary>

    /// <summary>Конструктор PointViewModel по координатам.</summary>
    public PointViewModel(double x, double y)
    {
        _x = x;
        _y = y;
    }

    /// <summary>Конструктор PointViewModel для координаты X.</summary>
    public double X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    /// <summary>Конструктор PointViewModel для координаты Y.</summary>
    public double Y
    {
        get => _y;
        set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    /// <summary>Приведение к типу точки.</summary>
    public Point2D ToPoint() => new Point2D(X, Y);

    /// <summary>
    /// Приведение из точки Point_1 в PointViewModelPointViewModel.
    /// </summary>
    /// <param name="point">Точка, необходимая для приведения к типу PointViewModel.</param>
    public static PointViewModel FromPoint(Point2D point) => new PointViewModel(point.X, point.Y);

    /// <summary>
    /// Приведение точки в строчный вид.
    /// </summary>
    public override string ToString() => $"({X:F15}, {Y:F15})";
    
    /// <summary>
    /// Уведомление о изменениях параметра класса.
    /// </summary>
    public void NotifyPropertyChanged()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
    }
}