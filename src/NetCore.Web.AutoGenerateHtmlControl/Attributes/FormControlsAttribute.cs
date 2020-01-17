using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class FormControlsAttribute : Attribute
    {
        public string HtmlAttributes { get; set; }

        public bool Hide { get; set; }

        protected readonly Dictionary<string, object> Attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, object> GetAttributes()
        {
            if (string.IsNullOrWhiteSpace(HtmlAttributes)) return Attributes;
            try
            {
                var attrs = JsonConvert.DeserializeObject<Dictionary<string, object>>(HtmlAttributes);
                if (attrs == null) return Attributes;
                foreach (var attr in attrs)
                    Attributes[attr.Key] = attr.Value;
            }
            catch
            {
                throw new FormatException(HtmlAttributes);
            }
            return Attributes;
        }
        protected void AddAttribute(string name, object value)
        {
            Attributes[name.ToLower()] = value;
        }

        public HtmlControlType ControlType { get; protected internal set; }
    }


}
