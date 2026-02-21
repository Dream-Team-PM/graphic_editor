// Helpers/AvaloniaScheduler.cs

using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

using Avalonia.Threading;

namespace graphic_editor.Helpers;

public class AvaloniaScheduler : IScheduler
{
    public static readonly AvaloniaScheduler Instance = new();
    
    public DateTimeOffset Now => DateTimeOffset.Now;
    
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        var d = new SingleAssignmentDisposable();
        Dispatcher.UIThread.Post(() => { if (!d.IsDisposed) d.Disposable = action(this, state); });
        return d;
    }
    
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
    
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) =>
        Schedule(state, dueTime - Now, action);
}