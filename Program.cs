using Avalonia;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;

namespace graphic_editor;

/// <summary>
/// Точка входа в приложение.
/// Содержит методы инициализации и запуска Avalonia приложения.
/// </summary>
sealed class Program
{
    /// <summary>
    /// Основная точка входа в приложение.
    /// Не используйте Avalonia, сторонние API или любой код, зависящий от SynchronizationContext,
    /// до вызова AppMain: вещи ещё не инициализированы и могут сломаться.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    
    public static void Main(string[] args)
    {
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        BuildAvaloniaApp()
            .UseReactiveUI()
            .StartWithClassicDesktopLifetime(args);
        Console.ReadLine();
    }
    
    /// <summary>
    /// Конфигурация Avalonia приложения. Не удаляйте; также используется визуальным дизайнером.
    /// </summary>
    /// <returns>Построитель приложения Avalonia.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
