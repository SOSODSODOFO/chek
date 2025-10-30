using System.Security.Cryptography;
using System.Text;

namespace CS2Scanner;

internal static class CodeGenerator
{
    private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length)
    {
        var sb = new StringBuilder(length);
        var data = RandomNumberGenerator.GetBytes(length);

        for (var i = 0; i < length; i++)
        {
            var index = data[i] % Characters.Length;
            sb.Append(Characters[index]);
        }

        return sb.ToString();
    }
}
