using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class HtmlControlsExtension
    {
        public static IHtmlContent ExLabel(this IHtmlHelper html, object text, string format,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("label");
            builder.InnerHtml.AppendHtml(html.FormatValue(text, format));
            builder.MergeAttributes(htmlAttributes);
            return builder;
        }
        public static IHtmlContent Button(this IHtmlHelper html, object value,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "button");
            builder.MergeAttribute("value", html.FormatValue(value, ""));
            builder.MergeAttributes(htmlAttributes);
            return builder;
        }

        public static IHtmlContent ExHidden(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes);
            return builder;
        }

        public static IHtmlContent ExTextBox(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "text");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes);
            return builder;
        }


        public static IHtmlContent ExPassword(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "password");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes);
            return builder;
        }

        public static IHtmlContent ExTextArea(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("textarea");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes);
            return builder;
        }

        public static IHtmlContent ExDropDownList(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("select");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttributes(htmlAttributes);
            if (!string.IsNullOrWhiteSpace(optionLabel))
            {
                var option = new TagBuilder("option");
                option.MergeAttribute("value", "");
                option.InnerHtml.AppendHtml(optionLabel);
                builder.InnerHtml.AppendHtml(option);
            }
            foreach (var item in selectList)
            {
                var optionBuilder = new TagBuilder("option");

                optionBuilder.MergeAttribute("value", html.FormatValue(item.Value, ""));
                if (item.Selected)
                    optionBuilder.MergeAttribute("selected", "selected");
                if (item.Disabled)
                    optionBuilder.MergeAttribute("disabled", "disabled");
                optionBuilder.InnerHtml.AppendHtml(item.Text);
                builder.InnerHtml.AppendHtml(optionBuilder);
            }
            return builder;
        }

        public static IHtmlContent ExListBox(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("select");
            builder.MergeAttribute("multiple", "multiple");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttributes(htmlAttributes);
            if (!string.IsNullOrWhiteSpace(optionLabel))
            {
                var option = new TagBuilder("option");
                option.MergeAttribute("value", "");
                option.InnerHtml.AppendHtml(optionLabel);
                builder.InnerHtml.AppendHtml(option);
            }
            foreach (var item in selectList)
            {
                var optionBuilder = new TagBuilder("option");

                optionBuilder.MergeAttribute("value", html.FormatValue(item.Value, ""));
                if (item.Selected)
                    optionBuilder.MergeAttribute("selected", "selected");
                if (item.Disabled)
                    optionBuilder.MergeAttribute("disabled", "disabled");
                optionBuilder.InnerHtml.AppendHtml(item.Text);
                builder.InnerHtml.AppendHtml(optionBuilder);
            }
            return builder;
        }


        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ControlAttributes =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        internal static Dictionary<string, object> Object2Dictionary(this object attribute)
        {
            if (attribute == null)
                return null;
            var properties = ControlAttributes.GetOrAdd(attribute.GetType(), t =>
              {
                  return t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
              });
            return properties.ToDictionary(k => k.Name.ToLower(), v => v.GetValue(attribute));
        }
    }
}
