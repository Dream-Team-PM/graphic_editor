using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using graphic_editor.ViewModels;

namespace graphic_editor;

/// <summary>
/// Локатор представлений (View Locator) для паттерна MVVM.
/// Преобразует ViewModel в соответствующий View на основе соглашения об именовании.
/// </summary>
/// <remarks>
/// По умолчанию ищет View с тем же именем, что и ViewModel, но с заменой "ViewModel" на "View".
/// Например: MainWindowViewModel → MainWindowView
/// </remarks>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Создает Control (View) на основе переданного ViewModel.
    /// </summary>
    /// <param name="param">ViewModel, для которого нужно создать View.</param>
    /// <returns>Созданный Control или TextBlock с сообщением об ошибке.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Проверяет, может ли данный DataTemplate обработать переданные данные.
    /// </summary>
    /// <param name="data">Данные для проверки (обычно ViewModel).</param>
    /// <returns>true, если данные являются производными от ViewModelBase; иначе false.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
