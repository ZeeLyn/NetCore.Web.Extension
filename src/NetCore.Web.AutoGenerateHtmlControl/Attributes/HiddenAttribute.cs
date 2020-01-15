using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HiddenAttribute : FormControlsAttribute
    {
        public HiddenAttribute()
        {
            ControlType = HtmlControlType.Hidden;
        }
    }
}
