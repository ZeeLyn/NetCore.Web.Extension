using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DataListColumnConvertAttribute : Attribute
    {
        public abstract object Convert(object value);
    }
}
