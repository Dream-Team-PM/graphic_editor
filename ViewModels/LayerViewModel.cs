using System;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace graphic_editor.ViewModels;

/// <summary>
/// ViewModel для представления слоя канваса: имя, видимость, блокировка, коллекция фигур.
/// Реализует реактивные свойства для привязки к UI через ReactiveUI.
/// </summary>
public class LayerViewModel : ViewModelBase
{
    private Guid _id; 
    /// <summary>Уникальный идентификатор слоя.</summary>
    
    private string _name; 
    /// <summary>Отображаемое имя слоя в интерфейсе.</summary>
    
    private bool _isVisible = true; 
    /// <summary>Флаг видимости слоя: если false, фигуры слоя не отрисовываются.</summary>
    
    private bool _isLocked; 
    /// <summary>Флаг блокировки слоя: если true, фигуры слоя нельзя редактировать.</summary>
    
    private ObservableCollection<FigureViewModel> _figures; 
    /// <summary>Коллекция фигур, принадлежащих данному слою.</summary>

    /// <summary>
    /// Конструктор по умолчанию: создаёт слой с именем "Слой 1".
    /// </summary>
    public LayerViewModel() : this("Слой 1") { }

    /// <summary>
    /// Конструктор с заданным именем слоя.
    /// </summary>
    /// <param name="name">Отображаемое имя нового слоя.</param>
    public LayerViewModel(string name)
    {
        _id = Guid.NewGuid();
        _name = name;
        _figures = new ObservableCollection<FigureViewModel>();
    }

	public LayerViewModel(Guid id, string name)
    {
        _id = id;
        _name = name;
        _figures = new ObservableCollection<FigureViewModel>();
    }
    
    /// <summary>
    /// Имя слоя для отображения в UI.
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Флаг видимости слоя: управляет отображением фигур слоя на канвасе.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    /// <summary>
    /// Флаг блокировки слоя: запрещает редактирование фигур при значении true.
    /// </summary>
    public bool IsLocked
    {
        get => _isLocked;
        set => this.RaiseAndSetIfChanged(ref _isLocked, value);
    }

    /// <summary>
    /// Коллекция фигур слоя, доступная для привязки в UI.
    /// </summary>
    public ObservableCollection<FigureViewModel> Figures => _figures;
    
    /// <summary>
    /// Уникальный идентификатор слоя (только для чтения).
    /// </summary>
    public Guid Id => _id;

    /// <summary>
    /// Количество фигур на слое (вычисляемое свойство).
    /// </summary>
    public int FigureCount => _figures.Count;

    /// <summary>
    /// Добавляет фигуру на слой и уведомляет об изменении количества фигур.
    /// </summary>
    /// <param name="figure">Экземпляр FigureViewModel для добавления.</param>
    public void AddFigure(FigureViewModel figure)
    {
        _figures.Add(figure);
        this.RaisePropertyChanged(nameof(FigureCount));
    }

    /// <summary>
    /// Удаляет фигуру со слоя и уведомляет об изменении количества фигур.
    /// </summary>
    /// <param name="figure">Экземпляр FigureViewModel для удаления.</param>
    public void RemoveFigure(FigureViewModel figure)
    {
        _figures.Remove(figure);
        this.RaisePropertyChanged(nameof(FigureCount));
    }
}