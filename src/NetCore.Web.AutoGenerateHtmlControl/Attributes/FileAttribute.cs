using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FileAttribute : FormControlsAttribute
    {
        public FileAttribute()
        {
            ControlType = HtmlControlType.File;
        }

        private bool _multiple;
        public bool Multiple
        {
            get => _multiple; set
            {
                _multiple = value;
                if (value)
                    AddAttribute(nameof(Multiple), "multiple");
            }
        }
    }
}
