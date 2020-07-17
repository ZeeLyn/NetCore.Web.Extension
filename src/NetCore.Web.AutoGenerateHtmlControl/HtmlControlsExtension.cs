using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class HtmlControlsExtension
    {
        public static IHtmlContent ExLabel(this IHtmlHelper html, string name, object text,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("label");
            builder.MergeAttribute("for", name);
            builder.InnerHtml.AppendHtml(text?.ToString());
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }
        public static IHtmlContent Button(this IHtmlHelper html, object value,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "button");
            builder.MergeAttribute("value", value?.ToString());
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }

        public static IHtmlContent ExFile(this IHtmlHelper html, string name,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "file");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }

        public static IHtmlContent ExHidden(this IHtmlHelper html, string name, object value,
            IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", value?.ToString());
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }

        public static IHtmlContent ExTextBox(this IHtmlHelper html, string name, object value,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "text");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", value?.ToString());
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }


        public static IHtmlContent ExPassword(this IHtmlHelper html, string name, object value,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "password");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            builder.MergeAttribute("value", value?.ToString());
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }

        public static IHtmlContent ExTextArea(this IHtmlHelper html, string name, object value,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("textarea");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            //builder.MergeAttribute("value", value?.ToString());
            builder.InnerHtml.SetContent(value?.ToString());
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
            return builder;
        }

        public static IHtmlContent ExDropDownList(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new TagBuilder("select");
            builder.MergeAttribute("name", name);
            builder.MergeAttribute("id", name);
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
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
            if (!string.IsNullOrWhiteSpace(globalCssClass))
                builder.MergeAttribute("class", globalCssClass);
            builder.MergeAttributes(htmlAttributes, true);
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
                var container = new TagBuilder("div");
                container.MergeAttribute("class", "radio");
                var label = new TagBuilder("label");
                if (!string.IsNullOrWhiteSpace(globalCssClass))
                    label.MergeAttribute("class", globalCssClass);
                label.MergeAttributes(htmlAttributes, true);

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
                container.InnerHtml.AppendHtml(label);
                builder.AppendHtml(container);
            }
            return builder;
        }

        public static IHtmlContent ExCheckBox(this IHtmlHelper html, string name, IEnumerable<SelectListItem> selectList,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new HtmlContentBuilder();
            foreach (var item in selectList)
            {
                var container = new TagBuilder("div");
                container.MergeAttribute("class", "checkbox");
                var label = new TagBuilder("label");
                var checkBoxBuilder = new TagBuilder("input");
                checkBoxBuilder.MergeAttribute("type", "checkbox");
                checkBoxBuilder.MergeAttribute("name", name);
                checkBoxBuilder.MergeAttribute("value", item.Value);
                if (item.Selected)
                    checkBoxBuilder.MergeAttribute("checked", "checked");
                if (item.Disabled)
                    checkBoxBuilder.MergeAttribute("disabled", "disabled");
                if (!string.IsNullOrWhiteSpace(globalCssClass))
                    checkBoxBuilder.MergeAttribute("class", globalCssClass);
                checkBoxBuilder.MergeAttributes(htmlAttributes, true);

                label.InnerHtml.AppendHtml(checkBoxBuilder);
                label.InnerHtml.AppendHtml(item.Text);
                container.InnerHtml.AppendHtml(label);
                builder.AppendHtml(container);
            }
            return builder;
        }

        public static IHtmlContent ExSingleCheckBox(this IHtmlHelper html, string name, bool isChecked,
            IDictionary<string, object> htmlAttributes, string globalCssClass = "")
        {
            var builder = new HtmlContentBuilder();
            var container = new TagBuilder("div");
            container.MergeAttribute("class", "checkbox");
            var label = new TagBuilder("label");
            var checkBoxBuilder = new TagBuilder("input");
            checkBoxBuilder.MergeAttribute("type", "checkbox");
            checkBoxBuilder.MergeAttribute("name", name);
            checkBoxBuilder.MergeAttribute("value", "true");
            if (isChecked)
                checkBoxBuilder.MergeAttribute("checked", "checked");

            if (!string.IsNullOrWhiteSpace(globalCssClass))
                checkBoxBuilder.MergeAttribute("class", globalCssClass);
            checkBoxBuilder.MergeAttributes(htmlAttributes, true);

            label.InnerHtml.AppendHtml(checkBoxBuilder);
            //label.InnerHtml.AppendHtml(name);
            container.InnerHtml.AppendHtml(label);
            builder.AppendHtml(container);

            return builder;
        }

        public static async Task<IHtmlContent> ExRichEditor(this IHtmlHelper html, string name, string value, string partialName, IDictionary<string, object> htmlAttributes)
        {
            if (!string.IsNullOrWhiteSpace(partialName))
                return await html.PartialAsync(partialName, new RichEditorContext { Name = name, Value = value, HtmlAttributes = htmlAttributes });
            var container = new TagBuilder("div");
            container.MergeAttribute("class", "editor-container");
            var textArea = new TagBuilder("textarea");
            textArea.MergeAttribute("id", name);
            textArea.MergeAttribute("name", name);
            textArea.MergeAttribute("style", "display:none");
            textArea.InnerHtml.AppendHtml(value);
            container.InnerHtml.AppendHtml(textArea);
            return container;
        }

        public static async Task<IHtmlContent> Uploader(this IHtmlHelper html, string name, object value, string partialName, IDictionary<string, object> htmlAttributes, Dictionary<string, string> extensionArgs = default)
        {
            if (!string.IsNullOrWhiteSpace(partialName))
                return await html.PartialAsync(partialName, new UploaderContext { Name = name, Value = value, HtmlAttributes = htmlAttributes });
            var container = new TagBuilder("div");
            container.MergeAttribute("class", "uploader-container");
            container.MergeAttribute("id", $"{name}-container");
            container.MergeAttribute("data-form-name", name);
            container.MergeAttributes(htmlAttributes, true);

            var tips = string.Empty;
            extensionArgs?.TryGetValue("Tips", out tips);
            var uploadBtnText = string.Empty;
            extensionArgs?.TryGetValue("UploadBtnText", out uploadBtnText);
            var auto = string.Empty;
            extensionArgs?.TryGetValue("auto", out auto);
            container.InnerHtml.AppendHtml("<div class=\"queueList\"><div class=\"placeholder\"><div class=\"filePicker btn btn-primary btn-sm\"></div><div>" + tips + "</div></div><ul class=\"filelist\"></ul></div><div class=\"statusBar\" style=\"display: none;\"><div class=\"progress\"><span class=\"text\">0%</span><span class=\"percentage\"></span></div><div class=\"info\"></div><div class=\"btns\"><div class=\"footer-add-btn btn btn-secondary btn-sm\"></div>" + (auto?.ToLower() == "true" ? "" : "<div class=\"uploadBtn btn btn-primary btn-sm disabled\">" + uploadBtnText + "</div>") + "</div></div>");
            return container;
        }
    }
}
