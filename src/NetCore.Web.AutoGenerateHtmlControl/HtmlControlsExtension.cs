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
        private static IDictionary<string, object> SetGlobalCssClass(this IDictionary<string, object> attributes, string globalCssClass)
        {
            if (string.IsNullOrWhiteSpace(globalCssClass))
                return attributes;
            if (attributes.ContainsKey("class"))
                attributes["class"] = attributes["class"] + " " + globalCssClass;
            else
                attributes["class"] = globalCssClass;
            return attributes;
        }
        public static IHtmlContent ExLabel(this IHtmlHelper html, object text, string format,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("label");
            builder.InnerHtml.AppendHtml(html.FormatValue(text, format));
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }
        public static IHtmlContent Button(this IHtmlHelper html, object value,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "button");
            builder.MergeAttribute("value", html.FormatValue(value, ""));
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }

        public static IHtmlContent ExFile(this IHtmlHelper html, string name,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "file");
            builder.MergeAttribute("name", name);
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }

        public static IHtmlContent ExHidden(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }

        public static IHtmlContent ExTextBox(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "text");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }


        public static IHtmlContent ExPassword(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "password");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", value?.ToString());
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }

        public static IHtmlContent ExTextArea(this IHtmlHelper html, string name, object value, string format,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("textarea");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", html.FormatValue(value, format));
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
            return builder;
        }

        public static IHtmlContent ExDropDownList(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("select");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
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

                optionBuilder.MergeAttribute("value", item.Value);
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
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("select");
            builder.MergeAttribute("multiple", "multiple");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));
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

                optionBuilder.MergeAttribute("value", item.Value);
                if (item.Selected)
                    optionBuilder.MergeAttribute("selected", "selected");
                if (item.Disabled)
                    optionBuilder.MergeAttribute("disabled", "disabled");
                optionBuilder.InnerHtml.AppendHtml(item.Text);
                builder.InnerHtml.AppendHtml(optionBuilder);
            }
            return builder;
        }

        public static IHtmlContent ExRadioButton(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new HtmlContentBuilder();
            foreach (var item in selectList)
            {
                var label = new TagBuilder("label");
                label.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));

                var radioButtonBuilder = new TagBuilder("input");
                radioButtonBuilder.MergeAttribute("type", "radio");
                radioButtonBuilder.MergeAttribute("name", name);
                radioButtonBuilder.MergeAttribute("value", item.Value);
                if (item.Selected)
                    radioButtonBuilder.MergeAttribute("checked", "checked");
                if (item.Disabled)
                    radioButtonBuilder.MergeAttribute("disabled", "disabled");
                label.InnerHtml.AppendHtml(radioButtonBuilder);
                label.InnerHtml.AppendHtml(item.Text);
                builder.AppendHtml(label);
            }
            return builder;
        }

        public static IHtmlContent ExCheckBox(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new HtmlContentBuilder();
            foreach (var item in selectList)
            {
                var label = new TagBuilder("label");
                label.MergeAttributes(htmlAttributes.SetGlobalCssClass(globalCssClass));

                var checkBoxBuilder = new TagBuilder("input");
                checkBoxBuilder.MergeAttribute("type", "checkbox");
                checkBoxBuilder.MergeAttribute("name", name);
                checkBoxBuilder.MergeAttribute("value", item.Value);
                if (item.Selected)
                    checkBoxBuilder.MergeAttribute("checked", "checked");
                if (item.Disabled)
                    checkBoxBuilder.MergeAttribute("disabled", "disabled");
                label.InnerHtml.AppendHtml(checkBoxBuilder);
                label.InnerHtml.AppendHtml(item.Text);
                builder.AppendHtml(label);
            }
            return builder;
        }
    }
}
