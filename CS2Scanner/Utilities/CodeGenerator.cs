using System;
using System.Text;

namespace CS2Scanner.Utilities;

internal static class CodeGenerator
{
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly Random Random = new();

    public static string Generate(int length)
    {
        var sb = new StringBuilder(length);
        lock (Random)
        {
            for (var i = 0; i < length; i++)
            {
                var index = Random.Next(AllowedChars.Length);
                sb.Append(AllowedChars[index]);
            }
        }

        return sb.ToString();
    }
}
