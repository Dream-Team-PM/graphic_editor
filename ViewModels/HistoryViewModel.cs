// ViewModels/HistoryViewModel.cs

using System;
using System.Collections.ObjectModel;
using graphic_editor.Interfaces;
using graphic_editor.Commands;

using ReactiveUI;

namespace graphic_editor.ViewModels;

/// <summary>
/// Класс истории, основывается на ViewModelBase (Находится в разработке).
/// </summary>
public class HistoryViewModel : ViewModelBase
{
    private int _currentIndex = -1; /// <summary>Приватное свойство для индекса действия.</summary>
    private readonly ObservableCollection<IHistoryAction> _actions = new(); /// <summary>Инимциализация коллекции действий.</summary>
	private CanvasViewModel? _canvas;

    public ObservableCollection<IHistoryAction> Actions => _actions; /// <summary>Публичная коллекция действий.</summary>
    public bool CanUndo => _currentIndex >= 0; /// <summary>Флаг проверки возможности отмены.</summary>
    public bool CanRedo => _currentIndex < _actions.Count - 1; /// <summary>Флаг проверки возможности повторения.</summary>
	public void SetCanvas(CanvasViewModel canvas) => _canvas = canvas;
	/// <summary>Публичная функция добавления действия.</summary>
    public void AddAction(IHistoryAction action)
    {
        // Удаляем все действия после текущего
        while (_actions.Count > _currentIndex + 1)
            _actions.RemoveAt(_actions.Count - 1);
		if (action is ZoomCommand zoomCmd && _canvas != null)
        {
            zoomCmd.SetCanvas(_canvas);
        }
		if (action is IHistoryActionWithCanvas actionWithCanvas && _canvas != null)
        {
            actionWithCanvas.SetCanvas(_canvas);
        }
        _actions.Add(action);
        _currentIndex++;
        this.RaisePropertyChanged(nameof(CanUndo));
        this.RaisePropertyChanged(nameof(CanRedo));
        this.RaisePropertyChanged(nameof(Actions));
    }

	/// <summary>Публичная функция отмены.</summary>
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

	/// <summary>Публичная функция повторения.</summary>
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

	/// <summary>Публичная функция очистки слоя.</summary>
    public void Clear()
    {
        _actions.Clear();
        _currentIndex = -1;
        this.RaisePropertyChanged(nameof(CanUndo));
        this.RaisePropertyChanged(nameof(CanRedo));
        this.RaisePropertyChanged(nameof(Actions));
    }
}