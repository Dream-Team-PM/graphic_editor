// Models/Project.cs
using System.Collections.ObjectModel;
using graphic_editor.ViewModels;

namespace graphic_editor.Models;

/// <summary>Простая модель проекта для сохранения/загрузки.</summary>
public class Project
{
    public string Name { get; set; } = "Безымянный";
    public ObservableCollection<LayerViewModel> Layers { get; set; } = new();
    public double CanvasZoom { get; set; } = 1.0;
    public double CanvasOffsetX { get; set; }
    public double CanvasOffsetY { get; set; }
    
    // Временные поля для сериализации (позже заменим на proper DTO)
    public string Version { get; set; } = "1.0";
}