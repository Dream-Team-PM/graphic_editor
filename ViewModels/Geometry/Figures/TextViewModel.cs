using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Media;
using ReactiveUI;

using graphic_editor.ViewModels;
using graphic_editor.Models;

namespace graphic_editor.Geometry;

/// <summary>
/// ViewModel для текстовой фигуры на холсте.
/// Поддерживает редактирование содержимого, шрифта, выравнивания и трансформации.
/// </summary>
public class TextViewModel : FigureViewModel
{
    private string _text = "Текст";
    private string _fontFamily = "Segoe UI";
    private double _fontSize = 24;
    private FontWeight _fontWeight = FontWeight.Regular;
    private Avalonia.Media.FontStyle _fontStyle = FontStyle.Normal;
    private TextAlignment _textAlignment = TextAlignment.Left;

    /// <summary>
    /// Инициализирует текстовую фигуру с заданными параметрами.
    /// </summary>
    public TextViewModel(
        double x, double y,
        string text,
        double fontSize = 24,
        string fontFamily = "Segoe UI",
        System.Drawing.Color lineColor = default,
        System.Drawing.Color fillColor = default,
        double opacity = 1.0)
        : base()
    {
        _text = text;
        _fontSize = fontSize;
        _fontFamily = fontFamily;
        LineColor = lineColor == default ? System.Drawing.Color.Black : lineColor;
        FillColor = fillColor == default ? System.Drawing.Color.Black : fillColor;
        Opacity = opacity;

        UpdateVertices(x, y);
        Name = "Текст";
    }

