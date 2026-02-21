// ViewModels/HistoryViewModel.cs

using System;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace graphic_editor.ViewModels;

public class HistoryViewModel : ViewModelBase
{
    private int _currentIndex = -1;
    private readonly ObservableCollection<IHistoryAction> _actions = new();

    public ObservableCollection<IHistoryAction> Actions => _actions;
    public bool CanUndo => _currentIndex >= 0;
    public bool CanRedo => _currentIndex < _actions.Count - 1;

    public void AddAction(IHistoryAction action)
    {
        // Удаляем все действия после текущего
        while (_actions.Count > _currentIndex + 1)
            _actions.RemoveAt(_actions.Count - 1);

        _actions.Add(action);
        _currentIndex++;
        this.RaisePropertyChanged(nameof(CanUndo));
        this.RaisePropertyChanged(nameof(CanRedo));
    }

    public void Undo()
    {
        if (CanUndo)
        {
            _actions[_currentIndex].Undo();
            _currentIndex--;
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }
    }

    public void Redo()
    {
        if (CanRedo)
        {
            _currentIndex++;
            _actions[_currentIndex].Redo();
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }
    }

    public void Clear()
    {
        _actions.Clear();
        _currentIndex = -1;
        this.RaisePropertyChanged(nameof(CanUndo));
        this.RaisePropertyChanged(nameof(CanRedo));
    }
}

public interface IHistoryAction
{
    string Description { get; }
    void Undo();
    void Redo();
}