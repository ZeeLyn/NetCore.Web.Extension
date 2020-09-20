using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeConverterAttribute : DataListColumnConvertAttribute
    {
        public DateTimeConverterAttribute()
        {
        }

        public DateTimeConverterAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public override object Convert(object value)
        {
            return DateTime.Parse(value.ToString()).ToString(Format);
        }
    }
}
