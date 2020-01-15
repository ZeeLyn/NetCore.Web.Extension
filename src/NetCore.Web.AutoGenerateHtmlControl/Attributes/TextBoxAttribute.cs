using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TextBoxAttribute : FormControlsAttribute
    {
        public TextBoxAttribute()
        {
            ControlType = HtmlControlType.TextBox;
        }

        private string _placeholder;
        public string Placeholder
        {
            get => _placeholder; set
            {
                _placeholder = value;
                AddAttribute("placeholder", value);
            }
        }
    }
}
