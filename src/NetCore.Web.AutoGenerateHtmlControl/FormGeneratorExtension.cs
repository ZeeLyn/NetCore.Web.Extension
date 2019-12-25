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
        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, string actionName, string controllerName, object routeValues, FormMethod method, TModel model, object htmlAttributes = null, bool antiforgery = default, string globalCssClass = "form-control")
        {

            var view = new FormViewModel
            {
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
