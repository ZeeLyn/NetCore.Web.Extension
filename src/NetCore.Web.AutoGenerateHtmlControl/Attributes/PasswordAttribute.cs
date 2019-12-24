using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : FormControlsAttribute
    {
        public PasswordAttribute()
        {
            ControlType = HtmlControl.Password;
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
