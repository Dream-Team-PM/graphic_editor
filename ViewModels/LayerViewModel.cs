// ViewModels/LayerViewModel.cs
using System.Collections.ObjectModel;

namespace graphic_editor.ViewModels;

public class LayerViewModel : ViewModelBase
{
    private string _name;
    private bool _isVisible = true;
    private bool _isLocked;
    private ObservableCollection<FigureViewModel> _figures;

    public LayerViewModel() : this("Слой 1") { }

    public LayerViewModel(string name)
    {
        _name = name;
        _figures = new ObservableCollection<FigureViewModel>();
    }
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => SetProperty(ref _isLocked, value);
    }

    public ObservableCollection<FigureViewModel> Figures => _figures;

    public int FigureCount => _figures.Count;

    public void AddFigure(FigureViewModel figure)
    {
        _figures.Add(figure);
        OnPropertyChanged(nameof(FigureCount));
    }

    public void RemoveFigure(FigureViewModel figure)
    {
        _figures.Remove(figure);
        OnPropertyChanged(nameof(FigureCount));
    }
}