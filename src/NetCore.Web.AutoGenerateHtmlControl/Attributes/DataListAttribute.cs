using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataListAttribute : Attribute
    {
        public string HtmlAttribute { get; set; }
    }
}
