using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ButtonAttribute : FormControlsAttribute
    {
        public ButtonAttribute()
        {
            ControlType = HtmlControl.Button;
        }

        public string ButtonText { get; set; } = "Button";
    }
}
