using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayOrderAttribute : Attribute
    {
        public DisplayOrderAttribute(int orderNumber = 0)
        {
            OrderNumber = orderNumber;
        }

        public int OrderNumber { get; set; }
    }
}
