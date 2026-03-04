using graphic_editor.ViewModels;
using graphic_editor.Models;

namespace graphic_editor.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с файлами проекта.
/// Предоставляет методы для сохранения, загрузки и экспорта данных редактора.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Асинхронно сохраняет проект в указанный файл.
    /// </summary>
    /// <param name="project">Объект проекта, содержащий слои и настройки.</param>
    /// <param name="path">Полный путь к файлу для сохранения.</param>
    /// <returns>Задача, возвращающая true при успешном сохранении; иначе false.</returns>
    Task<bool> SaveProjectAsync(Project project, string path);
    
    /// <summary>
    /// Асинхронно загружает проект из указанного файла.
    /// </summary>
    /// <param name="path">Полный путь к файлу проекта.</param>
    /// <returns>Задача, возвращающая объект Project или null при ошибке загрузки.</returns>
    Task<Project?> LoadProjectAsync(string path);
    
    /// <summary>
    /// Асинхронно экспортирует содержимое канваса в растровое изображение PNG.
    /// </summary>
    /// <param name="canvas">CanvasViewModel с данными для экспорта.</param>
    /// <param name="path">Полный путь к выходному файлу PNG.</param>
    /// <param name="settings">Настройки экспорта (разрешение, фон, масштаб).</param>
    /// <returns>Задача, возвращающая true при успешном экспорте; иначе false.</returns>
    Task<bool> ExportAsPngAsync(CanvasViewModel canvas, string path, ExportSettings settings);
}