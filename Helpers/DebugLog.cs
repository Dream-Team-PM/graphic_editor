// Helpers/DebugLog.cs

using System;
using System.IO;

namespace graphic_editor.Helpers;

public static class DebugLog
{
    private static readonly string LogPath = Path.Combine(
        AppContext.BaseDirectory, "debug.log");

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
    
    public static string GetLogPath() => LogPath;
}