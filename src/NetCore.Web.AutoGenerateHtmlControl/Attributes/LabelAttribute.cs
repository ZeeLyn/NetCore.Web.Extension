using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LabelAttribute : FormControlsAttribute
    {
        public LabelAttribute()
        {
            ControlType = HtmlControlType.Label;
        }
    }
}
