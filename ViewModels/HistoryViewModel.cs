using System;
using System.Collections.ObjectModel;
using graphic_editor.Interfaces;
using graphic_editor.Commands;
using graphic_editor.Helpers;

using ReactiveUI;

namespace graphic_editor.ViewModels;

/// <summary>
/// ViewModel для управления историей действий (Undo/Redo).
/// Хранит стек команд и обеспечивает навигацию по истории изменений.
/// </summary>
public class HistoryViewModel : ViewModelBase
{
    private int _currentIndex = -1; 
    /// <summary>Индекс текущего действия в истории для навигации Undo/Redo.</summary>
    
    private readonly ObservableCollection<IHistoryAction> _actions = new(); 
    /// <summary>Коллекция всех выполненных действий, поддерживающих отмену.</summary>
    
    private CanvasViewModel? _canvas; 
    /// <summary>Ссылка на CanvasViewModel для инъекции в команды, требующие контекст канваса.</summary>

    /// <summary>
    /// Публичная коллекция действий для привязки в UI (список истории).
    /// </summary>
    public ObservableCollection<IHistoryAction> Actions => _actions;
    
    /// <summary>
    /// Проверяет, доступна ли операция отмены (есть действия для Undo).
    /// </summary>
    public bool CanUndo => _currentIndex >= 0;
    
    /// <summary>
    /// Проверяет, доступна ли операция повтора (есть отменённые действия для Redo).
    /// </summary>
    public bool CanRedo => _currentIndex < _actions.Count - 1;
    
    /// <summary>
    /// Устанавливает ссылку на CanvasViewModel для команд, требующих контекст канваса.
    /// </summary>
    /// <param name="canvas">Экземпляр CanvasViewModel для инъекции.</param>
    public void SetCanvas(CanvasViewModel canvas) => _canvas = canvas;

    private string _currentActionDescription = "";
    
    /// <summary>
    /// Описание последнего выполненного действия для отображения в UI.
    /// </summary>
    public string CurrentActionDescription
    {
        get => _currentActionDescription;
        private set => this.RaiseAndSetIfChanged(ref _currentActionDescription, value);
    }

    /// <summary>
    /// Добавляет новое действие в историю, обрезая ветку Redo при необходимости.
    /// </summary>
    /// <param name="action">Экземпляр IHistoryAction для добавления.</param>
    public void AddAction(IHistoryAction action)
    {
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
        _currentActionDescription = action.Description;
        this.RaisePropertyChanged(nameof(CurrentActionDescription));
        DebugLog.Write($"[DEBUG] History: Added '{action.Description}', CanUndo={CanUndo}, CanRedo={CanRedo}");
    }

    /// <summary>
    /// Выполняет отмену последнего действия, перемещая указатель истории назад.
    /// </summary>
    public void Undo()
    {
        if (CanUndo)
        {
            var action = _actions[_currentIndex];
            DebugLog.Write($"[DEBUG] History: Undo '{action.Description}'");
            action.Undo();
            _currentIndex--;
            _currentActionDescription = CanUndo ? _actions[_currentIndex].Description : "";
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
            this.RaisePropertyChanged(nameof(CurrentActionDescription));
        }
    }

    /// <summary>
    /// Выполняет повтор ранее отменённого действия, перемещая указатель истории вперёд.
    /// </summary>
    public void Redo()
    {
        if (CanRedo)
        {
            _currentIndex++;
            var action = _actions[_currentIndex];
            DebugLog.Write($"[DEBUG] History: Redo '{action.Description}'");
            action.Redo();
            _currentActionDescription = action.Description;
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
            this.RaisePropertyChanged(nameof(CurrentActionDescription));
        }
    }

    /// <summary>
    /// Очищает всю историю действий, сбрасывая указатель в начальное состояние.
    /// </summary>
    public void Clear()
    {
        _actions.Clear();
        _currentIndex = -1;
        this.RaisePropertyChanged(nameof(CanUndo));
        this.RaisePropertyChanged(nameof(CanRedo));
        this.RaisePropertyChanged(nameof(Actions));
    }
}