using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    public class UploaderAttribute : FormControlsAttribute
    {
        public UploaderAttribute()
        {
            ControlType = HtmlControlType.Uploader;
        }

        public string ServerUrl { get; set; }

        public string Script { get; set; }
    }
}
