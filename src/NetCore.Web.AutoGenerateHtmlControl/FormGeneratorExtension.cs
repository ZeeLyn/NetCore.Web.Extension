using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class FormGeneratorExtension
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ControlAttributes =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();


        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, string appendHtmlString = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            return await html.GenerateFormAsync(actionName, controllerName, routeValues, method, model, htmlAttributes, string.IsNullOrWhiteSpace(appendHtmlString) ? null : html.Raw(appendHtmlString), antiforgery, globalCssClass);
        }

        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, Func<IHtmlContent> appendHtmlBuilder = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            var htmlContent = appendHtmlBuilder?.Invoke();
            return await html.GenerateFormAsync(actionName, controllerName, routeValues, method, model, htmlAttributes, htmlContent, antiforgery, globalCssClass);
        }


        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, IHtmlContent appendHtmlContent = null, bool? antiforgery = default, string globalCssClass = "form-control")
        {
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

            foreach (var prop in properties)
            {
                var controlAttrs = prop.GetCustomAttributes<FormControlsAttribute>().ToList();
                if (!controlAttrs.Any())
                    continue;

                var display = prop.GetCustomAttribute<DisplayNameAttribute>();
                view.FormItems.Add(new FormItem
                {
                    DisplayName = display == null ? prop.Name : display.DisplayName,
                    Value = prop.GetValue(model),
                    Name = prop.Name,
                    Controls = controlAttrs
                });
            }

            return await html.PartialAsync("___FormGeneratePartial___", view);
        }




    }
}
