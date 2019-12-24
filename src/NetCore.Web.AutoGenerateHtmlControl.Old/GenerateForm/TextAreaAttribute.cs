using System;

namespace NetCore.Web.AutoGenerateHtmlControl.GenerateForm
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class TextAreaAttribute : FormControlsAttribute
    {
        public TextAreaAttribute()
        {
            ControlType = HtmlControl.TextArea;
        }

        private int _rows;

        public int Rows
        {
            get => _rows;

            set
            {
                _rows = value;
                AddAttribute(nameof(Rows), value);
            }
        }

        private string _placeholder;
        public string Placeholder
        {
            get => _placeholder; set
            {
                _placeholder = value;
                AddAttribute(nameof(Placeholder), value);
            }
        }
    }
}
