using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RichEditorAttribute : FormControlsAttribute
    {
        public RichEditorAttribute()
        {
            ControlType = HtmlControlType.RichEditor;
        }

        public string PartialName { get; set; }
    }
}