    /// <summary>
    /// Содержимое текста.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            this.RaiseAndSetIfChanged(ref _text, value);
            NotifyTextChanged();
        }
    }

    /// <summary>
    /// Название шрифта.
    /// </summary>
    public string FontFamily
    {
        get => _fontFamily;
        set
        {
            this.RaiseAndSetIfChanged(ref _fontFamily, value);
            UpdateBoundingBoxFromText();
            this.RaisePropertyChanged(nameof(Center));
        }
    }

    /// <summary>
    /// Размер шрифта в пикселях.
    /// </summary>
    public double FontSize
    {
        get => _fontSize;
        set
        {
            this.RaiseAndSetIfChanged(ref _fontSize, Math.Max(1, value)); 
			UpdateBoundingBoxFromText();
            this.RaisePropertyChanged(nameof(Center));
        }
    }

    /// <summary>
    /// Насыщенность шрифта.
    /// </summary>
    public FontWeight FontWeight
    {
        get => _fontWeight;
        set => this.RaiseAndSetIfChanged(ref _fontWeight, value);
    }

    /// <summary>
    /// Начертание шрифта.
    /// </summary>
    public Avalonia.Media.FontStyle FontStyle
    {
        get => _fontStyle;
        set => this.RaiseAndSetIfChanged(ref _fontStyle, value);
    }

    /// <summary>
    /// Выравнивание текста.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => _textAlignment;
        set => this.RaiseAndSetIfChanged(ref _textAlignment, value);
    }

    /// <summary>
    /// Центральная точка текста.
    /// </summary>
    public override Point2D Center
    {
        get
        {
            var bbox = GetBoundingBox();
            return new Point2D(
                (bbox.MinX + bbox.MaxX) / 2,
                (bbox.MinY + bbox.MaxY) / 2
            );
        }
    }

    /// <summary>
    /// Возвращает вершины ограничивающего прямоугольника.
    /// </summary>
    public override IEnumerable<Point2D> GetVertexPoint() => Vertices.Select(v => v.ToPoint());

    /// <summary>
    /// Перемещает текст.
    /// </summary>
    public override void Move(double dx, double dy)
    {
        foreach (var vertex in Vertices)
        {
            vertex.X += dx;
            vertex.Y += dy;
        }
        NotifyPropertyChanged();
		this.RaisePropertyChanged(nameof(Center));
    }

	public override void Reflection(Point2D a, Point2D b)
	{
    	base.Reflection(a, b);
    	NotifyPropertyChanged();
	}

    /// <summary>
    /// Поворачивает текст.
    /// </summary>
    public override void Rotate(double angle)
	{
    	_rotation += angle;
    	this.RaisePropertyChanged(nameof(Rotation));
    
    	// Поворачиваем вершины ограничивающего прямоугольника (для hit-testing и bounding box)
    	var center = Center;
    	foreach (var vertex in Vertices)
    	{
        	var rotated = vertex.ToPoint().Rotate(center, angle);
        	vertex.X = rotated.X;
        	vertex.Y = rotated.Y;
    	}
    	NotifyPropertyChanged();
	}

    /// <summary>
    /// Масштабирует текст.
    /// </summary>
    public override void Scale(double sx, double sy)
    {
        var center = Center;
        foreach (var vertex in Vertices)
        {
            var scaled = vertex.ToPoint().Scale(center, sx, sy);
            vertex.X = scaled.X;
            vertex.Y = scaled.Y;
        }
        _fontSize = Math.Max(1, _fontSize * Math.Sqrt(sx * sy));
        this.RaisePropertyChanged(nameof(FontSize));
        UpdateBoundingBoxFromText();
        NotifyPropertyChanged();
    }

    /// <summary>
    /// Проверяет попадание точки.
    /// </summary>
    public override bool IsIn(Point2D point, double eps = 0.001)
    {
        var bbox = GetBoundingBox();
        return point.X >= bbox.MinX - eps && point.X <= bbox.MaxX + eps &&
               point.Y >= bbox.MinY - eps && point.Y <= bbox.MaxY + eps;
    }

    /// <summary>
    /// Клонирование.
    /// </summary>
    public override FigureViewModel Clone()
    {
        var clone = new TextViewModel(
            Vertices[0].X, Vertices[0].Y,
            _text, _fontSize, _fontFamily,
            LineColor, FillColor, Opacity)
        {
            FontWeight = _fontWeight,
            FontStyle = _fontStyle,
            TextAlignment = _textAlignment,
            Rotation = _rotation
        };
        for (int i = 1; i < Vertices.Count && i < clone.Vertices.Count; i++)
        {
            clone.Vertices[i].X = Vertices[i].X;
            clone.Vertices[i].Y = Vertices[i].Y;
        }
		clone.NotifyPropertyChanged();
        return clone;
    }

    /// <summary>
    /// Обновляет вершины.
    /// </summary>
    private void UpdateVertices(double x, double y)
    {
        var width = Math.Max(10, _text.Length * _fontSize * 0.6);
        var height = _fontSize * 1.2;

        while (Vertices.Count < 4)
            Vertices.Add(new PointViewModel());
        Vertices[0].X = x; Vertices[0].Y = y;
        Vertices[1].X = x + width; Vertices[1].Y = y;
        Vertices[2].X = x + width; Vertices[2].Y = y + height;
        Vertices[3].X = x; Vertices[3].Y = y + height;
    }

    /// <summary>
    /// Пересчитывает bounding box.
    /// </summary>
    public void UpdateBoundingBoxFromText()
    {
        if (Vertices.Count < 4) return;
        
        var x = Vertices[0].X;
        var y = Vertices[0].Y;
        var width = Math.Max(10, _text.Length * _fontSize * 0.6);
        var height = _fontSize * 1.2;

        Vertices[0].X = x; Vertices[0].Y = y;
        Vertices[1].X = x + width; Vertices[1].Y = y;
        Vertices[2].X = x + width; Vertices[2].Y = y + height;
        Vertices[3].X = x; Vertices[3].Y = y + height;
    }

    /// <summary>
    /// Возвращает FormattedText для Avalonia.
    /// </summary>
    public Avalonia.Media.FormattedText GetFormattedText()
    {
        var typeface = new Typeface(
            new Avalonia.Media.FontFamily(_fontFamily),
            _fontStyle,
            _fontWeight
        );

        return new Avalonia.Media.FormattedText(
            _text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            _fontSize,
            new SolidColorBrush(ToAvaloniaColor(FillColor))
        )
        {
            TextAlignment = _textAlignment
        };
    }

    /// <summary>
    /// Конвертация цвета.
    /// </summary>
    private static Avalonia.Media.Color ToAvaloniaColor(System.Drawing.Color c) => 
        Avalonia.Media.Color.FromArgb(c.A, c.R, c.G, c.B);

	public void NotifyTextChanged()
	{
    	this.RaisePropertyChanged(nameof(Text));
    	UpdateBoundingBoxFromText();
    	this.RaisePropertyChanged(nameof(Center));
    	this.RaisePropertyChanged(nameof(Vertices));
	}
}