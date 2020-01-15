using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DataListColumnConvertAttribute : Attribute
    {

        public abstract string Convert(object value);
    }
}
