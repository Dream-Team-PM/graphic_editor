// Helpers/AvaloniaScheduler.cs

using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

using Avalonia.Threading;

namespace graphic_editor.Helpers;

/// <summary>
/// Реализация <see cref="IScheduler"/> для выполнения задач в UI-потоке Avalonia через <see cref="Dispatcher.UIThread"/>.
/// </summary>
public class AvaloniaScheduler : IScheduler
{
    /// <summary>
    /// Статический экземпляр планировщика для повторного использования.
    /// </summary>
    public static readonly AvaloniaScheduler Instance = new();
    
    /// <inheritdoc/>
    public DateTimeOffset Now => DateTimeOffset.Now;
    
    /// <summary>
    /// Планирует выполнение действия немедленно в UI-потоке.
    /// </summary>
    /// <typeparam name="TState">Тип состояния, передаваемого в действие.</typeparam>
    /// <param name="state">Состояние для передачи в действие.</param>
    /// <param name="action">Действие для выполнения.</param>
    /// <returns>
    /// <see cref="IDisposable"/> для отмены запланированного действия.
    /// </returns>
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        var d = new SingleAssignmentDisposable();
        Dispatcher.UIThread.Post(() => { if (!d.IsDisposed) d.Disposable = action(this, state); });
        return d;
    }
    
    /// <summary>
    /// Планирует выполнение действия с задержкой в UI-потоке.
    /// </summary>
    /// <typeparam name="TState">Тип состояния, передаваемого в действие.</typeparam>
    /// <param name="state">Состояние для передачи в действие.</param>
    /// <param name="dueTime">Время задержки перед выполнением.</param>
    /// <param name="action">Действие для выполнения.</param>
    /// <returns>
    /// <see cref="IDisposable"/> для отмены запланированного действия.
    /// </returns>
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        var d = new SingleAssignmentDisposable();
        _ = System.Threading.Tasks.Task.Delay(dueTime).ContinueWith(_ => 
        {
            if (!d.IsDisposed) 
                Dispatcher.UIThread.Post(() => { if (!d.IsDisposed) d.Disposable = action(this, state); });
        });
        return d;
    }
    
    /// <summary>
    /// Планирует выполнение действия к определённому моменту времени.
    /// </summary>
    /// <typeparam name="TState">Тип состояния, передаваемого в действие.</typeparam>
    /// <param name="state">Состояние для передачи в действие.</param>
    /// <param name="dueTime">Абсолютное время выполнения.</param>
    /// <param name="action">Действие для выполнения.</param>
    /// <returns>
    /// <see cref="IDisposable"/> для отмены запланированного действия.
    /// </returns>
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) =>
        Schedule(state, dueTime - Now, action);
}