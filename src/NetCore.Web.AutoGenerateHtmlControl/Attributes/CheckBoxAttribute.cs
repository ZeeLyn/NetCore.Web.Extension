using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CheckBoxAttribute : FormControlsAttribute
    {
        public CheckBoxAttribute()
        {
            ControlType = HtmlControlType.CheckBox;
        }

        public Type DataSource { get; set; }

        public IEnumerable<SelectListItem> GetDataSource()
        {
            return null;
        }
    }
}
