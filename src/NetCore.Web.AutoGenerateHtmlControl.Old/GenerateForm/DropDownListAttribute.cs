﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCore.Web.AutoGenerateHtmlControl.GenerateForm
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DropDownListAttribute : FormControlsAttribute
    {
        public DropDownListAttribute()
        {
            ControlType = HtmlControl.DropDownList;
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
