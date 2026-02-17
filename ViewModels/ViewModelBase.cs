using CommunityToolkit.Mvvm.ComponentModel;

namespace graphic_editor.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModel в приложении.
/// Наследует от ObservableObject из CommunityToolkit.Mvvm для реализации INotifyPropertyChanged.
/// </summary>
/// <remarks>
/// Все ViewModel должны наследоваться от этого класса для поддержки привязки данных.
/// Предоставляет механизм уведомлений об изменении свойств.
/// </remarks>
public abstract class ViewModelBase : ObservableObject
{
}
