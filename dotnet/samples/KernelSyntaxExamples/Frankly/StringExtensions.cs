using System;

namespace Frankly
{

    /// <summary>
    /// Some extensions we like
    /// </summary>
    public static partial class StringExtensions
    {
        public static string? TruncateTo(this string text, int n, bool addEllipsis = false)
        {
            var ellipsis = (addEllipsis && text?.Length > n) ? "..." : "";
            try
            {

                if (string.IsNullOrEmpty(text))
                    return text;
                else
                    return (text.Length > n) ? text.Substring(0, n) : text;
            }
            catch (Exception e)
            {
                Console.Write("TruncateTo Exception : " + e.Message);
            }
            return text;
        }
    }
}
