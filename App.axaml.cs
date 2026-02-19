using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using graphic_editor.ViewModels;

namespace graphic_editor;

/// <summary>
/// Основной класс приложения Avalonia.
/// Отвечает за инициализацию приложения, создание главного окна и управление жизненным циклом.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Инициализирует приложение, загружая XAML-ресурсы.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Вызывается после завершения инициализации фреймворка Avalonia.
    /// Создает и запускает игровой сервер, настраивает главное окно.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Отключает валидацию DataAnnotations в Avalonia для совместимости.
    /// </summary>
    /// <remarks>
    /// Это необходимо для предотвращения конфликтов с CommunityToolkit.Mvvm.
    /// </remarks>
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}