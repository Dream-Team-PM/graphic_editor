using System.Drawing;
using graphic_editor.Geometry;
using graphic_editor.IO.Dto;
using graphic_editor.ViewModels;

namespace graphic_editor.IO.Mappers;

public static class FigureDtoMapper
{
    public static FigureDto ToDto(FigureViewModel vm)
    {
        return vm switch
        {
            RectangleViewModel r => new RectangleDto
            {
                Id = r.Id,
                Name = r.Name,
                Opacity = r.Opacity,
                Rotation = r.Rotation,
                LineColorArgb = r.LineColor.ToArgb(),
                FillColorArgb = r.FillColor.ToArgb(),
                Thickness = r.Thickness,
                IsSelected = r.IsSelected,
                X = r.X,
                Y = r.Y,
                Width = r.Width,
                Height = r.Height
            },
            EllipseViewModel e => new EllipseDto
            {
                Id = e.Id,
                Name = e.Name,
                Opacity = e.Opacity,
                Rotation = e.Rotation,
                LineColorArgb = e.LineColor.ToArgb(),
                FillColorArgb = e.FillColor.ToArgb(),
                Thickness = e.Thickness,
                IsSelected = e.IsSelected,
                X = e.X,
                Y = e.Y,
                Width = e.Width,
                Height = e.Height
            },
            CircleViewModel c => new CircleDto
            {
                Id = c.Id,
                Name = c.Name,
                Opacity = c.Opacity,
                Rotation = c.Rotation,
                LineColorArgb = c.LineColor.ToArgb(),
                FillColorArgb = c.FillColor.ToArgb(),
                Thickness = c.Thickness,
                IsSelected = c.IsSelected,
                CenterX = c.Center.X,
                CenterY = c.Center.Y,
                Radius = c.Radius
            },
            LineViewModel l => new LineDto
            {
                Id = l.Id,
                Name = l.Name,
                Opacity = l.Opacity,
                Rotation = l.Rotation,
                LineColorArgb = l.LineColor.ToArgb(),
                FillColorArgb = l.FillColor.ToArgb(),
                Thickness = l.Thickness,
                IsSelected = l.IsSelected,
                X1 = l.X1,
                Y1 = l.Y1,
                X2 = l.X2,
                Y2 = l.Y2
            },
            PenPointViewModel p => new PenPointDto
            {
                Id = p.Id,
                Name = p.Name,
                Opacity = p.Opacity,
                Rotation = p.Rotation,
                LineColorArgb = p.LineColor.ToArgb(),
                FillColorArgb = p.FillColor.ToArgb(),
                Thickness = p.Thickness,
                IsSelected = p.IsSelected,
                X = p.X,
                Y = p.Y
            },
            GroupViewModel g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Opacity = g.Opacity,
                Rotation = g.Rotation,
                LineColorArgb = g.LineColor.ToArgb(),
                FillColorArgb = g.FillColor.ToArgb(),
                Thickness = g.Thickness,
                IsSelected = g.IsSelected,
                Children = g.Children.Select(ToDto).ToList()
            },
            _ => throw new NotSupportedException($"Неизвестный тип фигуры: {vm.GetType().Name}. Добавь case в FigureDtoMapper!")
        };
    }

    public static FigureViewModel ToViewModel(FigureDto dto)
    {
        return dto switch
        {
            RectangleDto r => new RectangleViewModel(r.X, r.Y, r.Width, r.Height,
                Color.FromArgb(r.LineColorArgb), r.Thickness, Color.FromArgb(r.FillColorArgb), r.Opacity),
            EllipseDto e => new EllipseViewModel(e.X, e.Y, e.Width, e.Height,
                Color.FromArgb(e.LineColorArgb), e.Thickness, Color.FromArgb(e.FillColorArgb), e.Opacity),
            CircleDto c => new CircleViewModel(c.CenterX, c.CenterY, c.Radius,
                Color.FromArgb(c.LineColorArgb), c.Thickness, Color.FromArgb(c.FillColorArgb), c.Opacity),
            LineDto l => new LineViewModel(l.X1, l.Y1, l.X2, l.Y2,
                Color.FromArgb(l.LineColorArgb), l.Thickness, Color.FromArgb(l.FillColorArgb), l.Opacity),
            PenPointDto p => new PenPointViewModel(p.X, p.Y,
                Color.FromArgb(p.LineColorArgb), p.Thickness, Color.FromArgb(p.FillColorArgb), p.Opacity),
            GroupDto g => new GroupViewModel(g.Children.Select(ToViewModel)),
            _ => throw new NotSupportedException($"Неизвестный DTO: {dto.GetType().Name}")
        };
    }
}
