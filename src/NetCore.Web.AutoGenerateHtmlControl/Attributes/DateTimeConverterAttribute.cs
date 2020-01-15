using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeConverterAttribute : DataListColumnConvertAttribute
    {
        public string Format { get; set; } = "yyyy-MM-dd hh:mm";
        public override string Convert(object value)
        {
            return DateTime.Parse(value.ToString()).ToString(Format);
        }
    }
}
