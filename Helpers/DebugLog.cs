// Helpers/DebugLog.cs

using System;
using System.IO;

namespace graphic_editor.Helpers;

/// <summary>
/// Вспомогательный класс для отладочного логирования в консоль и файл.
/// </summary>
public static class DebugLog
{
	/// <summary>
    /// Путь к файлу лога в директории приложения.
    /// </summary>
    private static readonly string LogPath = Path.Combine(
        AppContext.BaseDirectory, "debug.log");

	/// <summary>
    /// Записывает сообщение в консоль и файл лога.
    /// </summary>
    /// <param name="message">Текст сообщения для логирования.</param>
    /// <remarks>
    /// Формат строки: [<time>] <message>
    /// При ошибке записи в файл ошибка выводится в консоль с префиксом [LOG ERROR].
    /// </remarks>
    public static void Write(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            Console.WriteLine(line);
            File.AppendAllText(LogPath, line + "\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOG ERROR] {ex.Message}");
        }
    }
    
	/// <summary>
    /// Возвращает полный путь к файлу лога.
    /// </summary>
    /// <returns>Абсолютный путь к файлу debug.log.</returns>
    public static string GetLogPath() => LogPath;
}