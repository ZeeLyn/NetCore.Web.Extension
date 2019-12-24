using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCore.Web.AutoGenerateHtmlControl.GenerateForm
{
    public class ListBoxAttribute : FormControlsAttribute
    {
        public ListBoxAttribute()
        {
            ControlType = HtmlControl.ListBox;
        }
        public Type DataSource { get; set; }

        public IEnumerable<SelectListItem> GetDataSource()
        {
            return null;
        }


        public string OptionLabel
        {
            get;
            set;
        }
    }
}
