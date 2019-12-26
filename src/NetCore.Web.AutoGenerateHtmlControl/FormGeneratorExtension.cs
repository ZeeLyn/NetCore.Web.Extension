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


        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, string appendHtmlString = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            return await html.GenerateFormAsync(context, actionName, controllerName, routeValues, method, model, htmlAttributes, string.IsNullOrWhiteSpace(appendHtmlString) ? null : html.Raw(appendHtmlString), antiforgery, globalCssClass);
        }

        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, Func<IHtmlContent> appendHtmlBuilder = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            var htmlContent = appendHtmlBuilder?.Invoke();
            return await html.GenerateFormAsync(context, actionName, controllerName, routeValues, method, model, htmlAttributes, htmlContent, antiforgery, globalCssClass);
        }


        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, IHtmlContent appendHtmlContent = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            var serviceProvider = context.RequestServices;
            var options = serviceProvider.GetRequiredService<AutoGenerateFormBuilder>();
            var view = new FormViewModel
            {
                AppendHtmlContent = appendHtmlContent,
                GlobalCssClass = globalCssClass,
                FormOptions = new FormOptions
                {
                    ActionName = actionName,
                    ControllerName = controllerName,
                    Method = method,
                    Antiforgery = antiforgery,
                    HtmlAttributes = htmlAttributes,
                    RouteValues = routeValues
                },
                FormItems = new List<FormItem>()
            };

            var properties = ControlAttributes.GetOrAdd(model.GetType(), t =>
            {
                return t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
            });


            var form = new TagBuilder("form");


            foreach (var prop in properties)
            {
                var controlAttrs = prop.GetCustomAttributes<FormControlsAttribute>().ToList();
                if (!controlAttrs.Any())
                    continue;
                var name = prop.Name;
                var value = prop.GetValue(model);
                var display = prop.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = display == null ? prop.Name : display.DisplayName;
                view.FormItems.Add(new FormItem
                {
                    DisplayName = display == null ? prop.Name : display.DisplayName,
                    Value = prop.GetValue(model),
                    Name = prop.Name,
                    Controls = controlAttrs
                });

                var group = new TagBuilder("div");
                group.MergeAttribute("class", "form-group");

                var groupName = new TagBuilder("label");
                groupName.InnerHtml.AppendHtml(name);
                group.InnerHtml.AppendHtml(groupName);
                foreach (var control in controlAttrs)
                {
                    switch (control.ControlType)
                    {
                        case HtmlControlType.Label:
                            group.InnerHtml.AppendHtml(html.ExLabel(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.Hidden:
                            group.InnerHtml.AppendHtml(html.ExHidden(name, value, control.GetAttributes()));
                            break;

                        case HtmlControlType.TextBox:
                            group.InnerHtml.AppendHtml(html.ExTextBox(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.Password:
                            group.InnerHtml.AppendHtml(html.ExPassword(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.TextArea:
                            group.InnerHtml.AppendHtml(html.ExTextArea(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.DropDownList:
                            var dropDownAttr = (DropDownListAttribute)control;
                            var dropDownDataSource = (IDataSource)serviceProvider.GetRequiredService(dropDownAttr.DataSource);
                            group.InnerHtml.AppendHtml(html.ExDropDownList(name,
                                await dropDownDataSource.GetAsync(new[] { value }), dropDownAttr.OptionLabel,
                                control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.ListBox:
                            var listBoxAttr = (ListBoxAttribute)control;
                            var listBoxDataSource = (IDataSource)serviceProvider.GetRequiredService(listBoxAttr.DataSource);
                            if (value is IEnumerable)
                            {
                                group.InnerHtml.AppendHtml(html.ExListBox(name, await listBoxDataSource.GetAsync(value as IEnumerable<object>), listBoxAttr.OptionLabel, control.GetAttributes(), globalCssClass));
                            }
                            else
                            {
                                throw new NotSupportedException($"ListBox does not support type {value.GetType()}.");
                            }
                            break;

                        case HtmlControlType.RadioButton:
                            var radioButtonAttr = (RadioButtonAttribute)control;
                            var radioButtonDataSource = (IDataSource)serviceProvider.GetRequiredService(radioButtonAttr.DataSource);
                            group.InnerHtml.AppendHtml(html.ExRadioButton(name,
                                await radioButtonDataSource.GetAsync(new[] { value }), radioButtonAttr.GetAttributes(),
                                globalCssClass));
                            break;

                        case HtmlControlType.CheckBox:
                            var checkBoxAttr = (CheckBoxAttribute)control;
                            var checkBoxDataSource = (IDataSource)serviceProvider.GetRequiredService(checkBoxAttr.DataSource);
                            if (value is IEnumerable)
                            {
                                group.InnerHtml.AppendHtml(html.ExCheckBox(name,
                                    await checkBoxDataSource.GetAsync(value as IEnumerable<object>),
                                    checkBoxAttr.GetAttributes(), globalCssClass));
                            }
                            else
                            {
                                throw new NotSupportedException($"CheckBox does not support type {value.GetType()}.");
                            }
                            break;

                        case HtmlControlType.Button:
                            var buttonAttr = (ButtonAttribute)control;
                            group.InnerHtml.AppendHtml(html.Button(buttonAttr.ButtonText, buttonAttr.GetAttributes(),
                                globalCssClass));
                            break;

                        case HtmlControlType.File:
                            group.InnerHtml.AppendHtml(html.ExFile(name, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.RichEditor:
                            var editorAttr = (RichEditorAttribute)control;
                            group.InnerHtml.AppendHtml(await html.ExRichEditor(name, value?.ToString(),
                                string.IsNullOrWhiteSpace(editorAttr.PartialName)
                                    ? options.DefaultRichEditorPartialName
                                    : editorAttr.PartialName, editorAttr.GetAttributes()));
                            break;

                        case HtmlControlType.Uploader:
                            var uploaderAttr = (UploaderAttribute)control;
                            group.InnerHtml.AppendHtml(await html.Uploader(name, value?.ToString(),
                                string.IsNullOrWhiteSpace(uploaderAttr.ServerUrl)
                                    ? options.UploadServerUrl
                                    : uploaderAttr.ServerUrl,
                                string.IsNullOrWhiteSpace(uploaderAttr.PartialName)
                                    ? options.DefaultUploaderPartialName
                                    : uploaderAttr.PartialName, uploaderAttr.GetAttributes()));
                            break;
                    }

                }

                group.InnerHtml.AppendHtml(html.ValidationMessage(name));
                form.InnerHtml.AppendHtml(group);
            }
            if (antiforgery.HasValue && antiforgery.Value)
                form.InnerHtml.AppendHtml(html.AntiForgeryToken());
            return form;
        }
    }
}
