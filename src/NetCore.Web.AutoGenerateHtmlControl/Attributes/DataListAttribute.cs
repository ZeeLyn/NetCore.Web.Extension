using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataListAttribute : Attribute
    {
        public string HtmlAttribute { get; set; }
    }
}
