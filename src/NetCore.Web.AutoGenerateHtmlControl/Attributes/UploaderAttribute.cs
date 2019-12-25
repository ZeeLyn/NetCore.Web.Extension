using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UploaderAttribute : FormControlsAttribute
    {
        public UploaderAttribute()
        {
            ControlType = HtmlControlType.Uploader;
        }

        public string ServerUrl { get; set; }

        public string PartialName { get; set; }
    }
}
