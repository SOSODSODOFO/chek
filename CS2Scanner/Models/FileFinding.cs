using System;

namespace CS2Scanner.Models;

internal sealed class FileFinding
{
    public string Path { get; init; } = string.Empty;
    public long SizeBytes { get; init; }

    public string GetReadableSize()
    {
        double size = SizeBytes;
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        var index = 0;
        while (size >= 1024 && index < units.Length - 1)
        {
            size /= 1024;
            index++;
        }
        return $"{size:F1} {units[index]}";
    }
}
