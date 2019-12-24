using System;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCore.Web.Extension
{
    public static class PagingExtension
    {
        public static IHtmlContent Paging(this IHtmlHelper html, HttpContext context, long pages, FormMethod method = FormMethod.Get, Action<PagingOptions> options = default)
        {
            var request = context.Request;
            var option = new PagingOptions(request);
            options?.Invoke(option);
            var builder = new StringBuilder();
            var post = method == FormMethod.Post;
            var formId = "fm" + GetRandomCode(4);
            if (post)
            {
                builder.AppendFormat("<form method=\"POST\" id=\"{0}\">", formId);
                builder.AppendFormat("<script>var {0}=document.getElementById('{0}');</script>", formId);
            }

            builder.AppendFormat("<ul class=\"pagination {0} {1}\">", PositionStyles[(int)option.Position], SizeStyles[(int)option.Size]);
            var pageIndex = GetPageIndex(request);

            var prePageUrl = BuildQuery(option, request, pageIndex - 1 < 1 ? 1 : pageIndex - 1);
            builder.AppendFormat("<li class=\"page-item{2}\"><a class=\"page-link\" href=\"{0}\"{3}>{1}</a></li>", post ? $"javascript:{formId}.action='{prePageUrl}';{formId}.submit();" : prePageUrl, option.PreButtonText, pageIndex <= 1 ? " disabled" : "", pageIndex <= 1 ? " tabindex=\"-1\"" : "");

            var mid = option.PagerItems / 2 + 1;
            var start = pageIndex - mid + 1 < 1 ? 1 : pageIndex - mid + 1;
            var end = pageIndex + (option.PagerItems - (pageIndex - start + 1));
            if (end > pages)
            {
                end = pages;
                start = pages - option.PagerItems + 1;
                if (start < 1)
                {
                    start = 1;
                }
            }

            for (var page = start; page <= end; page++)
            {
                var currentPageUrl = BuildQuery(option, request, page);
                builder.AppendFormat("<li class=\"page-item{1}\"><a class=\"page-link\" href=\"{2}\">{0}</a></li>", page, pageIndex == page ? " active" : "", post ? $"javascript:{formId}.action='{currentPageUrl}';{formId}.submit();" : currentPageUrl);
            }

            var nextPageUrl = BuildQuery(option, request, pageIndex >= pages ? pages : pageIndex + 1);
            builder.AppendFormat("<li class=\"page-item{2}\"><a class=\"page-link\" href=\"{0}\"{3}>{1}</a></li>", post ? $"javascript:{formId}.action='{nextPageUrl}';{formId}.submit();" : nextPageUrl, option.NextButtonText, pageIndex >= pages ? " disabled" : "", pageIndex >= pages ? " tabindex=\"-1\"" : "");


            builder.Append("</ul>");

            if (post)
            {
                if (request.Method.Equals("post", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var (key, value) in request.Form)
                    {
                        if (key == "__RequestVerificationToken")
                            continue;
                        if (value.Count == 1)
                            builder.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, HttpUtility.HtmlEncode(value.ToString()));
                        else
                            foreach (var v in value)
                            {
                                builder.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, HttpUtility.HtmlEncode(v));
                            }
                    }
                }
                builder.AppendFormat("</form>");
            }

            return html.Raw(builder.ToString());
        }


        private static readonly string[] AllCharArray = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        private static string GetRandomCode(int length)
        {
            var builder = new StringBuilder();
            var rand = new Random(Guid.NewGuid().GetHashCode());
            for (var i = 0; i < length; i++)
            {
                builder.Append(AllCharArray[rand.Next(AllCharArray.Length - 1)]);
            }
            return builder.ToString();
        }

        private static long GetPageIndex(HttpRequest request)
        {
            return request.Query.ContainsKey("page") ? long.Parse(request.Query["page"]) : 1;
        }


        private static string BuildQuery(PagingOptions options, HttpRequest request, long page)
        {
            var query = new StringBuilder();
            query.AppendFormat("{0}?page={1}", options.Path, page);
            foreach (var (key, value) in request.Query)
            {
                if (key == "page")
                    continue;
                if (value.Count == 1)
                    query.AppendFormat("&{0}={1}", key, HttpUtility.UrlEncode(value[0]));
                else
                {
                    foreach (var v in value)
                    {
                        query.AppendFormat("&{0}={1}", key, HttpUtility.UrlEncode(v));
                    }
                }
            }

            return query.ToString();
        }

        private static readonly string[] PositionStyles = { "justify-content-start", "justify-content-center", "justify-content-end" };

        private static readonly string[] SizeStyles = { "pagination-sm", "pagination-lg", "" };
    }

    public class PagingOptions
    {
        internal PagingOptions(HttpRequest request)
        {
            Path = request.Path;
        }
        public string Path { get; set; }
        public string PreButtonText { get; set; } = "&laquo;";

        public string NextButtonText { get; set; } = "&raquo;";

        public int PagerItems { get; set; } = 11;

        public PagerPosition Position { get; set; } = PagerPosition.Right;

        public PagerSize Size { get; set; } = PagerSize.Normal;
    }

    public enum PagerPosition
    {
        Left, Center, Right
    }

    public enum PagerSize
    {
        Small,
        Large,
        Normal
    }
}
