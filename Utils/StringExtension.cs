using System.Text;

namespace APS_Forma_Console.Utils;
internal static class StringExtension
{
    public static string MaskString(this string value, int charNumber = 3) =>
        value.Length <= 8 ? "********" : value[..charNumber] + new string('*', value.Length - 8) + value[^charNumber..];

    public static string ToBase64Url(this string value) => 
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    public static string ToBase64Url(this string value, bool extended = true) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
