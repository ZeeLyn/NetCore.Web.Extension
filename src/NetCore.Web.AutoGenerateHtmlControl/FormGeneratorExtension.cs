using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class FormGeneratorExtension
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ControlAttributes =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> FormAttributes =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();


        private static Dictionary<string, object> GetAttributeFromObject(this object obj)
        {
            if (obj == null)
                return null;
            var type = obj.GetType();
            var props = FormAttributes.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            return props.ToDictionary(k => k.Name, v => v.GetValue(obj));
        }


        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string url, FormMethod method, TModel model, object htmlAttributes = null, string appendHtmlString = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            return await html.GenerateFormAsync(context, url, method, model, htmlAttributes, string.IsNullOrWhiteSpace(appendHtmlString) ? null : html.Raw(appendHtmlString), antiforgery, globalCssClass);
        }

        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string url, FormMethod method, TModel model, object htmlAttributes = null, Func<IHtmlContent> appendHtmlBuilder = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            var htmlContent = appendHtmlBuilder?.Invoke();
            return await html.GenerateFormAsync(context, url, method, model, htmlAttributes, htmlContent, antiforgery, globalCssClass);
        }


        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string url, FormMethod method, TModel model, object htmlAttributes = null, IHtmlContent appendHtmlContent = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            var serviceProvider = context.RequestServices;
            var options = serviceProvider.GetRequiredService<AutoGenerateFormBuilder>();

            var properties = ControlAttributes.GetOrAdd(model.GetType(), t =>
            {
                return t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
            });

            var form = new TagBuilder("form");
            form.MergeAttribute("method", method.ToString());
            form.MergeAttribute("action", url);
            var attribute = htmlAttributes.GetAttributeFromObject();
            if (attribute != null)
                form.MergeAttributes(attribute, true);

            foreach (var prop in properties)
            {
                var controlAttrs = prop.GetCustomAttributes<FormControlsAttribute>().ToList();
                if (!controlAttrs.Any())
                    continue;
                var name = prop.Name;
                var value = prop.GetValue(model);
                var display = prop.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = display == null ? name : display.DisplayName;

                var group = new TagBuilder("div");
                group.MergeAttribute("class", "form-group");

                var groupName = new TagBuilder("label");
                groupName.InnerHtml.AppendHtml(displayName);
                group.InnerHtml.AppendHtml(groupName);


                var controlContainer = new TagBuilder("div");
                controlContainer.AddCssClass("input-group");
                controlContainer.MergeAttribute("id", $"{name.ToLower()}-input-group");
                foreach (var control in controlAttrs)
                {
                    switch (control.ControlType)
                    {
                        case HtmlControlType.Label:
                            controlContainer.InnerHtml.AppendHtml(html.ExLabel(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.Hidden:
                            controlContainer.InnerHtml.AppendHtml(html.ExHidden(name, value, control.GetAttributes()));
                            break;

                        case HtmlControlType.TextBox:
                            controlContainer.InnerHtml.AppendHtml(html.ExTextBox(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.Password:
                            controlContainer.InnerHtml.AppendHtml(html.ExPassword(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.TextArea:
                            controlContainer.InnerHtml.AppendHtml(html.ExTextArea(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.DropDownList:

                            var dropDownAttr = (DropDownListAttribute)control;
                            var dropDownDataSource = (IDataSource)serviceProvider.GetService(dropDownAttr.DataSource);
                            if (dropDownDataSource == null)
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                break;
                            }
                            controlContainer.InnerHtml.AppendHtml(html.ExDropDownList(name,
                                await dropDownDataSource.GetAsync(new[] { value }), dropDownAttr.OptionLabel,
                                control.GetAttributes(), globalCssClass));

                            break;

                        case HtmlControlType.ListBox:
                            var listBoxAttr = (ListBoxAttribute)control;
                            var listBoxDataSource = (IDataSource)serviceProvider.GetService(listBoxAttr.DataSource);
                            if (listBoxDataSource == null)
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                break;
                            }
                            if (value is IEnumerable)
                            {
                                controlContainer.InnerHtml.AppendHtml(html.ExListBox(name, await listBoxDataSource.GetAsync(value as IEnumerable<object>), listBoxAttr.OptionLabel, control.GetAttributes(), globalCssClass));
                            }
                            else
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml($"ListBox does not support type {value.GetType()}.");
                            }
                            break;

                        case HtmlControlType.RadioButton:
                            var radioButtonAttr = (RadioButtonAttribute)control;
                            var radioButtonDataSource = (IDataSource)serviceProvider.GetService(radioButtonAttr.DataSource);
                            if (radioButtonDataSource == null)
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                break;
                            }
                            controlContainer.InnerHtml.AppendHtml(html.ExRadioButton(name,
                                await radioButtonDataSource.GetAsync(new[] { value }), radioButtonAttr.GetAttributes(),
                                globalCssClass));
                            break;

                        case HtmlControlType.CheckBox:
                            var checkBoxAttr = (CheckBoxAttribute)control;
                            if (value is IEnumerable)
                            {
                                var checkBoxDataSource = (IDataSource)serviceProvider.GetService(checkBoxAttr.DataSource);
                                if (checkBoxDataSource == null)
                                {
                                    controlContainer.MergeAttribute("style", "color:red;");
                                    controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                    break;
                                }
                                controlContainer.InnerHtml.AppendHtml(html.ExCheckBox(name,
                                    await checkBoxDataSource.GetAsync(value as IEnumerable<object>),
                                    checkBoxAttr.GetAttributes(), globalCssClass));
                            }
                            else if (value is bool isChecked)
                            {
                                controlContainer.InnerHtml.AppendHtml(html.ExSingleCheckBox(name, isChecked, checkBoxAttr.GetAttributes(), globalCssClass));
                            }
                            else
                            {

                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml($"CheckBox does not support type {value.GetType()}.");
                            }
                            break;

                        case HtmlControlType.Button:
                            var buttonAttr = (ButtonAttribute)control;
                            controlContainer.InnerHtml.AppendHtml(html.Button(buttonAttr.ButtonText, buttonAttr.GetAttributes(),
                                globalCssClass));
                            break;

                        case HtmlControlType.File:
                            controlContainer.InnerHtml.AppendHtml(html.ExFile(name, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.RichEditor:
                            var editorAttr = (RichEditorAttribute)control;
                            controlContainer.InnerHtml.AppendHtml(await html.ExRichEditor(name, value?.ToString(),
                                string.IsNullOrWhiteSpace(editorAttr.PartialName)
                                    ? options.DefaultRichEditorPartialName
                                    : editorAttr.PartialName, editorAttr.GetAttributes()));
                            break;

                        case HtmlControlType.Uploader:
                            var uploaderAttr = (UploaderAttribute)control;
                            controlContainer.InnerHtml.AppendHtml(await html.Uploader(name, value?.ToString(),
                                string.IsNullOrWhiteSpace(uploaderAttr.ServerUrl)
                                    ? options.UploadServerUrl
                                    : uploaderAttr.ServerUrl,
                                string.IsNullOrWhiteSpace(uploaderAttr.PartialName)
                                    ? options.DefaultUploaderPartialName
                                    : uploaderAttr.PartialName, uploaderAttr.GetAttributes()));
                            break;
                    }

                }

                group.InnerHtml.AppendHtml(controlContainer);
                group.InnerHtml.AppendHtml(html.ValidationMessage(name));
                form.InnerHtml.AppendHtml(group);
            }

            form.InnerHtml.AppendHtml(appendHtmlContent);
            if (antiforgery.HasValue && antiforgery.Value)
                form.InnerHtml.AppendHtml(html.AntiForgeryToken());
            return form;
        }
    }
}
