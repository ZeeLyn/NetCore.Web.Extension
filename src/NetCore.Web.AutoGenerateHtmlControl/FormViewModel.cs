using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public class FormViewModel
    {
        public FormOptions FormOptions { get; set; }

        public List<FormItem> FormItems { get; set; }

        public string GlobalCssClass { get; set; }

        public IHtmlContent AppendHtmlContent { get; set; }
    }

    public class FormItem
    {
        public string DisplayName { get; set; }

        public IEnumerable<FormControlsAttribute> Controls { get; set; }

        public object Value { get; set; }

        public string Name { get; set; }
    }

    public class FormOptions
    {
        public string ActionName { get; set; }

        public string ControllerName { get; set; }

        public object RouteValues { get; set; }

        public FormMethod Method { get; set; }

        public bool? Antiforgery { get; set; }

        public object HtmlAttributes { get; set; }
    }
}
