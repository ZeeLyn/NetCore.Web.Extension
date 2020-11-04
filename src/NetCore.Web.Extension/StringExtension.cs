using System.Net;

namespace NetCore.Web.Extension
{
    public static class StringExtension
    {
        public static string UrlEncode(this string source)
        {
            return WebUtility.UrlEncode(source);
        }

        public static string UrlDecode(this string encodeText)
        {
            return WebUtility.UrlDecode(encodeText);
        }

        public static string HtmlEncode(this string source)
        {
            return WebUtility.HtmlEncode(source);
        }

        public static string HtmlDecode(this string encodeText)
        {
            return WebUtility.HtmlDecode(encodeText);
        }
    }
}
