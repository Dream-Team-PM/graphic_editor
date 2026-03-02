// ViewModels/LayerViewModel.cs

using System;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace graphic_editor.ViewModels;

/// <summary>
/// Модель слоя холста, основывается на ViewModelBase.
/// </summary>
public class LayerViewModel : ViewModelBase
{
    private Guid _id;
    private string _name; /// <summary>Название слоя.</summary>
    private bool _isVisible = true; /// <summary>Флаг видимости слоя.</summary>
    private bool _isLocked; /// <summary>Флаг для проверки, заблокирован слой или нет.</summary>
    private ObservableCollection<FigureViewModel> _figures; /// <summary>Коллекция фигур, размещённых на одном холсте.</summary>

    public LayerViewModel() : this("Слой 1") { } /// <summary>Конструктор LayerViewModel.</summary>

 	/// <summary>Конструктор LayerViewModel по названию.</summary>
    public LayerViewModel(string name)
    {
        _id = Guid.NewGuid();
        _name = name;
        _figures = new ObservableCollection<FigureViewModel>();
    }
    
	/// <summary>Публичное свойство - имя слоя.</summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

	/// <summary>Публичное свойство - видимый слой или нет.</summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

	/// <summary>Публичное свойство - заблокирован слой или нет.</summary>
    public bool IsLocked
    {
        get => _isLocked;
        set => this.RaiseAndSetIfChanged(ref _isLocked, value);
    }

	/// <summary>Публичная коллекция фигур на одном слое.</summary>
    public ObservableCollection<FigureViewModel> Figures => _figures;
    
    public Guid Id => _id;

	/// <summary>Публичное сввойство подсчёта числа фигур на слое.</summary>
    public int FigureCount => _figures.Count;

	/// <summary>Публичная функция добавления фигуры на слой.</summary>
    public void AddFigure(FigureViewModel figure)
    {
        _figures.Add(figure);
        this.RaisePropertyChanged(nameof(FigureCount));
    }

	/// <summary>Публичная функция удаления фигуры со слоя.</summary>
    public void RemoveFigure(FigureViewModel figure)
    {
        _figures.Remove(figure);
        this.RaisePropertyChanged(nameof(FigureCount));
    }
}